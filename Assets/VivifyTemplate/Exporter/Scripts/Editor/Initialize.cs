using System;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
	public static class Initialize
	{
		[MenuItem("Vivify/Setup Project")]
		[Obsolete]
		private static void SetupProject()
		{
			PlayerSettings.colorSpace = ColorSpace.Linear;
			PlayerSettings.virtualRealitySupported = true;
			Debug.Log("Project set up!");
			Debug.Log("If you plan to build for android, install the android module in build settings.");
			Debug.Log("You can only install it if you have a unity version installed through the hub.");
		}
	}
}