using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class InstallPackages
    {
        static AddRequest InputRequest;
        static AddRequest ManagementRequest;
        static AddRequest OpenXRRequest;
        static AddRequest OculusRequest;

        private static bool Cancel = false;

        private static InstallPackagesPopup _popup;

        #if UNITY_2021
        [MenuItem("Vivify/Cancel Packages")]
        public static void CancelInstall()
        {
            Cancel = true;
        }
        [MenuItem("Vivify/Install Packages")]
        #endif
        public static void Setup()
        {
            _popup = InstallPackagesPopup.Popup();
            _popup.SetStatus("Installing packages. Please wait...", Color.gray);
            EditorApplication.update += Progress;
        }

        private static void Progress()
        {
            if (Cancel)
            {
                _popup.SetStatus("Cancelling...", Color.red);
                EditorApplication.update -= Progress;
                return;
            }

            if (InputRequest == null)
            {
                //Install Input System
                InputRequest = Client.Add("com.unity.inputsystem");
                return;
            }
            if (InputRequest.IsCompleted)
            {
                //Input Installed
                if (InputRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("Failed to install Input System");
                    //EditorApplication.Exit(1);
                    return;
                }
            }
            else
            {
                //Waiting
                return;
            }

            if (ManagementRequest == null)
            {
                //Install OpenXR
                ManagementRequest = Client.Add("com.unity.xr.management");
                return;
            }
            if (ManagementRequest.IsCompleted)
            {
                //OpenXR Installed
                if (ManagementRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("Failed to install Management");
                    //EditorApplication.Exit(1);
                    return;
                }
            }
            else
            {
                //Waiting
                return;
            }

            if (OpenXRRequest == null)
            {
                //Install OpenXR
                OpenXRRequest = Client.Add("com.unity.xr.openxr");
                return;
            }
            if (OpenXRRequest.IsCompleted)
            {
                //OpenXR Installed
                if (OpenXRRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("Failed to install OpenXR");
                    //EditorApplication.Exit(1);
                    return;
                }
            }
            else
            {
                //Waiting
                return;
            }

            if (OculusRequest == null)
            {
                //Install Oculus
                OculusRequest = Client.Add("com.unity.xr.oculus");
                return;
            }
            if (OculusRequest.IsCompleted)
            {
                //Oculus Installed
                if (OculusRequest.Status == StatusCode.Failure)
                {
                    Debug.LogError("Failed to install Oculus");
                    //EditorApplication.Exit(1);
                    return;
                }
            }
            else
            {
                //Waiting
                return;
            }

            _popup.SetStatus("All packages installed.", Color.green);
            EditorApplication.Exit(1);
        }
    }
}
