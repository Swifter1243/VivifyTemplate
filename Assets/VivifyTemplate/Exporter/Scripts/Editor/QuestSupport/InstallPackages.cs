using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class InstallPackages
    {
        static Stack<string> Requests = new Stack<string>();
        static AddRequest InstallingRequest;

        public static void Setup()
        {
            Requests.Push("com.unity.xr.oculus");
            Requests.Push("com.unity.xr.openxr");
            Requests.Push("com.unity.xr.management");
            Requests.Push("com.unity.inputsystem");
            EditorApplication.update += Progress;
            Progress();
        }

        private static void Progress()
        {
            if (Requests.Count == 0 && (InstallingRequest == null || InstallingRequest.IsCompleted))
            {
                Debug.Log("QUIT");
                //EditorApplication.Exit(1);
            }
            if (InstallingRequest == null)
            {
                var request = Requests.Pop();
                Debug.Log($"Installing {request}");
                InstallingRequest = Client.Add(request);
            }
            Debug.Log(InstallingRequest.Status);
            if (InstallingRequest.IsCompleted)
            {
                if (InstallingRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + InstallingRequest.Result.packageId);
                    InstallingRequest = null;
                }
                else if (InstallingRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(InstallingRequest.Error.message);
                    InstallingRequest = null;
                }
                Progress();
            }
            
            
        }
    }
}