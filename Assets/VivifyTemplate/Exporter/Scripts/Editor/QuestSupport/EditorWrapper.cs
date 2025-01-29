using AssetsTools.NET.Extra;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class EditorWrapper
    {
        public static async Task MakeProject(string path, string editor)
        {
            try
            {
                QuestSetup.State = BackgroundTaskState.CreatingProject;
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = editor;
                    process.StartInfo.Arguments = $"-createProject \"{path}\" -quit";

                    process.Start();

                    var read = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();
                    UnityEngine.Debug.Log(read);

                    QuestSetup.State = BackgroundTaskState.Idle;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static async Task InstallPackages()
        {
            try
            {
                QuestSetup.State = BackgroundTaskState.AddingPackages;
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = QuestPreferences.UnityEditor;
                    process.StartInfo.Arguments = $"-projectPath \"{QuestPreferences.ProjectPath}\" -executeMethod InstallPackages.Setup -quit";

                    process.Start();

                    var read = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();
                    UnityEngine.Debug.Log(read);

                    QuestSetup.State = BackgroundTaskState.Idle;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static async Task BuildProject()
        {
            try
            {
                QuestSetup.State = BackgroundTaskState.AddingPackages;
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = QuestPreferences.UnityEditor;
                    process.StartInfo.Arguments = $"-projectPath \"{QuestPreferences.ProjectPath}\" -executeMethod BuildProject.Build";

                    process.Start();

                    var read = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();
                    UnityEngine.Debug.Log(read);

                    QuestSetup.State = BackgroundTaskState.Idle;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}