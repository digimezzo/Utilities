using System.IO;

namespace Digimezzo.Utilities.Exceptions
{
    public class FileInUsingException : IOException
    {
        public FileInUsingException(string path) : base($"The process cannot access the file '{Path.GetFullPath(path)}' because it is being used by another process.")
        {
        }
    }

    public class MaxPathException : IOException
    {
        public MaxPathException(string path) : base($"The length of target path '{Path.GetFullPath(path)}' is over MAX_PATH")
        {
        }
    }
}