namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class BuildTask
    {
        private Logger _logger = new Logger();
        private readonly string _name;
        BuildProgressWindow.BuildState _state = BuildProgressWindow.BuildState.InProgress;

        public BuildTask(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }

        public Logger GetLogger()
        {
            return _logger;
        }

        public void Success()
        {
            _state = BuildProgressWindow.BuildState.Success;
        }

        public void Fail(string message)
        {
            _logger.Log(message);
            _state = BuildProgressWindow.BuildState.Fail;
        }

        public BuildProgressWindow.BuildState GetState()
        {
            return _state;
        }
    }
}
