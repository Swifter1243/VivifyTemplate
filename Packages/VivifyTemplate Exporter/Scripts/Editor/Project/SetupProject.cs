using System;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
namespace VivifyTemplate.Exporter.Scripts.Editor.Project
{
	public static class SetupProject
	{
		[MenuItem("Vivify/Setup Project")]
		[Obsolete("Uses Single Pass")]
		public static void Setup()
		{
			PlayerSettings.colorSpace = ColorSpace.Linear;
			PlayerSettings.virtualRealitySupported = true;
			AssignLayers();
			Debug.Log("Project set up!");
			ProjectIsInitialized.Value = true;
		}

        private static void AssignLayers()
        {
            SetLayer(0, "Default");
            SetLayer(1, "TransparentFX");
            SetLayer(2, "IgnoreRaycast");
            SetLayer(3, "ThirdPerson");
            SetLayer(4, "Water");
            SetLayer(5, "UI");
            SetLayer(6, "FirstPerson");
            SetLayer(7, "Layer7");
            SetLayer(8, "Note");
            SetLayer(9, "NoteDebris");
            SetLayer(10, "Avatar");
            SetLayer(11, "Obstacle");
            SetLayer(12, "Saber");
            SetLayer(13, "NeonLight");
            SetLayer(14, "Environment");
            SetLayer(15, "GrabPassTexture1");
            SetLayer(16, "CutEffectParticles");
            SetLayer(17, "HmdOnly");
            SetLayer(18, "DesktopOnly");
            SetLayer(19, "NonReflectedParticles");
            SetLayer(20, "EnvironmentPhysics");
            SetLayer(21, "AlwaysVisible");
            SetLayer(22, "Event");
            SetLayer(23, "DesktopOnlyAndReflected");
            SetLayer(24, "HmdOnlyAndReflected");
            SetLayer(25, "FixMRAlpha");
            SetLayer(26, "AlwaysVisibleAndReflected");
            SetLayer(27, "DontShowInExternalMRCamera");
            SetLayer(28, "PlayersPlace");
            SetLayer(29, "Skybox");
            SetLayer(30, "MRForegroundClipPlane");
            SetLayer(31, "Reserved");
        }

        private static void SetLayer(int index, string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            SerializedProperty layersSerializedProperty = layersProp.GetArrayElementAtIndex(index);
            if (layersSerializedProperty != null)
            {
                if (string.IsNullOrEmpty(layersSerializedProperty.stringValue))
                {
                    layersSerializedProperty.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"Layer {index}: \"{layerName}\" has been added.");
                }
                else if (layersSerializedProperty.stringValue != layerName)
                {
                    Debug.LogWarning($"Layer {index} is already assigned to \"{layersSerializedProperty.stringValue}\" and will not be overwritten.");
                }
                else
                {
                    Debug.Log($"Layer {index}: \"{layerName}\" already exists.");
                }
            }
            else
            {
                Debug.LogError($"Could not assign layer {index}: \"{layerName}\". Index is out of range.");
            }
        }
	}
}
