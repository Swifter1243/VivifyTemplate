using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
{
    public class SimpleTimer
    {
        private float _lastMarkedTime = 0;

        public float Mark()
        {
            float currentTime = Time.realtimeSinceStartup;
            float elapsed = currentTime - _lastMarkedTime;
            _lastMarkedTime = currentTime;
            return elapsed;
        }
    }
}