using System;
using System.Text.RegularExpressions;

namespace Digimezzo.Utilities.Utils
{
    public static class FileUtils
    {
        public static string Folder(string path)
        {
            return System.IO.Path.GetDirectoryName(path);
        }

        public static string Name(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        public static string NameWithoutExtension(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public static long SizeInBytes(string path)
        {
            return new System.IO.FileInfo(path).Length;
        }

        public static DateTime DateModified(string path)
        {
            return new System.IO.FileInfo(path).LastWriteTime;
        }

        public static long DateModifiedTicks(string path)
        {
            return new System.IO.FileInfo(path).LastWriteTime.Ticks;
        }

        public static bool IsPathTooLong(string path)
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
    }
}
