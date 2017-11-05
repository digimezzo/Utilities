using System;

namespace Digimezzo.Utilities.Packaging
{
    public class Package
    {
        #region Properties
        private string name { get; set; }
        private Version version { get; set; }
        private Configuration configuration { get; set; }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Version Version
        {
            get
            {
                return this.version;
            }
        }

        public string UnformattedVersion
        {
            get
            {
                //  {0}: Major Version,
                //  {1}: Minor Version,
                //  {2}: Build Number,
                //  {3}: Revision
                return string.Format("{0}.{1}.{2}.{3}", this.version.Major, this.version.Minor, this.version.Build, this.version.Revision);
            }
        }

        public string FormattedVersion
        {
            get
            {
                //  {0}: Major Version,
                //  {1}: Minor Version,
                //  {2}: Build Number,
                //  {3}: Revision

                if (this.version.Build != 0)
                {
                    return string.Format("{0}.{1}.{2} (Build {3})", this.version.Major, this.version.Minor, this.version.Build, this.version.Revision);
                }
                else
                {
                    return string.Format("{0}.{1} (Build {2})", this.version.Major, this.version.Minor, this.version.Revision);
                }
            }
        }

        public string FormattedVersionNoBuild
        {
            get
            {
                //  {0}: Major Version,
                //  {1}: Minor Version,
                //  {2}: Build Number,
                //  {3}: Revision

                if (this.version.Build != 0)
                {
                    return string.Format("{0}.{1}.{2}", this.version.Major, this.version.Minor, this.version.Build, this.version.Revision);
                }
                else
                {
                    return string.Format("{0}.{1}", this.version.Major, this.version.Minor, this.version.Revision);
                }
            }
        }

        public Configuration Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        public string Label
        {
            get { return this.configuration == Configuration.Debug ? Constants.Preview : Constants.Release; }
        }

        public string Filename
        {
            get
            {
                string filename = string.Format("{0} {1}", this.name, this.version.ToString());

                if (this.Configuration == Configuration.Debug)
                {
                    filename += " " + Constants.Preview;
                }
                return filename;
            }
        }

        public string InstallableFileExtension
        {
            get
            {
                return ".msi";
            }
        }

        public string PortableFileExtension
        {
            get
            {
                return ".zip";
            }
        }

        public string UpdateFileExtension
        {
            get
            {
                return ".update";
            }
        }
        #endregion

        #region Construction
        public Package(string name, Version version, Configuration configuration)
        {
            this.name = name;
            this.version = version;
            this.configuration = configuration;
        }
        #endregion

        #region Public
        public bool IsOlder(Package referencePackage)
        {
            return this.version < referencePackage.version;
        }
        #endregion
    }
}
