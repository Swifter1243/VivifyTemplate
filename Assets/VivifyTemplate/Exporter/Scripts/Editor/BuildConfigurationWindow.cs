using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class BuildConfigurationWindow : EditorWindow
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
            VersionToggle("Android 2021", BuildVersion.Android2021);

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _compressed = EditorGUILayout.ToggleLeft("Compressed", _compressed);

            EditorGUILayout.Space(20);
            GUILayout.FlexibleSpace();

            if (_versions.Count > 0)
            {
                if (GUILayout.Button("Build", GUILayout.Height(40)))
                {
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

            IEnumerable<BuildRequest> requests = _versions.Select(v => PlatformManager.Instance.MakeRequest(v));
            BuildAssetBundles.BuildAllRequests(requests.ToList(), options);
        }

        [MenuItem("Vivify/Build/Build Configuration Window")]
        public static void ShowWindow()
        {
            BuildConfigurationWindow window = GetWindow<BuildConfigurationWindow>(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            window.titleContent = new GUIContent("Build Configuration");
            window.minSize = new Vector2(400, 240);
            window.maxSize = window.minSize;
        }
    }
}
