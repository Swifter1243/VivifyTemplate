using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;

public class Initialize
{
	[MenuItem("Vivify/Setup Project")]
    [Obsolete]
    static void SetupProject()
	{
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.virtualRealitySupported = true;
        Debug.Log("Project set up!");
        Debug.Log("If you plan to build for android, install the android module in build settings.");
        Debug.Log("Also you can only install it if you have a unity version installed through the hub for some reason");
	}
}