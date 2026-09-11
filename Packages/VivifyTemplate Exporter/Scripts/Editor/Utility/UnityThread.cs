using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
    [InitializeOnLoad]
    public class Startup {
        static Startup()
        {
            UnityThread.InitUnityThread();
        }
    }

    //https://stackoverflow.com/a/41333540
    [ExecuteAlways]
    public static class UnityThread
    {
        private static readonly List<Action> _actionQueuesUpdateFunc = new List<Action>();
        private static readonly List<Action> _actionCopiedQueueUpdateFunc = new List<Action>();
        private static volatile bool _noActionQueueToExecuteUpdateFunc = true;

        internal static void InitUnityThread()
        {
            EditorApplication.update += Update;
        }

        public static void ExecuteInUpdate(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("Action cannot be null");
            }

            lock (_actionQueuesUpdateFunc)
            {
                _actionQueuesUpdateFunc.Add(action);
                _noActionQueueToExecuteUpdateFunc = false;
                Debug.Log("ExecuteInUpdate");
            }
        }

        public static void Update()
        {
            if (_noActionQueueToExecuteUpdateFunc)
            {
                return;
            }
            Debug.Log("Update");

            _actionCopiedQueueUpdateFunc.Clear();
            lock (_actionQueuesUpdateFunc)
            {
                _actionCopiedQueueUpdateFunc.AddRange(_actionQueuesUpdateFunc);
                _actionQueuesUpdateFunc.Clear();
                _noActionQueueToExecuteUpdateFunc = true;
                Debug.Log("Update2");
            }
            Debug.Log("Update3");

            foreach (var func in _actionCopiedQueueUpdateFunc)
            {
                Debug.Log("Update4");
                func.Invoke();
            }
        }
    }
}
