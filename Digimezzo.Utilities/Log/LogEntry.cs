using System.Diagnostics;

namespace Digimezzo.Utilities.Log
{
    public class LogEntry
    {
        public string TimeStamp { get; set; }
        public string Level { get; set; }
        public string CallerFilePath { get; set; }
        public string CallerMemberName { get; set; }
        public int CallerLineNumber { get; set; }
        public string Message { get; set; }
    }
}
