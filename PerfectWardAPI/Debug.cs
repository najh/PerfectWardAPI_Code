using PerfectWardApi.Api;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PerfectWardAPI
{
    public static class Debug
    {
        const string LOG_SOURCE = "PwTaskService";
        const string LOG_NAME = "PerfectWardConnector";

        private static FileStream logStream;
        private static StreamWriter logWriter;
        private static EventLog eventLog;

        private static DirectoryInfo LogFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\PerfectWardAPI");

        static Debug()
        {
            try
            {
                if (!EventLog.SourceExists(LOG_SOURCE))
                {
                    EventLog.CreateEventSource(LOG_SOURCE, LOG_NAME);
                }
                eventLog = new EventLog()
                {
                    Source = LOG_SOURCE,
                    Log = LOG_NAME
                };
                eventLog.Clear();

                if (LogFolder.Exists)
                {
                    var fname = $"{DateTime.Now.ToString("yyyy-dd-M_HH.mm.ss")}_debug.log";
                    var logPath = $"{LogFolder.FullName}\\{fname}";
                    logStream = new FileStream(logPath, FileMode.Create);
                    logWriter = new StreamWriter(logStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in logging:\n{ex}");
            }
        }

        public static void Uninstall()
        {
            if (LogFolder.Exists)
            {
                foreach (var f in LogFolder.GetFiles("*_.debug.log"))
                {
                    f.Delete();
                }
            }
            if (EventLog.SourceExists(LOG_SOURCE))
            {
                EventLog.DeleteEventSource(LOG_SOURCE);
            }
            EventLog.Delete(LOG_NAME);
        }

        public static void Log(string message)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
                logWriter.Flush();
                logStream.Flush();
            }

            eventLog.WriteEntry(message, EventLogEntryType.Information, EventId);
        }

        private static int EventId
        {
            get
            {
                var id = EnvironmentVariables.Get(EnvironmentVariables.EventId);
                if (id == null) return EventId = 0;
                return EventId = (int.Parse(id) % int.MaxValue) + 1;
            }
            set
            {
                EnvironmentVariables.Set(EnvironmentVariables.EventId, value.ToString());
            }
        }

        public static void Close()
        {
            logStream?.Close();
        }
    }
}
