using System;
using System.Text.RegularExpressions;

namespace Digimezzo.Utilities.IO
{
    public class FileInformation
    {
        #region Variables
        private string path;
        #endregion

        #region Construction
        public FileInformation(string path)
        {
            this.path = path;
        }
        #endregion

        #region Readonly Properties
        public string Folder
        {
            get { return System.IO.Path.GetDirectoryName(this.path); }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(this.path); }
        }

        public string NameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(this.path); }
        }

        public long SizeInBytes
        {
            get { return new System.IO.FileInfo(this.path).Length; }
        }

        public DateTime DateModified
        {
            get { return new System.IO.FileInfo(this.path).LastWriteTime; }
        }

        public long DateModifiedTicks
        {
            get { return new System.IO.FileInfo(this.path).LastWriteTime.Ticks; }
        }
        #endregion

        #region Static
        public bool IsPathTooLong(string path)
        {
            // The fully qualified file name must be less than 260 characters, 
            // and the directory name must be less than 248 characters. This 
            // simple method just checks for a limit of 248 characters.
            return path.Length >= 248;
        }

        public static bool IsAbsolutePath(string path)
        {
            Regex regex = new Regex("^(([a-zA-Z]:\\\\)|(//)).*");
            Match match = regex.Match(path);

            return match.Success;
        }

        public static string SanitizeFilename(string filename)
        {

            string retVal = string.Empty;
            string replaceStr = string.Empty;

            // Invalid characters for filenames: \ / : * ? " < > |
            retVal = filename.Replace("\\", replaceStr);
            retVal = retVal.Replace("/", replaceStr);
            retVal = retVal.Replace(":", replaceStr);
            retVal = retVal.Replace("*", replaceStr);
            retVal = retVal.Replace("?", replaceStr);
            retVal = retVal.Replace("\"", replaceStr);
            retVal = retVal.Replace("<", replaceStr);
            retVal = retVal.Replace(">", replaceStr);
            retVal = retVal.Replace("|", replaceStr);

            return retVal;
        }
        #endregion
    }
}
