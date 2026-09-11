using System;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
    public static class XRPluginHelper
    {
        public static bool IsInstalled()
        {
            // Check if the XR Management namespace exists
            Type xrManagementType = Type.GetType("UnityEngine.XR.Management.XRGeneralSettings, Unity.XR.Management");
            if (xrManagementType != null)
            {
                return true;
            }

            // Alternatively, check for another specific class from the new XR plugins
            Type xrManagerSettingsType = Type.GetType("UnityEngine.XR.Management.XRManagerSettings, Unity.XR.Management");
            return xrManagerSettingsType != null;
        }
    }
}
