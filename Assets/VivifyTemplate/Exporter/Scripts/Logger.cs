using System;
using System.Threading.Tasks;

namespace VivifyTemplate.Exporter.Scripts
{
    public class Logger
    {
        private string _log = string.Empty;
        private bool _empty = true;

        public void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");

            if (!_empty)
            {
                _log += Environment.NewLine;
            }

            _log += $"[{time}] " + message;

            _empty = false;
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
