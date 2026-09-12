using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VivifyTemplate.Utilities.Runtime
{
	[ExecuteInEditMode]
	public class PrefabSaver : MonoBehaviour
	{
		public GameObject m_destinationPrefab;
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
			if (m_onSceneSave && m_destinationPrefab != null)
				SaveToPrefab();
		}

		public void SaveToPrefab()
		{
			string prefabPath = GetDestinationPrefabPath();
			GameObject temp = Instantiate(gameObject);

			try
			{
				// Remove C# scripts
				var components = temp.GetComponentsInChildren<Component>().ToList();
				foreach (var comp in components)
				{
					if (comp == null) continue; // Missing script
					var type = comp.GetType();

					if (comp is IPrefabSaveProcessor prefabSaveProcessor)
						prefabSaveProcessor.OnPrefabSaved(temp, prefabPath);

					if (comp is MonoBehaviour && !type.Namespace?.StartsWith("UnityEngine") == true)
						DestroyImmediate(comp);
				}

				// Enable animator (bc the animation window likes to turn it off in preview)
				if (temp.TryGetComponent(out Animator animator))
					animator.enabled = true;

				if (PrefabUtility.SaveAsPrefabAsset(temp, prefabPath) == null)
					throw new InvalidOperationException($"Failed to save prefab at '{prefabPath}'.");
			}
			finally
			{
				DestroyImmediate(temp);
			}

			if (m_logResult)
				Debug.Log($"Prefab '{prefabPath}' overwritten successfully.");
		}

		private string GetDestinationPrefabPath()
		{
			if (m_destinationPrefab == null)
				throw new InvalidOperationException("Destination prefab is not assigned.");

			string prefabPath = AssetDatabase.GetAssetPath(m_destinationPrefab);
			if (!prefabPath.StartsWith("Assets/", StringComparison.Ordinal) ||
			    !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
			    PrefabUtility.GetPrefabAssetType(m_destinationPrefab) == PrefabAssetType.NotAPrefab)
			{
				throw new InvalidOperationException("Destination must be a prefab asset under Assets.");
			}

			return prefabPath;
		}
		#endif
	}
}
