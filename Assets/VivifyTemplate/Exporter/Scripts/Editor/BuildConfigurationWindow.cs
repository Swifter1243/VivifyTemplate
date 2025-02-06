using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class BuildConfigurationWindow : EditorWindow
    {
        private readonly HashSet<BuildVersion> _versions = new HashSet<BuildVersion>();
        private bool _compressed = false;

        private Texture2D _tbsLogo;

        private void OnEnable()
        {
            // there has to be better way to do this lol
            _tbsLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VivifyTemplate/Exporter/Textures/TBS_trans.png");
        }

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
            GUILogo();
            GUIVersions();
            EditorGUILayout.Space(20);
            GUISettings();
            EditorGUILayout.Space(20);
            GUILayout.FlexibleSpace();
            GUIBuild();
        }

        private void GUIBuild()
        {

            bool questNotReady = !QuestSetup.IsQuestProjectReady() && _versions.Contains(BuildVersion.Android2021);
            bool canBuild = _versions.Count > 0 && !questNotReady;

            if (questNotReady)
            {
                GUIStyle redTextStyle = new GUIStyle();
                redTextStyle.normal.textColor = Color.red;
                redTextStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Your project for quest is not set up.", redTextStyle);
                EditorGUILayout.Space(10);

                if (GUILayout.Button("Setup", GUILayout.Height(40)))
                {
                    QuestSetup.CreatePopup();
                }
            }

            if (canBuild)
            {
                if (GUILayout.Button("Build", GUILayout.Height(40)))
                {
                    Build();
                }
            }
        }
        private void GUIVersions()
        {

            EditorGUILayout.LabelField("Versions", EditorStyles.boldLabel);
            VersionToggle("Windows 2019", BuildVersion.Windows2019);
            VersionToggle("Windows 2021", BuildVersion.Windows2021);
            VersionToggle("Android (Quest) 2021", BuildVersion.Android2021);
        }

        private void GUISettings()
        {

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _compressed = EditorGUILayout.Toggle("Compressed", _compressed);
            ProjectBundle.Value = EditorGUILayout.TextField("Bundle Name To Export", ProjectBundle.Value);
            ShouldExportBundleInfo.Value = EditorGUILayout.Toggle("Should Export Bundle Info", ShouldExportBundleInfo.Value);

            if (ShouldExportBundleInfo.Value) {
                ShouldPrettifyBundleInfo.Value = EditorGUILayout.Toggle("Should Prettify Bundle Info", ShouldPrettifyBundleInfo.Value);
            }
        }

        private void GUILogo()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    background = Texture2D.blackTexture
                },
                fixedWidth = 80
            };
            GUILayout.Box(_tbsLogo, style, GUILayout.Height(80));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void Build()
        {
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

            if (!_compressed)
            {
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
            }

            IEnumerable<BuildRequest> requests = _versions.Select(v => PlatformManager.Instance.CreateRequestFromVersion(v));
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
