using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VivifyTemplate.Utilities.Scripts
{
	[ExecuteInEditMode]
	public class PrefabSaver : MonoBehaviour
	{
		public string m_destinationFolder;
		public bool m_onSceneSave = true;
		public bool m_logResult = true;

		#if UNITY_EDITOR
		private void OnEnable()
		{
			EditorSceneManager.sceneSaved += SaveOnSceneSave;
		}
		private void OnDisable()
		{
			EditorSceneManager.sceneSaved -= SaveOnSceneSave;
		}

		private void SaveOnSceneSave(Scene _)
		{
			if (m_onSceneSave)
				SaveToPrefab();
		}
		public void SaveToPrefab()
		{
			string prefabName = name;
			string prefabPath = Path.Combine(m_destinationFolder, $"{prefabName}.prefab");

			if (!AssetDatabase.IsValidFolder(m_destinationFolder))
			{
				throw new Exception("Destination folder does not exist.");
			}

			// Remove C# scripts
			GameObject temp = Instantiate(gameObject);
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

			if (m_logResult)
				Debug.Log($"Prefab '{prefabName}' overwritten successfully.");
		}
		#endif
	}
}
