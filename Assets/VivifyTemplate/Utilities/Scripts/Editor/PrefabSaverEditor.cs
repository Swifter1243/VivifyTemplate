using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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
			if (saver.m_destination == null)
			{
				EditorGUILayout.HelpBox("Please assign a destination prefab.", MessageType.Warning);
				return;
			}

			if (GUILayout.Button("Save To Destination", GUILayout.Height(30)))
			{
				SaveToPrefab();
			}
		}

		private void OnEnable()
		{
			EditorSceneManager.sceneSaved += SaveToPrefab;
		}
		private void OnDisable()
		{
			EditorSceneManager.sceneSaved -= SaveToPrefab;
		}

		private void SaveToPrefab(Scene _)
		{
			SaveToPrefab();
		}
		private void SaveToPrefab()
		{
			PrefabSaver saver = (PrefabSaver)target;
			var prefab = saver.m_destination;
			string prefabPath = AssetDatabase.GetAssetPath(prefab);

			if (string.IsNullOrEmpty(prefabPath))
			{
				Debug.LogError("Destination prefab must be an asset in the project, not a scene object.");
				return;
			}

			// Remove C# scripts
			GameObject temp = Instantiate(saver.gameObject);
			var components = temp.GetComponents<Component>().ToList();
			foreach (var comp in components)
			{
				if (comp == null) continue; // Missing script
				var type = comp.GetType();
				if (comp is MonoBehaviour && !type.Namespace?.StartsWith("UnityEngine") == true)
				{
					DestroyImmediate(comp);
				}
			}

			// Enable animator (bc the animation window likes to turn it off in preview)
			if (temp.TryGetComponent(out Animator animator))
			{
				animator.enabled = true;
			}

			PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
			AssetDatabase.Refresh();

			DestroyImmediate(temp);

			Debug.Log($"Prefab '{prefab.name}' overwritten successfully.");
		}
	}
}
