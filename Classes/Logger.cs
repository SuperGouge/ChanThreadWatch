using System;
using System.IO;

namespace JDP {
    public static class Logger {
        private static readonly TextWriter tw;
        private static readonly object _logSync = new object();
        private static readonly string _logPath = Path.Combine(Settings.GetSettingsDirectory(), Settings.LogFileName);

        static Logger() {
            try {
                tw = TextWriter.Synchronized(File.AppendText(_logPath));
            }
            catch { }
        }

        public static void Log(string logMessage) {
            lock (_logSync) {
                try {
                    tw.WriteLine("[{0} - {1}]", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
                    tw.WriteLine(logMessage);
                    tw.WriteLine("-------------------------------");
                    tw.Flush();
                }
                catch { }
            }
        }
    }
}
