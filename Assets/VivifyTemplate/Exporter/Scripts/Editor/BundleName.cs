using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class BundleName : EditorWindow
    {
        private string inputText;

        public static string projectBundle
        {
            get => PlayerPrefs.GetString("projectBundle", "bundle");
            set => PlayerPrefs.SetString("projectBundle", value);
        }

        private void OnEnable()
        {
            inputText = projectBundle;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);

            inputText = EditorGUILayout.TextField("Bundle name:", inputText).Trim();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Apply"))
            {
                Close();
                projectBundle = inputText;
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