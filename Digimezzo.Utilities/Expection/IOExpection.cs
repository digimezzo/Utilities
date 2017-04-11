using System.IO;

namespace Digimezzo.Utilities.Expection
{
    public class FileInUsingExpection : IOException
    {
        public FileInUsingExpection(string path) : base(
            "The process cannot access the file '" + Path.GetFullPath(path) +
            "' because it is being used by another process.")
        {
        }
    }

    public class MaxPathExpection : IOException
    {
        public MaxPathExpection(string path) : base("The length of target path '" + Path.GetFullPath(path) +
                                                    "' is over MAX_PATH")
        {
        }
    }
}