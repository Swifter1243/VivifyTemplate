namespace VivifyTemplate.Exporter.Scripts.Editor.Build
{
    public class AccumulatingLogger : Logger
    {
        private string _log = string.Empty;
        private bool _empty = true;

        public AccumulatingLogger()
        {
            OnLog += (message) =>
            {
                if (!_empty)
                {
                    _log += "/n";
                }
                else
                {
                    _empty = false;
                }

                _log += message;
            };
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
