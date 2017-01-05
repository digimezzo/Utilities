using System;
using System.Text.RegularExpressions;
using Digimezzo.Utilities.Win32;

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

        /// <summary>
        /// Send file to recycle bin
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
        public static bool Send(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = FileOperationFlags.FOF_ALLOWUNDO | flags
                };
                NativeMethods.SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool Send(string path)
        {
            return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
        }

        /// <summary>
        /// Send file silently to recycle bin. Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool MoveToRecycleBin(string path)
        {
            return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI | FileOperationFlags.FOF_SILENT);
        }
    }
}
