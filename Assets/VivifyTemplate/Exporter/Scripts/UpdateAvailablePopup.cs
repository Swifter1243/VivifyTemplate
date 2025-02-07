using System;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
namespace VivifyTemplate.Exporter.Scripts
{
	public class UpdateAvailablePopup : EditorWindow
	{
		public static void Popup()
		{
			Rect res = EditorHelper.GetMainEditorWindowSize();
			Vector2 size = new Vector2(800, 300);
			UpdateAvailablePopup window = CreateInstance<UpdateAvailablePopup>();
			window.ShowUtility();
			window.minSize = size;
			window.maxSize = size;
			window.titleContent = new GUIContent("VivifyTemplate Update Available!");
			window.position = new Rect(res.width / 2f - size.x * 0.5f, res.height / 2f - size.y * 0.5f, size.x, size.y);
		}

		private void OnGUI()
		{
			GUIStyle style = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = 20,
				normal =
				{
					textColor = Color.white,
				}
			};

			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField("A new update for VivifyTemplate is available!", style, GUILayout.Height(style.fontSize * 2));
			EditorGUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space(80);
			if (GUILayout.Button("Go to releases", GUILayout.Height(40)))
			{
				Application.OpenURL("https://github.com/Swifter1243/VivifyTemplate?tab=readme-ov-file#setup");
			}
			EditorGUILayout.Space(80);
			EditorGUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();
		}
	}
}
