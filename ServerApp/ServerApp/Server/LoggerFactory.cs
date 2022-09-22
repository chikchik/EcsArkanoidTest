using System;
using Gaming.ContainerManager.ImageContracts.V1;

namespace ServerApp.Server
{
    public class LoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string category)
        {
            return new Logger();
        }

        public ILogger System { get; } = new Logger();

        private class Logger : ILogger
        {
            public void Log(LogLevel level, string message)
            {
                Console.WriteLine($"[{level}] {message}");
            }

            public void Log(LogLevel level, Exception exception)
            {
                Console.WriteLine($"[{level}] {exception}");
            }
        }
    }
}