using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
	[ExecuteInEditMode]
	public class QuickOrient : MonoBehaviour
	{
		private static Transform s_current;

		private void Update()
		{
			s_current = transform;
		}

		#if UNITY_EDITOR

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void ScriptsHasBeenReloaded()
		{
			SceneView.duringSceneGui += DuringSceneGui;
		}

		private static void DuringSceneGui(SceneView sceneView)
		{
			Event e = Event.current;

			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F && s_current && Selection.activeGameObject == null)
			{
				SceneView scene = SceneView.lastActiveSceneView;
				scene.LookAtDirect(s_current.position + Vector3.forward * scene.cameraDistance, s_current.rotation);
				scene.Repaint();
			}
		}
		#endif
	}
}
