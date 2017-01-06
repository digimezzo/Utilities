using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Digimezzo.Utilities.Packaging
{
    public class PackageCreator
    {
        #region Variables
        // Configuration
        private XDocument packagerDoc;
        private Package package;
        private string installablePackageName;
        private string portablePackageName;
        private string updatePackageName;

        // Local
        private string currentDirectory;
        private string packageDirectory;

        // Remote
        // Requires directory structure on the server: content/software/<ApplicationName>/releases/.update
        // Requires directory structure on the server: content/software/<ApplicationName>/previews/.update
        private string publishDirectory = ""; // Filled in during Initialize()
        private string publishUpdateSubDirectory = ".update";
        private string versionsFileName = "versions.dat";
        private string versionsFileSubDirectory = "content/software";
        #endregion

        #region Construction
        public PackageCreator(string packageName, Version packageVersion, Configuration configuration)
        {
            this.package = new Package(packageName, packageVersion, configuration);
        }
        #endregion

        #region Public
        public void Execute()
        {
            // Initialize
            // ----------
            this.Initialize();

            Console.WriteLine("Packager");
            Console.WriteLine("========");

            Console.WriteLine(Environment.NewLine + string.Format("Welcome to the packager for {0} {1}", this.package.Filename, this.package.FormattedVersion));
            Console.WriteLine(Environment.NewLine + "Press any key to start packaging...");

            Console.ReadKey();

            // Clean up the destination directory
            // ----------------------------------
            foreach (string f in Directory.GetFiles(this.packageDirectory))
            {
                File.Delete(f);
            }

            // Create the installable version
            // ------------------------------
            Task createInstallableVersionTask = this.CreateInstallableVersionAsync();
            createInstallableVersionTask.Wait();

            // Create the update package
            // -------------------------
            Task createUpdatePackageTask = this.CreateUpdatePackageAsync();
            createUpdatePackageTask.Wait();

            // Create the portable version
            // ---------------------------
            Task createPortableVersionTask = this.CreatePortableVersionAsync();
            createPortableVersionTask.Wait();

            // Do you wish to publish this package?
            // ------------------------------------
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Environment.NewLine + Environment.NewLine + "Do you wish to publish this package? [Y/N]");

            ConsoleKeyInfo info = Console.ReadKey();

            if (info.Key == ConsoleKey.Y)
            {
                Console.Write(Environment.NewLine + Environment.NewLine + "Please provide the publishing FTP Server:");
                string server = Console.ReadLine();

                Console.Write(Environment.NewLine + Environment.NewLine + "Please provide the publishing FTP Port:");
                string port = Console.ReadLine();

                Console.Write(Environment.NewLine + Environment.NewLine + "Please provide the publishing username:");
                string username = Console.ReadLine();

                Console.Write(Environment.NewLine + Environment.NewLine + "Please provide the publishing password:");
                string password = Console.ReadLine();

                this.PublishPackage(server, port, username, password);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(Environment.NewLine + Environment.NewLine + "Package published");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(Environment.NewLine + Environment.NewLine + "Package not published");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Environment.NewLine + Environment.NewLine + "Press any key to close this window and go to the package directory");

            Console.ReadKey();

            Process.Start(@"explorer.exe", @"/select, """ + this.packageDirectory + @"""");

            Console.ReadKey();
        }
        #endregion

        #region Private
        private void Initialize()
        {
            this.publishDirectory = string.Format("content/software/{0}", this.package.Name.ToLower());

            this.installablePackageName = this.package.Filename + this.package.InstallableFileExtension;
            this.portablePackageName = this.package.Filename + " - Portable" + this.package.PortableFileExtension;
            this.updatePackageName = this.package.Filename + this.package.UpdateFileExtension;

            this.packagerDoc = XDocument.Load("PackagerConfiguration.xml");

            this.packageDirectory = (from p in this.packagerDoc.Element("Packager").Element("Packaging").Elements("PackageDirectory")
                                     select p.Value).FirstOrDefault();

            if (!Directory.Exists(this.packageDirectory)) Directory.CreateDirectory(this.packageDirectory);

            this.currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        async Task CreateInstallableVersionAsync()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(Environment.NewLine + " - Creating installable version");

            bool success = true;

            await Task.Run(() =>
            {
                try
                {
                    // Delete all installable files if they exist
                    foreach (FileInfo f in new DirectoryInfo(this.currentDirectory).GetFiles(@"*" + this.package.InstallableFileExtension))
                    {
                        f.Delete();
                    }

                    // Delete all portable files if they exist
                    foreach (FileInfo f in new DirectoryInfo(this.currentDirectory).GetFiles(@"*" + this.package.PortableFileExtension))
                    {
                        f.Delete();
                    }

                    // Make sure we're not in portable mode
                    this.SetPortableMode(false);


                    // Get the bin directory for the WIX runtimes

                    var wixBinDirectory = (from p in this.packagerDoc.Element("Packager").Element("Packaging").Element("Installable").Elements("WixBinDirectory")
                                           select p.Value).FirstOrDefault();

                    // Create the .bat file for WIX
                    if (File.Exists("CreateMsiInstaller.bat"))
                    {
                        File.Delete("CreateMsiInstaller.bat");
                    }

                    using (TextWriter writer = File.CreateText("CreateMsiInstaller.bat"))
                    {
                        writer.WriteLine(@"DEL *.wixobj");
                        writer.WriteLine(@"DEL *.wixpdb");
                        writer.WriteLine(@"DEL *" + this.package.InstallableFileExtension);
                        writer.WriteLine(@"DEL *" + this.package.PortableFileExtension);
                        writer.WriteLine(@"""" + wixBinDirectory + @"\candle.exe"" *.wxs");
                        writer.WriteLine(String.Format(@"""" + wixBinDirectory + @"\light.exe"" -ext WixUIExtension -ext WixUtilExtension -out ""{0}"" *.wixobj", this.installablePackageName));
                        writer.WriteLine("PAUSE");
                    }

                    Process.Start("CreateMsiInstaller.bat");

                    // Wait until the installable file is created
                    while (!File.Exists(this.installablePackageName))
                    {
                        Task.Delay(1000);
                    }

                    // Copy the installable version to the destination directory (this is a loop because the files can be in use by the .bat file)
                    bool copySuccess = false;

                    while (!copySuccess)
                    {
                        try
                        {
                            File.Copy(this.installablePackageName, Path.Combine(this.packageDirectory, this.installablePackageName), true);
                            copySuccess = true;
                        }
                        catch (Exception)
                        {
                            copySuccess = false;
                        }
                        Task.Delay(1000);
                    }
                }
                catch (Exception)
                {
                    success = false;
                }
            });

            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\tOK");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\tERROR");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private void SetPortableMode(bool isPortable)
        {
            XDocument baseSettingsDoc = XDocument.Load("BaseSettings.xml");

            var isPortableElement = (from n in baseSettingsDoc.Element("Settings").Elements("Namespace")
                                     from s in n.Elements("Setting")
                                     from v in s.Elements("Value")
                                     where n.Attribute("Name").Value.Equals("Configuration") & s.Attribute("Name").Value.Equals("IsPortable")
                                     select v).FirstOrDefault();

            isPortableElement.Value = isPortable.ToString();
            baseSettingsDoc.Save("BaseSettings.xml");
        }

        async Task CreateUpdatePackageAsync()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(Environment.NewLine + " - Creating update package");

            bool success = true;

            await Task.Run(() =>
            {
                try
                {
                    // Make sure that the package doesn't exist, otherwise the next "using" throws an exception.
                    if (File.Exists(this.updatePackageName)) File.Delete(this.updatePackageName);

                    // Create package containing the installable file
                    using (ZipArchive archive = ZipFile.Open(this.updatePackageName, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(this.installablePackageName, this.installablePackageName);
                    }

                    // Copy the update package to the destination directory (this is a loop because the files can be in use by the .bat file)
                    bool copySuccess = false;

                    while (!copySuccess)
                    {
                        try
                        {
                            File.Copy(this.updatePackageName, Path.Combine(this.packageDirectory, this.updatePackageName), true);
                            copySuccess = true;
                        }
                        catch (Exception ex)
                        {
                            copySuccess = false;
                        }

                        Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                }
            });

            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\tOK");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\tERROR");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        async Task CreatePortableVersionAsync()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(Environment.NewLine + " - Creating portable version");

            bool success = true;

            await Task.Run(() =>
            {
                try
                {
                    // Delete all portable files if they exist
                    foreach (FileInfo f in new DirectoryInfo(this.currentDirectory).GetFiles(@"*" + this.package.PortableFileExtension))
                    {
                        f.Delete();
                    }

                    // Make sure we're in portable mode
                    this.SetPortableMode(true);

                    // Make sure that the package doesn't exist, otherwise the next "using" throws an exception.
                    if (File.Exists(this.portablePackageName)) File.Delete(this.portablePackageName);

                    // Create the portable file
                    using (ZipArchive archive = ZipFile.Open(this.portablePackageName, ZipArchiveMode.Create))
                    {
                        // Add directories
                        List<string> directories = (from p in this.packagerDoc.Element("Packager").Element("Packaging").Element("Portable").Element("Directories").Elements("Directory")
                                                    select p.Value).ToList();

                        foreach (string d in directories)
                        {
                            var di = new DirectoryInfo(d);
                            FileInfo[] fi = di.GetFiles();

                            foreach (FileInfo f in fi)
                            {
                                archive.CreateEntryFromFile(f.FullName, f.Directory.Name +"/"+ f.Name);
                            }
                        }

                        // Add files
                        List<string> files = (from p in this.packagerDoc.Element("Packager").Element("Packaging").Element("Portable").Element("Files").Elements("File")
                                              select p.Value).ToList();

                        foreach (string f in files)
                        {
                            archive.CreateEntryFromFile(f, f);
                        }
                    }

                    File.Copy(this.portablePackageName, Path.Combine(this.packageDirectory, this.portablePackageName), true);
                }
                catch (Exception)
                {
                    success = false;
                }
            });

            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\tOK");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\tERROR");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private void PublishPackage(string server, string port, string username, string password)
        {
            string subdirectory = this.package.Configuration == Configuration.Debug ? "previews" : "releases";

            // Upload to FTP Server
            // --------------------
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);

                // Upload Installable package
                client.UploadFile(String.Format("ftp://{0}:{1}/{2}/{3}/{4}",
                                                server,
                                                port,
                                                this.publishDirectory,
                                                subdirectory,
                                                this.installablePackageName),
                                  "STOR",
                                  this.installablePackageName);

                // Upload Portable package
                client.UploadFile(String.Format("ftp://{0}:{1}/{2}/{3}/{4}",
                                                server,
                                                port,
                                                this.publishDirectory,
                                                subdirectory,
                                                this.portablePackageName),
                                  "STOR",
                                  this.portablePackageName);

                // Upload Update package
                client.UploadFile(String.Format("ftp://{0}:{1}/{2}/{3}/{4}/{5}",
                                               server,
                                               port,
                                               this.publishDirectory,
                                               subdirectory,
                                               this.publishUpdateSubDirectory,
                                               this.updatePackageName),
                                 "STOR",
                                 this.updatePackageName);

                // Download versions file
                client.DownloadFile(String.Format("ftp://{0}:{1}/{2}/{3}",
                                                  server,
                                                  port,
                                                  this.versionsFileSubDirectory,
                                                  this.versionsFileName),
                                    this.versionsFileName);

                // Update versions file
                XDocument versionsDoc = XDocument.Load(this.versionsFileName);

                XElement applicationElement = (from a in versionsDoc.Element("Applications").Elements("Application")
                                               where a.Attribute("Name").Value.ToLower().Equals(this.package.Name.ToLower())
                                               select a).FirstOrDefault();

                if (applicationElement != null)
                {
                    var versionElement = new XElement("Version", this.package.Version.ToString());
                    versionElement.SetAttributeValue("Configuration", Convert.ToInt32(this.package.Configuration));

                    applicationElement.Add(versionElement);
                }

                versionsDoc.Save(this.versionsFileName);

                // Upload versions file
                client.UploadFile(String.Format("ftp://{0}:{1}/{2}/{3}",
                                               server,
                                               port,
                                               this.versionsFileSubDirectory,
                                               this.versionsFileName),
                                 "STOR",
                                 this.versionsFileName);
            }
        }
        #endregion
    }
}
