using UnityEngine;
namespace VivifyTemplate.Utilities.Runtime
{
	public interface IPrefabSaveProcessor
	{
		void OnPrefabSaved(GameObject root, string prefabPath);
	}
}
