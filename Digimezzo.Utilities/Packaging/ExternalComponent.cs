namespace Digimezzo.Utilities.Packaging
{
    public class ExternalComponent
    {
        #region Variables
        private string name;
        private string description;
        private string url;
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        #endregion
    }
}
