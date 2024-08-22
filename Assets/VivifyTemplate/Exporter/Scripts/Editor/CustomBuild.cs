using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class CustomBuild : EditorWindow
    {
        private readonly HashSet<BuildVersion> _versions = new HashSet<BuildVersion>();
        private bool _compressed = false;

        private void VersionToggle(string label, BuildVersion version)
        {
            bool hasVersion = _versions.Contains(version);
            bool toggle = EditorGUILayout.ToggleLeft(label, hasVersion);

            if (toggle && !hasVersion)
            {
                _versions.Add(version);
            }

            if (!toggle && hasVersion)
            {
                _versions.Remove(version);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Versions", EditorStyles.boldLabel);
            VersionToggle("Windows 2019", BuildVersion.Windows2019);
            VersionToggle("Windows 2021", BuildVersion.Windows2021);
            VersionToggle("Android 2019", BuildVersion.Android2019);
            VersionToggle("Android 2021", BuildVersion.Android2021);

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _compressed = EditorGUILayout.ToggleLeft("Compressed", _compressed);

            EditorGUILayout.Space(20);

            if (_versions.Count > 0)
            {
                if (GUILayout.Button("Build"))
                {
                    Close();
                    Build();
                }
            }
        }

        private void Build()
        {
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

            if (!_compressed)
            {
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
            }

            BuildAssetBundles.BuildAll(_versions.ToList(), options);
        }

        [MenuItem("Vivify/Build/Custom Build")]
        private static void CreatePopup()
        {
            CustomBuild window = CreateInstance<CustomBuild>();
            window.titleContent = new GUIContent("Custom Build");
            window.minSize = new Vector2(400, 240);
            window.maxSize = window.minSize;
            window.ShowUtility();
        }
    }
}
