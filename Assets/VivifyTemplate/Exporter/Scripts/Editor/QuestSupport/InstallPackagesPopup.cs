using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport {
	public class InstallPackagesPopup : EditorWindow {
		private string _status = "";
		private Color _statusColor = Color.white;

		// lmfao
		private static Rect GetMainEditorWindowSize()
		{
			Type containerWindowType = Type.GetType("UnityEditor.ContainerWindow,UnityEditor");
			if (containerWindowType == null) return new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);

			FieldInfo showModeField = containerWindowType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
			PropertyInfo positionProperty = containerWindowType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

			if (showModeField == null || positionProperty == null) return new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);

			foreach (var window in Resources.FindObjectsOfTypeAll(containerWindowType))
			{
				int showMode = (int)showModeField.GetValue(window);
				// ShowMode 4 is Main Editor Window
				if (showMode == 4)
				{
					return (Rect)positionProperty.GetValue(window);
				}
			}

			return new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
		}

		public static InstallPackagesPopup Popup()
		{
			Rect res = GetMainEditorWindowSize();
			Vector2 size = new Vector2(800, 300);
			InstallPackagesPopup window = CreateInstance<InstallPackagesPopup>();
			window.ShowPopup();
			window.position = new Rect(res.width / 2f - size.x * 0.5f, res.height / 2f - size.y * 0.5f, size.x, size.y);
			window.SetStatus("Installing packages. Please wait...", Color.gray);
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
