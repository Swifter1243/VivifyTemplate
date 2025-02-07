using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Builder;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Editor.Project;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;
namespace VivifyTemplate.Exporter.Scripts.Editor.UI
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

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            GUIVersions();
            EditorGUILayout.Space(30);
            GUISettings();
            EditorGUILayout.Space(30);
            GUIQuickBuild();
            EditorGUILayout.Space(30);

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            if (UpdateChecker.PossibleUpdate.HasValue)
            {
                GUILayout.Space(30);
                GUIUpdate(UpdateChecker.PossibleUpdate.Value);
            }

            GUILayout.FlexibleSpace();
            GUIBuild();
        }

        private void GUIUpdate(UpdateChecker.UpdateAvailableData updateAvailableData)
        {
            GUIStyle style = new GUIStyle
            {
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 15
            };

            GUILayout.Label("A new update is available.", style, GUILayout.Height(style.fontSize * 1.5f));

            string updateString = $"{updateAvailableData.OldVersion} -> {updateAvailableData.NewVersion}";
            style.fontStyle = FontStyle.Normal;
            GUILayout.Label(updateString, style, GUILayout.Height(style.fontSize * 1.5f));

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Update", GUILayout.Height(40), GUILayout.Width(160)))
            {
                Application.OpenURL("https://github.com/Swifter1243/VivifyTemplate/releases/latest");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            style.normal.textColor = new Color(1, 1f, 0.5f);
            style.fontSize = 12;
            GUILayout.Space(15);
            GUILayout.Label("Delete old module folders before adding the updated ones.", style, GUILayout.Height(style.fontSize * 1.5f));
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
                    SetupProject.Setup();
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

            _compressed = EditorGUILayout.Toggle(new GUIContent("Compressed", "Whether to compress the bundle. This will take longer, but will significantly reduce file size."), _compressed);
            ProjectBundle.Value = EditorGUILayout.TextField(new GUIContent("Bundle To Export", "Assets attached to this bundle name will be exported."), ProjectBundle.Value);
            ShouldExportBundleInfo.Value = EditorGUILayout.Toggle(new GUIContent("Export Bundle Info", "Whether to export the bundleinfo.json file."), ShouldExportBundleInfo.Value);

            if (ShouldExportBundleInfo.Value) {
                ShouldPrettifyBundleInfo.Value = EditorGUILayout.Toggle(new GUIContent("Prettify Bundle Info", "Whether to format the bundleinfo.json with indents and new lines."), ShouldPrettifyBundleInfo.Value);
            }

            GUIOutputDirectory();
        }

        private static void GUIOutputDirectory()
        {

            GUILayout.BeginHorizontal();
            if (OutputDirectory.IsSet())
            {
                OutputDirectory.Value = EditorGUILayout.TextField(new GUIContent("Output Directory", "Which folder to output the bundles (and bundleinfo.json)."), OutputDirectory.Value);
            }
            else
            {
                GUILayout.Label(new GUIContent("Output Directory", "Which folder to output the bundles (and bundleinfo.json)."), EditorStyles.label);
                GUILayout.FlexibleSpace();
            }
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                OutputDirectory.SetFromExplorer();
            }
            GUILayout.EndHorizontal();
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
