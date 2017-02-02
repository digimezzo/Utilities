using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Digimezzo.Utilities.Log
{
    public class LogClient
    {
        #region Variables
        private string logfile;
        private string logfolder;
        private static LogClient instance;
        private Queue<LogEntry> logEntries;
        private Object logEntriesLock = new Object();
        private Timer logTimer = new Timer();
        private bool isInitialized;
        private int archiveAboveSize = 5242880; // 5 MB
        private int maxArchiveFiles = 3;
        #endregion

        #region Construction
        private LogClient()
        {
            this.logfolder = Path.Combine(SettingsClient.ApplicationFolder(), "Log");
            this.logfile = System.IO.Path.Combine(this.logfolder, ProcessExecutable.Name() + ".log");
            this.logEntries = new Queue<LogEntry>();
            this.logTimer.Interval = 25;
            this.logTimer.Elapsed += LogTimer_Elapsed;
        }

        private void LogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.logTimer.Stop();

            lock (this.logEntriesLock)
            {
                if (this.logEntries.Count > 0)
                {
                    this.TryWrite(logEntries.Dequeue());
                    this.logTimer.Start();
                }
            }
        }

        public static LogClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogClient();
                }
                return instance;
            }
        }
        #endregion

        #region Private
        private void AddLogEntry(LogLevels level, StackFrame frame, string message, string[] args)
        {
            this.logTimer.Stop();

            try
            {
                if (args != null) message = string.Format(message, args);
            }
            catch (Exception ex)
            {
                lock (logEntriesLock)
                {
                    this.logEntries.Enqueue(new LogEntry() { Level = level, Frame = frame, Message = ex.Message });
                }
            }

            lock (logEntriesLock)
            {
                this.logEntries.Enqueue(new LogEntry() { Level = level, Frame = frame, Message = message });
            }

            this.logTimer.Start();
        }

        private void TryWrite(LogEntry entry)
        {
            try
            {
                string callsite = string.Empty;

                try
                {
                    // Try to find the callsite
                    var method = entry.Frame.GetMethod();
                    var className = method.ReflectedType.Name;
                    var methodName = method.Name;

                    callsite = className + "." + methodName;
                }
                catch (Exception)
                {
                }

                // If the log directory doesn't exist, create it.
                if (!Directory.Exists(this.logfolder)) Directory.CreateDirectory(this.logfolder);

                // If the logfile doesn't exist, this also creates it.
                bool isWriteSuccess = false;

                while (!isWriteSuccess)
                {
                    try
                    {
                        using (StreamWriter sw = File.AppendText(this.logfile))
                        {
                            string levelDescription = string.Empty;

                            switch (entry.Level)
                            {
                                case LogLevels.Info:
                                    levelDescription = "Info";
                                    break;
                                case LogLevels.Warning:
                                    levelDescription = "Warning";
                                    break;
                                case LogLevels.Error:
                                    levelDescription = "Error";
                                    break;
                                default:
                                    levelDescription = "Error";
                                    break;
                            }

                            sw.WriteLine(string.Format("{0}|{1}|{2}|{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), levelDescription, callsite, entry.Message));
                        }
                        isWriteSuccess = true;
                    }
                    catch (Exception)
                    {
                    }
                }

                // Rotate the log file
                try
                {
                    this.RotateLogfile();
                }
                catch (Exception)
                {
                }

                // Delete archived log files
                try
                {
                    this.DeleteArchives();
                }
                catch (Exception)
                {
                }
            }
            catch (Exception)
            {
            }
        }

        private void RotateLogfile()
        {
            var fi = new FileInfo(this.logfile);

            if (fi.Length >= this.archiveAboveSize)
            {
                string archivedLogfile = Path.Combine(Path.GetDirectoryName(this.logfile), Path.GetFileNameWithoutExtension(this.logfile) + DateTime.Now.ToString(" (yyyy-MM-dd HH.mm.ss.fff)") + ".log");
                File.Move(this.logfile, archivedLogfile);
            }
        }

        private void DeleteArchives()
        {
            var di = new DirectoryInfo(this.logfolder);

            FileInfo[] files = di.GetFiles().OrderBy(p => p.CreationTime).ToArray();

            while (files.Length > this.maxArchiveFiles + 1)
            {
                FileInfo fi = files.FirstOrDefault();
                if (fi != null) File.Delete(fi.FullName);
                files = di.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            }
        }
        #endregion

        #region Static
        public static void Initialize(int archiveAboveSize, int maxArchiveFiles)
        {
            if (LogClient.Instance.isInitialized) return;
            LogClient.Instance.isInitialized = true;
            LogClient.Instance.archiveAboveSize = archiveAboveSize;
            LogClient.Instance.maxArchiveFiles = maxArchiveFiles;
        }

        public static string Logfile()
        {
            return LogClient.Instance.logfile;
        }

        public static string GetAllExceptions(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Exception:");
            sb.AppendLine(ex.ToString());
            sb.AppendLine("");
            sb.AppendLine("Stack trace:");
            sb.AppendLine(ex.StackTrace);

            int innerExceptionCounter = 0;

            while (ex.InnerException != null)
            {
                innerExceptionCounter += 1;
                sb.AppendLine("Inner Exception " + innerExceptionCounter + ":");
                sb.AppendLine(ex.InnerException.ToString());
                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        public static void Info(string message, string arg1="", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "")
        {
            LogClient.Instance.AddLogEntry(LogLevels.Info, new StackFrame(1), message, new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        }

        public static void Warning(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "")
        {
            LogClient.Instance.AddLogEntry(LogLevels.Warning, new StackFrame(1), message, new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        }

        public static void Error(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "")
        {
            LogClient.Instance.AddLogEntry(LogLevels.Error, new StackFrame(1), message, new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        }
        #endregion
    }
}
