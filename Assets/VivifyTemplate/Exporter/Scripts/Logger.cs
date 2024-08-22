using System;
using System.Threading.Tasks;

namespace VivifyTemplate.Exporter.Scripts
{
    public class Logger
    {
        private string _log = string.Empty;

        public async void Log(string message)
        {
            await Task.Run(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                _log += $"[{time}] " + message + Environment.NewLine;
            });
        }

        public string GetOutput()
        {
            return _log;
        }
    }
}
