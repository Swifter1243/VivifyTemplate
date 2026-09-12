using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Utilities.Runtime;

namespace VivifyTemplate.Utilities.Editor
{
	[CustomEditor(typeof(PrefabSaver))]
	public class PrefabSaverEditor : UnityEditor.Editor
	{
		private SerializedProperty m_destinationPrefab;
		private SerializedProperty m_onSceneSave;
		private SerializedProperty m_logResult;

		private void OnEnable()
		{
			m_destinationPrefab = serializedObject.FindProperty(nameof(PrefabSaver.m_destinationPrefab));
			m_onSceneSave = serializedObject.FindProperty(nameof(PrefabSaver.m_onSceneSave));
			m_logResult = serializedObject.FindProperty(nameof(PrefabSaver.m_logResult));
		}

		public override void OnInspectorGUI()
		{
			PrefabSaver saver = (PrefabSaver)target;
			serializedObject.Update();

			using (new EditorGUI.DisabledScope(true))
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

			bool createPrefab = DrawDestinationPrefabField();

			EditorGUILayout.PropertyField(m_onSceneSave);
			EditorGUILayout.PropertyField(m_logResult);
			serializedObject.ApplyModifiedProperties();

			if (createPrefab)
				CreateAndAssignEmptyPrefab(saver);

			if (saver.m_destinationPrefab == null)
			{
				EditorGUILayout.HelpBox("Assign a destination prefab or create one above.", MessageType.Warning);
				return;
			}

			string prefabPath = AssetDatabase.GetAssetPath(saver.m_destinationPrefab);
			if (!IsValidDestinationPrefab(saver.m_destinationPrefab, prefabPath))
			{
				EditorGUILayout.HelpBox("Destination must be a prefab asset under Assets.", MessageType.Error);
				return;
			}

			if (GUILayout.Button("Save To Destination Prefab", GUILayout.Height(30)))
				saver.SaveToPrefab();
		}

		private bool DrawDestinationPrefabField()
		{
			EditorGUI.BeginChangeCheck();
			GameObject destinationPrefab = (GameObject)EditorGUILayout.ObjectField(
				"Destination Prefab",
				m_destinationPrefab.objectReferenceValue,
				typeof(GameObject),
				false);
			if (EditorGUI.EndChangeCheck())
				m_destinationPrefab.objectReferenceValue = destinationPrefab;

			if (m_destinationPrefab.objectReferenceValue == null)
			{
				Texture prefabIcon = EditorGUIUtility.IconContent("Prefab Icon").image;
				GUIContent createIcon = prefabIcon != null
					? new GUIContent("Create Prefab Asset", prefabIcon, "Create and assign an empty prefab")
					: new GUIContent("Create Prefab Asset", "Create and assign an empty prefab");
				return GUILayout.Button(createIcon, GUILayout.Height(22));
			}

			return false;
		}

		private void CreateAndAssignEmptyPrefab(PrefabSaver saver)
		{
			string absolutePrefabPath = EditorUtility.SaveFilePanel(
				"Save Prefab To Location",
				GetInitialDirectory(saver),
				saver.name,
				"prefab");

			if (string.IsNullOrEmpty(absolutePrefabPath))
				return;

			string prefabPath = FileUtil.GetProjectRelativePath(absolutePrefabPath).Replace('\\', '/');
			if (string.IsNullOrEmpty(prefabPath) ||
			    !prefabPath.StartsWith("Assets/", StringComparison.Ordinal) ||
			    !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
			    !IsValidAssetFolder(prefabPath))
			{
				EditorUtility.DisplayDialog(
					"Invalid Prefab Location",
					"Create the destination prefab inside this project's Assets folder.",
					"OK");
				return;
			}

			var emptyObject = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
			try
			{
				GameObject prefab = PrefabUtility.SaveAsPrefabAsset(emptyObject, prefabPath);
				if (prefab == null)
				{
					Debug.LogError($"Failed to create prefab at '{prefabPath}'.", saver);
					return;
				}

				serializedObject.Update();
				m_destinationPrefab.objectReferenceValue = prefab;
				serializedObject.ApplyModifiedProperties();
				EditorGUIUtility.PingObject(prefab);
			}
			finally
			{
				DestroyImmediate(emptyObject);
			}
		}

		private static string GetInitialDirectory(PrefabSaver saver)
		{
			string prefabPath = AssetDatabase.GetAssetPath(saver.m_destinationPrefab);
			if (string.IsNullOrEmpty(prefabPath))
				return Application.dataPath;

			string assetDirectory = Path.GetDirectoryName(prefabPath);
			if (string.IsNullOrEmpty(assetDirectory))
				return Application.dataPath;

			string projectRoot = Directory.GetParent(Application.dataPath).FullName;
			return Path.GetFullPath(Path.Combine(projectRoot, assetDirectory));
		}

		private static bool IsValidAssetFolder(string prefabPath)
		{
			string destinationFolder = Path.GetDirectoryName(prefabPath)?.Replace('\\', '/');
			return !string.IsNullOrEmpty(destinationFolder) &&
			       AssetDatabase.IsValidFolder(destinationFolder);
		}

		private static bool IsValidDestinationPrefab(GameObject prefab, string prefabPath)
		{
			return prefab != null &&
			       prefabPath.StartsWith("Assets/", StringComparison.Ordinal) &&
			       prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) &&
			       PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.NotAPrefab;
		}
	}
}
