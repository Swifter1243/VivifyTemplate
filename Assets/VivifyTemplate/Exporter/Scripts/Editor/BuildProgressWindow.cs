using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
    public class BuildProgressWindow : EditorWindow
    {
        public enum BuildState
        {
            InProgress,
            Success,
            Fail
        }

        private readonly List<BuildTask> _individualBuilds = new List<BuildTask>();
        private readonly List<BuildTask> _shaderKeywordsRewriterTasks = new List<BuildTask>();
        private BuildTask _serializeTask;
        private string _finishMessage = string.Empty;

        private readonly TaskWindowData _individualBuildTaskWindow = new TaskWindowData();
        private readonly TaskWindowData _shaderKeywordRewriterTaskWindow = new TaskWindowData();
        private Vector2 _serializeTaskScrollPosition;

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

        public void FinishBuild(string message)
        {
            _finishMessage = message;
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
            }

            GUILayout.EndVertical();
        }

        private void DrawStatus()
        {
            bool finished = _finishMessage != string.Empty;

            if (finished)
            {
                GUILayout.Label(_finishMessage, EditorStyles.largeLabel);
            }
            else
            {
                int dotAmount = Mathf.FloorToInt(Time.realtimeSinceStartup) % 3 + 1;
                string message = "Building";

                for (int i = 0; i < dotAmount; i++)
                {
                    message += ".";
                }

                GUILayout.Label(message, EditorStyles.largeLabel);
            }
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

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Task Content
            EditorGUILayout.BeginVertical();

            if (data.SelectedTaskIndex != -1 && buildTasks.Count > data.SelectedTaskIndex)
            {
                BuildTask task = buildTasks[data.SelectedTaskIndex];
                string log = task.GetLogger().GetOutput();
                data.ContentScrollPosition = EditorGUILayout.BeginScrollView(data.ContentScrollPosition, GUILayout.MaxHeight(height));

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
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static BuildProgressWindow CreatePopup()
        {
            BuildProgressWindow window = CreateInstance<BuildProgressWindow>();
            window.titleContent = new GUIContent("Build Progress");
            window.minSize = new Vector2(800, 150 + 150 + 150 + 100);
            window.maxSize = window.minSize;
            window.ShowUtility();
            return window;
        }
    }
}
