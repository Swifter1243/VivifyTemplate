using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
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
    public class UnityThread : MonoBehaviour
    {
        private static UnityThread _instance = null;
        private static readonly List<Action> _actionQueuesUpdateFunc = new List<Action>();
        private readonly List<Action> _actionCopiedQueueUpdateFunc = new List<Action>();
        private static volatile bool _noActionQueueToExecuteUpdateFunc = true;

        internal static void InitUnityThread()
        {
            Debug.Log("InitUnityThread");
            if (_instance != null)
            {
                return;
            }
            // add an invisible game object to the scene
            var obj = new GameObject("MainThreadExecute")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _instance = obj.AddComponent<UnityThread>();
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

        public void Update()
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

        public void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}