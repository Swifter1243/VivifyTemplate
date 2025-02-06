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
        private GUIStyle _titleStyle;

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

        [Obsolete("Possibly sets up project, which uses Single Pass")]
        private void OnGUI()
        {
            GUILogo();

            _titleStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15,
                normal =
                {
                    textColor = new Color(0.9f, 0.9f, 0.9f),
                }
            };

            GUIVersions();
            EditorGUILayout.Space(30);
            GUISettings();
            EditorGUILayout.Space(30);
            GUIQuickBuild();
            EditorGUILayout.Space(30);

            GUILayout.FlexibleSpace();
            GUIBuild();
        }

        [Obsolete("Possibly sets up project, which uses Single Pass")]
        private void GUIBuild()
        {
            bool questNotReady = !QuestSetup.IsQuestProjectReady() && _versions.Contains(BuildVersion.Android2021);

            GUIStyle redTextStyle = new GUIStyle
            {
                normal =
                {
                    textColor = Color.red
                },
                alignment = TextAnchor.MiddleCenter
            };

            if (questNotReady)
            {
                GUILayout.Label("Your project for quest is not set up.", redTextStyle);
                EditorGUILayout.Space(10);

                if (GUILayout.Button("Setup", GUILayout.Height(40)))
                {
                    QuestSetup.CreatePopup();
                }
            }
            else if (!ProjectIsInitialized.Value)
            {
                GUILayout.Label("Your project has not been set up for Beat Saber.", redTextStyle);
                EditorGUILayout.Space(10);

                if (GUILayout.Button("Setup", GUILayout.Height(40)))
                {
                    Initialize.SetupProject();
                }
            }
            else if (_versions.Count > 0)
            {
                if (GUILayout.Button("Build", GUILayout.Height(40)))
                {
                    Build();
                }
            }
        }
        private void GUIVersions()
        {
            EditorGUILayout.LabelField("Versions", _titleStyle, GUILayout.Height(_titleStyle.fontSize * 1.5f));
            VersionToggle("Windows 2019", BuildVersion.Windows2019);
            VersionToggle("Windows 2021", BuildVersion.Windows2021);
            VersionToggle("Android (Quest) 2021", BuildVersion.Android2021);
        }

        private void GUISettings()
        {
            EditorGUILayout.LabelField("Settings", _titleStyle, GUILayout.Height(_titleStyle.fontSize * 1.5f));

            _compressed = EditorGUILayout.Toggle("Compressed", _compressed);
            ProjectBundle.Value = EditorGUILayout.TextField("Bundle Name To Export", ProjectBundle.Value);
            ShouldExportBundleInfo.Value = EditorGUILayout.Toggle("Should Export Bundle Info", ShouldExportBundleInfo.Value);

            if (ShouldExportBundleInfo.Value) {
                ShouldPrettifyBundleInfo.Value = EditorGUILayout.Toggle("Should Prettify Bundle Info", ShouldPrettifyBundleInfo.Value);
            }
        }

        private void GUIQuickBuild()
        {
            EditorGUILayout.LabelField("Quick Build", _titleStyle, GUILayout.Height(_titleStyle.fontSize * 1.5f));
            GUILayout.Label("If you press F5, you can build a version uncompressed for quick iteration.", EditorStyles.label);
            EditorGUILayout.Space(5);

            int selectedVersion = EditorGUILayout.Popup("Working Version", (int)WorkingVersion.Value, VersionTools.GetVersionsStrings());
            WorkingVersion.Value = (BuildVersion)Enum.GetValues(typeof(BuildVersion)).GetValue(selectedVersion);
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
