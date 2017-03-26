using System.Diagnostics;

namespace Digimezzo.Utilities.Log
{
    public class LogEntry
    {
        public LogLevel Level { get; set; }
        public string Callsite { get; set; }
        public string Message { get; set; }
    }
}
