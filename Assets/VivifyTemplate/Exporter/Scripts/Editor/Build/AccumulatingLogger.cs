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
                if (_empty)
                {
                    _empty = false;
                }
                else
                {
                    _log += "/n";
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
