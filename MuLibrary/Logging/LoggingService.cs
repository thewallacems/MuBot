using System;
using System.Diagnostics;
using System.IO;

namespace MuLibrary.Logging
{
    /// <summary>
    /// A service used to log files to the Console and/or Text File
    /// </summary>
    public class LoggingService
    {
        public LoggingService()
        {
            var Class = this.GetType().Name;

            if (Class.EndsWith("`1"))
                Class = Class[..(Class.Length - 3)];

            Log($"{Class} created");
        }

        /// <summary>
        /// Logs a message to a Text File or to a Text File and the Console using the given LoggingEnum flag
        /// </summary>
        /// <param name="message">The message being logged</param>
        /// <param name="_file">The _file to which the message will be logged</param>
        /// <param name="flag">The given flag telling how to log the message</param>
        /// <exception cref="ArgumentException"></exception>
        public void Log(string message, string _file, LoggingEnum flag)
        {
            switch (flag)
            {
                case LoggingEnum.TotalLogging: TotalLogging(message, _file); break;
                case LoggingEnum.LogToFile: LogToFile(message, _file); break;
                default: throw new ArgumentException(message: "Invalid enum value: ", paramName: nameof(flag));
            }
        }

        /// <summary>
        /// Logs a message to the Console
        /// </summary>
        /// <param name="message">The message which is being logged</param>
        public void Log(string message)
        {
            LogToConsole(message);
        }

        /// <summary>
        /// Logs the message to the Console and Text File
        /// </summary>
        /// <param name="message">The message which is being logged</param>
        /// <param name="_file">The _file to which the message is being logged</param>
        private void TotalLogging(string message, string _file)
        {
            LogToConsole(message);
            LogToFile(message, _file);
        }

        /// <summary>
        /// Logs the message to the Console
        /// </summary>
        /// <param name="message">The message being logged to the Console</param>
        private void LogToConsole(string message)
        {
            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(3).GetMethod();
            var Class = methodBase.DeclaringType.Name;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ///AsyncMethodBuilderCore///
            Console.WriteLine($"[{timestamp}] {Class,-22} {message,-80}");
        }

        /// <summary>
        /// Logs the message to a Text File
        /// </summary>
        /// <param name="message">The message being logged to the Text File</param>
        /// <param name="_file">The _file to which the message is being logged</param>
        private void LogToFile(string message, string _file)
        {
            if (!File.Exists(_file))
            {
                using FileStream fs = File.Create(_file);
                fs.Close();
            }

            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(3).GetMethod();
            var Class = methodBase.DeclaringType.Name;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");

            using StreamWriter sw = File.AppendText(_file);
            sw.Write($"[{timestamp}] {Class} {message}");
            sw.Close();
        }
    }
}