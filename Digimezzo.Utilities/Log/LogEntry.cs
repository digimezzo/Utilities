using System.Diagnostics;

namespace Digimezzo.Utilities.Log
{
    public class LogEntry
    {
        public LogLevels Level { get; set; }
        public StackFrame Frame { get; set; }
        public string Message { get; set; }
    }
}
