using System;
using System.Reflection;
using UnityEngine;
namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
	public static class EditorHelper
	{
		public static Rect GetMainEditorWindowSize()
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
	}
}
