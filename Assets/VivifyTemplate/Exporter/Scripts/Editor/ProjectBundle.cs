using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class ProjectBundle : EditorWindow
    {
        private string _inputText;

        public static string Value
        {
            get => PlayerPrefs.GetString("projectBundle", "bundle");
            set => PlayerPrefs.SetString("projectBundle", value);
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

        [MenuItem("Vivify/Set Bundle Name")]
        private static void CreatePopup()
        {
            ProjectBundle window = CreateInstance<ProjectBundle>();
            window.minSize = new Vector2(400, 80);
            window.maxSize = window.minSize;
            window.ShowUtility();
        }
    }
}