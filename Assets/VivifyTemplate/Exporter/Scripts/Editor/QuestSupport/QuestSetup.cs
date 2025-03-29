using System;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using JetBrains.Annotations;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public class QuestSetup : EditorWindow
    {
        public static BackgroundTaskState State = BackgroundTaskState.Idle;

        private bool EditorChecks()
        {
            if (!Directory.Exists(Path.GetDirectoryName(QuestPreferences.UnityHubPath)))
            {
                EditorGUILayout.LabelField("You do not have Unity Hub installed. This is required for managing your editor installations.", EditorStyles.boldLabel);
                if (GUILayout.Button("To download, click the first \"Download\" button."))
                {
                    Application.OpenURL("https://unity.com/download");
                }
                return false;
            }

            if ((QuestPreferences.UnityEditor == "" || !Directory.Exists(Path.GetDirectoryName(QuestPreferences.UnityEditor))) &&
                State == BackgroundTaskState.Idle && !HubWrapper.FinishedGettingEditors())
            {
                Task.Run(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await HubWrapper.GetUnityVersions();
                });
            }

            if (State == BackgroundTaskState.SearchingEditors)
            {
                EditorGUILayout.LabelField("Searching for Unity editors... (PLEASE WAIT)", EditorStyles.boldLabel);
                return false;
            }

            if (State == BackgroundTaskState.Idle && (QuestPreferences.UnityEditor == "" || !Directory.Exists(Path.GetDirectoryName(QuestPreferences.UnityEditor))))
            {
                if (HubWrapper.TryGetUnityEditor("2021.3.16f1", out var foundVersion) && Directory.Exists(Path.GetDirectoryName(foundVersion)))
                {
                    QuestPreferences.UnityEditor = foundVersion;
                }
                else
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 15,
                    };

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Could not find Unity Editor version 2021.3.16f1. This version is required to build quest bundles.", style, GUILayout.Height(style.fontSize * 2));
                    EditorGUILayout.Space(10);

                    GUI.enabled = State != BackgroundTaskState.DownloadingEditor;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(80);
                    if (GUILayout.Button("Download", GUILayout.Height(30)))
                    {
                        Application.OpenURL("unityhub://2021.3.16f1/4016570cf34f");
                    }
                    if (GUILayout.Button("Rescan", GUILayout.Height(30)))
                    {
                        Task.Run(async () =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            await HubWrapper.GetUnityVersions();
                        });
                    }
                    GUILayout.Space(80);
                    EditorGUILayout.EndHorizontal();

                    style.normal.textColor = Color.red;
                    style.fontStyle = FontStyle.Bold;

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Make sure you check the box that says \"Android Build Support\" when installing.", style, GUILayout.Height(style.fontSize * 2));
                    EditorGUILayout.Space(10);

                    GUI.enabled = true;
                    return false;
                }
            }

            var editorDirectory = Path.GetDirectoryName(QuestPreferences.UnityEditor);
            if (State == BackgroundTaskState.Idle && editorDirectory != null)
            {
                var androidPlaybackEngine = Path.Combine(editorDirectory, "Data", "PlaybackEngines", "AndroidPlayer");
                if (!Directory.Exists(androidPlaybackEngine))
                {
                    EditorGUILayout.LabelField(
                        "Could not find the Android Build Module for Unity Editor version 2021.3.16f1. This module is required to build quest bundles.");
                    GUI.enabled = State != BackgroundTaskState.DownloadingAndroidBuildSupport;
                    if (GUILayout.Button("Download Android Build Module. Under \"Component Installers\""))
                    {
                        Application.OpenURL("https://unity.com/releases/editor/whats-new/2021.3.16#installers");
                    }

                    GUI.enabled = true;
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private void Header()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 40,
                richText = true,
                fixedHeight = 50
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("<color=#E84855>Quest</color> <color=#272635>Setup</color>", style);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(40);
        }

        private void Info()
        {
            var verticalStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 10, 10),
            };
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontStyle = FontStyle.Bold,
                fontSize = 20
            };
            var paragraphStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 14,
                wordWrap = true
            };

            EditorGUILayout.BeginVertical(verticalStyle);

            EditorGUILayout.LabelField("What Is This?", headerStyle, GUILayout.Height(headerStyle.fontSize * 2));
            EditorGUILayout.LabelField(
                "To build vivify bundles for quest predictably, accurately, and easily, you need to build with Unity 2021.3.16f1",
                paragraphStyle);
            EditorGUILayout.LabelField(
                "<b><i>Luckily, this template will handle all of that for you!</i></b> It will setup the project for you, link your assets, and build your bundle all on its own!",
                paragraphStyle);

            EditorGUILayout.EndVertical();
        }

        private bool InitializeProject()
        {
            var hasProject = QuestPreferences.ProjectPath != "" && Directory.Exists(QuestPreferences.ProjectPath);

            var verticalStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontStyle = FontStyle.Bold,
                fontSize = 20
            };
            var paragraphStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 14,
                wordWrap = true
            };
            var centeredLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            EditorGUILayout.BeginVertical(verticalStyle);

            EditorGUILayout.LabelField("Initialize Project", headerStyle, GUILayout.Height(headerStyle.fontSize * 2));
            EditorGUILayout.LabelField(
                "You will be prompted to pick a directory where your project will be created in. The folder you select will create a new folder inside which is the Unity 2021 project.",
                paragraphStyle);
            GUILayout.Space(10);
            GUI.enabled = !hasProject;

            if (GUILayout.Button("Create"))
            {
                var path = EditorUtility.OpenFolderPanel("Select Directory to Create a Project", "", "");
                if (path != "")
                {
                    var projectName = Directory.GetParent(Application.dataPath)?.Name + "_Quest";
                    var destinationPath = Path.Combine(path, projectName);
                    if (Directory.Exists(destinationPath))
                    {
                        Debug.LogError($"Folder at {destinationPath} already exists!");

                        EditorGUILayout.EndVertical();
                        return false;
                    }

                    var editorPath = QuestPreferences.UnityEditor;

                    Task.Run(async () =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        await EditorWrapper.MakeProject(destinationPath, editorPath);
                    });

                    QuestPreferences.ProjectPath = destinationPath;
                }
            }

            GUILayout.Space(5);
            EditorGUILayout.LabelField("OR", centeredLabelStyle);
            GUILayout.Space(5);

            if (GUILayout.Button("Locate"))
            {
                var path = EditorUtility.OpenFolderPanel("Select Directory for Existing Project", "", "");
                if (path != "")
                {
                    bool isUnityProject = IOHelper.IsUnityProject(path);
                    if (!isUnityProject)
                    {
                        throw new Exception($"The path ${path} doesn't seem to be a Unity project.");
                    }

                    QuestPreferences.ProjectPath = path;
                }
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            return hasProject;
        }

        private static bool IsDirectoryNotEmpty(string path)
        {
            if (!Directory.Exists(path)) return false;
            var items = Directory.EnumerateFileSystemEntries(path);
            using (var en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

        private bool MakeSymlink()
        {
            var questAssets = Path.Combine(QuestPreferences.ProjectPath, "Assets");
            if (!Directory.Exists(questAssets) || State == BackgroundTaskState.CreatingProject) return false;
            var hasSymlink = !IsDirectoryNotEmpty(questAssets);

            var verticalStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontStyle = FontStyle.Bold,
                fontSize = 20
            };
            var paragraphStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 14,
                wordWrap = true
            };

            EditorGUILayout.BeginVertical(verticalStyle);

            EditorGUILayout.LabelField("Make Symlink", headerStyle, GUILayout.Height(headerStyle.fontSize * 2));
            EditorGUILayout.LabelField(
                "You will be asked for admin permissions to create a symlink between your project into the quest project.",
                paragraphStyle);
            GUILayout.Space(10);
            GUI.enabled = !hasSymlink;
            if (GUILayout.Button("Create"))
            {
                Directory.Delete(questAssets);
                Symlink.MakeSymlink(Application.dataPath, questAssets);
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            return hasSymlink;
        }

        [CanBeNull]
        private static string GetOpenXRPackageDirectory()
        {
            var packageCache = Path.Combine(QuestPreferences.ProjectPath, "Library/PackageCache");
            if (!Directory.Exists(packageCache)) return null;
            var directories = Directory.GetDirectories(packageCache);
            foreach (var directory in directories)
            {
                if (directory.Contains("com.unity.xr.openxr"))
                {
                    return directory;
                }
            }

            return null;
        }

        private bool InstallPackages()
        {
            var openXRPackageDirectory = GetOpenXRPackageDirectory();
            var hasPackages = openXRPackageDirectory != null && Directory.Exists(openXRPackageDirectory);

            var verticalStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontStyle = FontStyle.Bold,
                fontSize = 20
            };
            var paragraphStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                fontSize = 14,
                wordWrap = true
            };

            EditorGUILayout.BeginVertical(verticalStyle);

            EditorGUILayout.LabelField("Install Packages", headerStyle, GUILayout.Height(headerStyle.fontSize * 2));
            EditorGUILayout.LabelField(
                "This will install XR Plugin Management and Oculus Integration packages into your project.",
                paragraphStyle);
            GUI.enabled = !hasPackages;
            GUILayout.Space(10);
            if (GUILayout.Button("Install"))
            {
                string project = QuestPreferences.ProjectPath;
                string editor = QuestPreferences.UnityEditor;
                Task.Run(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await EditorWrapper.InstallPackages(editor, project);
                });
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            return hasPackages;
        }

        private void Footer()
        {
            var style = new GUIStyle()
            {
                fontSize = 15,
                richText = true,
                padding = new RectOffset(10, 10, -10, 0)
            };

            EditorGUILayout.LabelField(
                IsQuestProjectReady()
                    ? "<color=#88FF88>You are ready to build</color>"
                    : "<color=#FF8888>You are <i>not</i> ready to build</color>", style);
        }

        public static bool IsQuestProjectReady()
        {
            var openXRPackageDirectory = GetOpenXRPackageDirectory();
            return Directory.Exists(QuestPreferences.ProjectPath) &&
                   !IsDirectoryNotEmpty(Path.Combine(QuestPreferences.ProjectPath, "Assets")) &&
                   openXRPackageDirectory != null &&
                   Directory.Exists(openXRPackageDirectory) &&
                   File.Exists(QuestPreferences.UnityEditor);
        }

        Vector2 _scrollPos = Vector2.zero;

        private void OnGUI()
        {
            if (!EditorChecks()) return;

            var style = new GUIStyle(GUI.skin.scrollView);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, style);

            Header();
            Info();
            GUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            if (!InitializeProject())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                return;
            }

            if (!MakeSymlink())
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (!InstallPackages())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                return;
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
            Footer();
        }

        public static void CreatePopup()
        {
            var window = CreateInstance<QuestSetup>();
            window.titleContent = new GUIContent("Setup Quest Project");
            window.position = new Rect(300, 300, 800, 900);
            window.minSize = new Vector2(800, 900);
            window.ShowUtility();
        }
    }
}
