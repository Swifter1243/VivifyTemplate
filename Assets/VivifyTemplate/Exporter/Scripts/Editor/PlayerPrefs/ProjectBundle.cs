using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
    public class ProjectBundle : EditorWindow
    {
        private string _inputText;
        private static readonly string PlayerPrefsKey = "projectBundle";

        public static string Value
        {
            get => UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey, "bundle");
            set => UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, value);
        }

        private void OnEnable()
        {
            _inputText = Value;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);

            _inputText = EditorGUILayout.TextField("Bundle name:", _inputText).Trim();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Apply"))
            {
                Close();
                Value = _inputText;
            }
        }

        [MenuItem("Vivify/Settings/Set Project Bundle Name")]
        private static void CreatePopup()
        {
            ProjectBundle window = CreateInstance<ProjectBundle>();
            window.titleContent = new GUIContent("Set Project Bundle Name");
            window.minSize = new Vector2(400, 80);
            window.maxSize = window.minSize;
            window.ShowUtility();
        }
    }
}
