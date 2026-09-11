using System.IO;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Utilities.Scripts.Editor
{
	[CustomEditor(typeof(PrefabSaver))]
	public class PrefabSaverEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			PrefabSaver saver = (PrefabSaver)target;

			GUILayout.Space(10);
			if (string.IsNullOrEmpty(saver.m_destinationFolder))
			{
				EditorGUILayout.HelpBox("Please assign a destination folder.", MessageType.Warning);
				return;
			}

			if (!Directory.Exists(saver.m_destinationFolder))
			{
				EditorGUILayout.HelpBox("Destination folder does not exist.", MessageType.Error);
				return;
			}

			if (GUILayout.Button("Save To Destination", GUILayout.Height(30)))
			{
				saver.SaveToPrefab();
			}
		}
	}
}
