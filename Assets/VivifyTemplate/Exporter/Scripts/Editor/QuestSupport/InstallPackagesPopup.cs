using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport {
	public class InstallPackagesPopup : EditorWindow {
		private string _status = "";
		private Color _statusColor = Color.white;

		public static InstallPackagesPopup Popup()
		{
			Rect res = EditorHelper.GetMainEditorWindowSize();
			Vector2 size = new Vector2(800, 300);
			InstallPackagesPopup window = CreateInstance<InstallPackagesPopup>();
			window.position = new Rect(res.width / 2f - size.x * 0.5f, res.height / 2f - size.y * 0.5f, size.x, size.y);
			window.SetStatus("Installing packages. Please wait...", Color.gray);
			window.ShowPopup();
			return window;
		}

		public void SetStatus(string status, Color color)
		{
			_status = status;
			_statusColor = color;
		}

		private void OnGUI()
		{
			GUIStyle style = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = 40,
				normal =
				{
					textColor = _statusColor,
				}
			};

			GUILayout.FlexibleSpace();
			EditorGUILayout.LabelField(_status, style, GUILayout.Height(style.fontSize * 2));
			GUILayout.FlexibleSpace();
		}

		private void OnDestroy()
		{
			Close();
		}
	}
}
