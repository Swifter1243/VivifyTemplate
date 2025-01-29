using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.IO;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public class QuestSetup : EditorWindow
    {
        public static BackgroundTaskState State = BackgroundTaskState.Idle;

        private void OnEnable()
        {
            if (QuestPreferences.UnityEditor == "")
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await HubWrapper.GetUnityVersions();

                }).Start();
            }
        }

        private bool EditorChecks()
        {
            if (QuestPreferences.UnityEditor != "") //debug
            {
                if (GUILayout.Button("Reset"))
                {
                    QuestPreferences.UnityEditor = "";
                    new Thread(async () =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        await HubWrapper.GetUnityVersions();

                    }).Start();
                }
            }

            if (State == BackgroundTaskState.SearchingEditors)
            {
                EditorGUILayout.LabelField("Searching for Unity editors...", EditorStyles.boldLabel);
                return false;
            }

            if (State == BackgroundTaskState.Idle && QuestPreferences.UnityEditor == "")
            {
                string foundVersion;
                if (HubWrapper.TryGetUnityEditor("2021.3.16f1", out foundVersion))
                {
                    UnityEngine.Debug.Log(foundVersion);
                    QuestPreferences.UnityEditor = foundVersion;
                }
                else
                {
                    EditorGUILayout.LabelField(
                        "Could not find Unity Editor version 2021.3.16f1. This version is required to build quest bundles.");
                    if (GUILayout.Button("Download"))
                    {
                        HubWrapper.DownloadUnity2021();
                    }

                    return false;
                }
            }

            var editorDirectory = Path.GetDirectoryName(QuestPreferences.UnityEditor);
            var androidPlaybackEngine = Path.Combine(editorDirectory, "Data", "PlaybackEngines", "AndroidPlayer");
            if (!Directory.Exists(androidPlaybackEngine))
            {
                EditorGUILayout.LabelField(
                    "Could not find the Android Build Module for Unity Editor version 2021.3.16f1. This version is required to build quest bundles.");
                if (GUILayout.Button("Download Android Build Module"))
                {
                    HubWrapper.DownloadUnity2021Android();
                }

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

            EditorGUILayout.LabelField("What?", headerStyle);
            GUILayout.Space(15);
            EditorGUILayout.LabelField(
                "To build vivify bundles for quest predictably, accurately, and easily, you need to build with Unity 2021.3.16f1",
                paragraphStyle);
            EditorGUILayout.LabelField(
                "<b><i>Luckily, this template will handle all of that for you!</i></b> It will setup the project for you, link your assets, and build your bundle all on its own!",
                paragraphStyle);

            EditorGUILayout.EndVertical();
        }

        private bool MakeProject()
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

            EditorGUILayout.BeginVertical(verticalStyle);

            EditorGUILayout.LabelField("Make Project", headerStyle);
            GUILayout.Space(15);
            EditorGUILayout.LabelField(
                "You will be prompted to pick a directory where your project will be created in, it's best to keep this the same across projects for organization.",
                paragraphStyle);
            GUI.enabled = !hasProject;
            if (GUILayout.Button("Create"))
            {
                var path = EditorUtility.OpenFolderPanel("Select Directory to Create a Project", "", "");
                if (path != "")
                {
                    var projectName = Directory.GetParent(Application.dataPath).Name + "_Quest";
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

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            return hasProject;
        }

        private static bool IsDirectoryNotEmpty(string path)
        {
            var items = Directory.EnumerateFileSystemEntries(path);
            using (var en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

        private bool MakeSymlink()
        {
            var questAssets = Path.Combine(QuestPreferences.ProjectPath, "Assets");
            if (!Directory.Exists(questAssets)) return false;
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

            EditorGUILayout.LabelField("Make Symlink", headerStyle);
            GUILayout.Space(15);
            EditorGUILayout.LabelField(
                "You will be asked for admin permissions to create a symlink between your project into the quest project.",
                paragraphStyle);
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

        private bool InstallPackages()
        {
            var questPackages = Path.Combine(QuestPreferences.ProjectPath, "Library/PackageCache/com.unity.xr.openxr@1.14.0");
            var hasPackages = Directory.Exists(questPackages);

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

            EditorGUILayout.LabelField("Install Packages", headerStyle);
            GUILayout.Space(15);
            EditorGUILayout.LabelField(
                "This will install XR Plugin Management and Oculus Integration packages into your project.",
                paragraphStyle);
            GUI.enabled = !hasPackages;
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

            if (IsQuestProjectReady())
            {
                EditorGUILayout.LabelField("<color=#88FF88>You are ready to build</color>", style);
            }
            else
            {
                EditorGUILayout.LabelField("<color=#FF8888>You are <i>not</i> ready to build</color>", style);
            }
        }

        public static bool IsQuestProjectReady()
        {
            return Directory.Exists(QuestPreferences.ProjectPath) &&
                   !IsDirectoryNotEmpty(Path.Combine(QuestPreferences.ProjectPath, "Assets")) &&
                   Directory.Exists(Path.Combine(QuestPreferences.ProjectPath, "Library/PackageCache/com.unity.xr.openxr@1.14.0"));
        }
        
        Vector2 _scrollPos = Vector2.zero;

        private void OnGUI()
        {
            EditorGUILayout.LabelField(State.ToString());
            if (!EditorChecks()) return;

            var style = new GUIStyle(GUI.skin.scrollView);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, style);

            Header();
            Info();
            GUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            if (!MakeProject())
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


            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndScrollView();
            Footer();
        }

        [MenuItem("Vivify/Quest Setup")]
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
