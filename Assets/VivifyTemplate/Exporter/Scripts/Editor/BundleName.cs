using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class BundleName : EditorWindow
    {
        private string _inputText;

        public static string ProjectBundle
        {
            get => PlayerPrefs.GetString("projectBundle", "bundle");
            set => PlayerPrefs.SetString("projectBundle", value);
        }

        private void OnEnable()
        {
            _inputText = ProjectBundle;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);

            _inputText = EditorGUILayout.TextField("Bundle name:", _inputText).Trim();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Apply"))
            {
                Close();
                ProjectBundle = _inputText;
            }
        }

        [MenuItem("Vivify/Set Bundle Name")]
        private static void CreatePopup()
        {
            BundleName window = CreateInstance<BundleName>();
            window.minSize = new Vector2(400, 80);
            window.maxSize = window.minSize;
            window.ShowUtility();
        }
    }
}