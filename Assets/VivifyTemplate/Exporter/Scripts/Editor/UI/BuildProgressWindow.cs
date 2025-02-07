using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;
namespace VivifyTemplate.Exporter.Scripts.Editor.UI
{
    public class BuildProgressWindow : EditorWindow
    {
        public SimpleTimer timer = new SimpleTimer();

        public enum BuildState
        {
            InProgress,
            Success,
            Fail
        }

        private readonly List<BuildTask> _individualBuilds = new List<BuildTask>();
        private readonly List<BuildTask> _shaderKeywordsRewriterTasks = new List<BuildTask>();
        private BuildTask _serializeTask;
        private bool _finished = false;
        private BuildSettings _buildSettings;

        private readonly TaskWindowData _individualBuildTaskWindow = new TaskWindowData();
        private readonly TaskWindowData _shaderKeywordRewriterTaskWindow = new TaskWindowData();
        private Vector2 _serializeTaskScrollPosition;

        public delegate void CloseFn();

        public event CloseFn OnClose;

        private void OnDestroy()
        {
            OnClose?.Invoke();
        }


        public BuildTask AddIndividualBuild(BuildVersion version)
        {
            string taskName = "Building " + version;
            BuildTask buildTask = new BuildTask(taskName);
            _individualBuilds.Add(buildTask);
            return buildTask;
        }

        public void AddShaderKeywordsRewriterTask(BuildTask task)
        {
            _shaderKeywordsRewriterTasks.Add(task);
        }

        public BuildTask StartSerialization()
        {
            BuildTask buildTask = new BuildTask("Serialization");
            _serializeTask = buildTask;
            return buildTask;
        }

        public void FinishBuild(BuildSettings buildSettings)
        {
            _buildSettings = buildSettings;
            timer.UpdateElapsed();
            _finished = true;
        }

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            DrawIndividualBuilds();
            DrawShaderKeywordRewriteTasks();
            DrawSerializeTask();
            DrawStatus();
        }

        private void DrawIndividualBuilds()
        {
            DrawTaskWindow("Building Bundles", _individualBuildTaskWindow, _individualBuilds);
        }

        private void DrawShaderKeywordRewriteTasks()
        {
            DrawTaskWindow("Shader Keyword Rewrite Tasks", _shaderKeywordRewriterTaskWindow, _shaderKeywordsRewriterTasks);
        }

        private void DrawSerializeTask()
        {
            float height = 150;
            GUILayout.BeginVertical(GUILayout.Height(height));

            GUILayout.Label("Bundle Info Serialization", EditorStyles.boldLabel);

            if (_serializeTask != null)
            {
                string log = _serializeTask.GetLogger().GetOutput();
                _serializeTaskScrollPosition = EditorGUILayout.BeginScrollView(_serializeTaskScrollPosition, GUILayout.MaxHeight(height));

                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    normal = {
                        textColor = Color.white,  // Override text color
                        background = EditorStyles.textArea.normal.background // Use normal background
                    }
                };

                EditorGUI.BeginDisabledGroup(true); // Disable editing
                EditorGUILayout.TextArea(log, textAreaStyle, GUILayout.ExpandHeight(true));
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndScrollView();
                LogCopyButton(log);
            }

            GUILayout.EndVertical();
        }

        private static void LogCopyButton(string log)
        {
            if (GUILayout.Button("Copy Log"))
            {
                GUIUtility.systemCopyBuffer = log;
            }
        }

        private void DrawStatus()
        {
            if (_finished)
            {
                float elapsed = Mathf.Round(timer.GetElapsed() * 100) / 100;
                string message = $"Build done in {elapsed}s!";
                GUILayout.Label(message, EditorStyles.largeLabel);

                if (GUILayout.Button("Open Output Folder"))
                {
                    IOHelper.OpenFolder(_buildSettings.OutputDirectory);
                }
            }
            else
            {
                float elapsed = Mathf.Floor(timer.UpdateElapsed());
                string message = $"Building ({elapsed}s elapsed)...";
                GUILayout.Label(message, EditorStyles.largeLabel);
            }
        }

        private string GetEllipses()
        {
            int dotAmount = Mathf.FloorToInt(timer.GetElapsed() * 2f) % 3 + 1;

            switch (dotAmount)
            {
                case 1: return ".";
                case 2: return "..";
                case 3: return "...";
                default: return "...";
            };
        }

        private Color GetTaskColor(BuildTask buildTask)
        {
            switch (buildTask.GetState())
            {
                case BuildState.Success: return Color.green;
                case BuildState.Fail: return Color.red;
                case BuildState.InProgress:
                default: return Color.white;
            };
        }

        private void DrawTaskWindow(string windowName, TaskWindowData data, List<BuildTask> buildTasks)
        {
            GUILayout.Label(windowName, EditorStyles.boldLabel);

            float width = 300;
            float height = 150;

            EditorGUILayout.BeginHorizontal(new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fixedHeight = height
            });

            // Tasks
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            data.TaskScrollPosition = EditorGUILayout.BeginScrollView(data.TaskScrollPosition, GUILayout.Width(width));

            for (int i = 0; i < buildTasks.Count; i++)
            {
                DrawTaskButton(data, buildTasks, i);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Task Content
            EditorGUILayout.BeginVertical();

            if (data.SelectedTaskIndex != -1 && buildTasks.Count > data.SelectedTaskIndex)
            {
                BuildTask task = buildTasks[data.SelectedTaskIndex];
                string log = task.GetLogger().GetOutput();

                DrawTaskLog(data, task, log, height, width);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTaskButton(TaskWindowData data, List<BuildTask> buildTasks, int i)
        {
            bool isSelected = data.SelectedTaskIndex == i;
            BuildTask selectedTask = buildTasks[i];

            GUIStyle selectedStyle = new GUIStyle(EditorStyles.miniButton);
            GUIStyle unselectedStyle = new GUIStyle(EditorStyles.miniButton)
            {
                normal =
                {
                    background = Texture2D.blackTexture,
                },
            };

            GUIStyle buttonStyle = isSelected ? selectedStyle : unselectedStyle;

            Color taskColor = GetTaskColor(selectedTask);
            buttonStyle.normal.textColor = taskColor;
            buttonStyle.hover.textColor = taskColor;

            if (GUILayout.Button(selectedTask.GetName(), buttonStyle))
            {
                data.SelectedTaskIndex = i;
            }
        }

        private void DrawTaskLog(TaskWindowData data, BuildTask task, string log, float height, float width)
        {
            if (task.GetState() == BuildState.InProgress && !task.GetLogger().IsEmpty())
            {
                log += "\n" + GetEllipses();
            }

            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                normal = {
                    textColor = Color.white,  // Override text color
                    background = EditorStyles.textArea.normal.background // Use normal background
                }
            };

            float contentHeight = Math.Max(height, textAreaStyle.CalcHeight(new GUIContent(log), width));

            // Convert Y position (distance from top -> distance from bottom)
            data.ContentScrollPosition.y = contentHeight + data.ContentScrollPosition.y;

            data.ContentScrollPosition = EditorGUILayout.BeginScrollView(data.ContentScrollPosition, GUILayout.MaxHeight(height));

            // Convert Y position (distance from bottom -> distance from top)
            data.ContentScrollPosition.y -= contentHeight;

            EditorGUI.BeginDisabledGroup(true); // Disable editing
            EditorGUILayout.TextArea(log, textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
            LogCopyButton(log);
        }

        public static BuildProgressWindow CreatePopup()
        {
            BuildProgressWindow window = CreateInstance<BuildProgressWindow>();
            window.titleContent = new GUIContent("Build Progress");
            window.minSize = new Vector2(800, 150 + 150 + 150 + 100);
            window.maxSize = window.minSize;
            window.ShowUtility();
            window.timer.Reset();
            return window;
        }
    }
}
