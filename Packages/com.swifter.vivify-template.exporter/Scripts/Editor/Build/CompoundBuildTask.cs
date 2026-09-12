using System;
namespace VivifyTemplate.Exporter.Editor.Build
{
	public class CompoundBuildTask
	{
		public BuildSettings buildSettings;
		public event Action onComplete;

		private bool _complete = false;

		public CompoundBuildTask(BuildSettings buildSettings)
		{
			this.buildSettings = buildSettings;
		}

		public void MarkComplete()
		{
			if (!_complete)
			{
				onComplete?.Invoke();
				_complete = true;
			}
		}
	}
}
