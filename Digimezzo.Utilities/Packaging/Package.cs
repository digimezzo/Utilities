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
                return string.Format("{0} {1} {2}", this.name, this.version.ToString(), this.Label);
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
