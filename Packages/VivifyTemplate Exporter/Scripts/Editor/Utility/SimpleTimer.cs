using UnityEngine;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
    public class SimpleTimer
    {
        private float _startTime = 0;
        private float _elapsed = 0;

        public float Reset()
        {
            UpdateElapsed();
            float oldElapsed = _elapsed;
            _startTime = Time.realtimeSinceStartup;
            UpdateElapsed();
            return oldElapsed;
        }

        public float UpdateElapsed()
        {
            _elapsed = Time.realtimeSinceStartup - _startTime;
            return _elapsed;
        }

        public float GetElapsed()
        {
            return _elapsed;
        }
    }
}
