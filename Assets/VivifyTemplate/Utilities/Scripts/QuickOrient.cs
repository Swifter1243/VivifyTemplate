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
		private static QuickOrient s_current;
		[SerializeField]
		private KeyCode m_hotkey = KeyCode.G;

		private void Update()
		{
			s_current = this;
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

			if (e.type == EventType.KeyDown && s_current && e.keyCode == s_current.m_hotkey)
			{
				SceneView scene = SceneView.lastActiveSceneView;
				scene.LookAtDirect(s_current.transform.position + Vector3.forward * scene.cameraDistance, s_current.transform.rotation);
				scene.Repaint();
			}
		}
		#endif
	}
}
