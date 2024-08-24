using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
{
    public class SimpleTimer
    {
        private float _startTime = 0;
        private float _elapsed = 0;

        public float Reset()
        {
            _startTime = Time.realtimeSinceStartup;
            _elapsed = Time.realtimeSinceStartup - _startTime;
            return _elapsed;
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
