using System;
using System.Threading.Tasks;

namespace VivifyTemplate.Exporter.Scripts
{
    public class Logger
    {
        private string _log = string.Empty;
        private bool _empty = true;

        public async void Log(string message)
        {
            _empty = false;

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

        public bool IsEmpty()
        {
            return _empty;
        }
    }
}
