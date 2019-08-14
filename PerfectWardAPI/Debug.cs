using System;
using System.IO;
using System.Windows;

namespace PerfectWardAPI
{
    public static class Debug
    {
        private static FileStream logStream;
        private static StreamWriter logWriter;

        static Debug()
        {
            try
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var fname = $"{DateTime.Now.ToString("yyyy-dd-M_HH.mm.ss")}_debug.log";
                var logPath = $"{appDataFolder}\\PerfectWardAPI\\{fname}";
                logStream = new FileStream(logPath, FileMode.Create);
                logWriter = new StreamWriter(logStream);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        public static void Log(string message)
        {
            logWriter.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
            logWriter.Flush();
            logStream.Flush();
            Console.WriteLine(message);
        }

        public static void Close()
        {
            logStream.Close();
        }
    }
}
