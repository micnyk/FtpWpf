using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpWpf
{
    public class Logger
    {
        public enum Type
        {
            Info,
            Warning,
            Error    
        }

        private static string LogFilePath = @"C:\Users\Michał\Desktop\ftpwpf.log";
        public static event EventHandler<LogEventArgs> LogEvent;

        public static void Log(Type type, string message, object sender = null)
        {
            LogEvent?.Invoke(sender, new LogEventArgs {Message = message});

            using (var writer = File.AppendText(LogFilePath))
            {
                writer.WriteLine("{0} {1}: {2}", DateTime.Now, type, message);
                writer.Flush();
                writer.Close();
            }
        }
    }

    public class LogEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
