using System;
namespace VivifyTemplate.Exporter.Editor.Build
{
    public class Logger
    {
        public delegate void LogDelegate(string message);

        public event LogDelegate OnLog;

        public void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string result = $"[{time}] " + message;
            OnLog?.Invoke(result);
        }

        public void LogUnformatted(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
