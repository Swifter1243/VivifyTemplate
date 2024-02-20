using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;

public class Initialize
{
	[MenuItem("Assets/Vivify/Setup Project")]
    [Obsolete]
    static void SetupProject()
	{
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.virtualRealitySupported = true;
        Debug.Log("Project set up!");
	}
}