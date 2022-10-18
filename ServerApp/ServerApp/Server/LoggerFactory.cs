using System;
using Gaming.ContainerManager.ImageContracts.V1;

namespace ServerApp.Server
{
    public class LoggerFactory : ILoggerFactory
    {
        
        private static DateTime startTime = DateTime.UtcNow;
        
        public static string GetTime()
        {
            var delta = DateTime.UtcNow - startTime;
            return delta.ToString(@"mm\:ss\.fff");
        }

        
        
        public ILogger CreateLogger(string category)
        {
            return new Logger();
        }

        public ILogger System { get; } = new Logger();

        private class Logger : ILogger
        {
            public void Log(LogLevel level, string message)
            {
                Console.WriteLine($"{GetTime()}: [{level}] {message}");
            }

            public void Log(LogLevel level, Exception exception)
            {
                Console.WriteLine($"{GetTime()}: [{level}] {exception}");
            }
        }
    }
}