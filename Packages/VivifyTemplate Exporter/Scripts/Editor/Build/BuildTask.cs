using VivifyTemplate.Exporter.Scripts.Editor.UI;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build
{
    public class BuildTask
    {
        private AccumulatingLogger _logger = new AccumulatingLogger();
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

        public AccumulatingLogger GetLogger()
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
