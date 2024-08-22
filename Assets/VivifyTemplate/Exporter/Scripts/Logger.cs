using System;

namespace VivifyTemplate.Exporter.Scripts
{
    public class Logger
    {
        private string _log = string.Empty;

        public void Log(string message)
        {
            _log += message + Environment.NewLine;
        }

        public string GetOutput()
        {
            return _log;
        }
    }
}
