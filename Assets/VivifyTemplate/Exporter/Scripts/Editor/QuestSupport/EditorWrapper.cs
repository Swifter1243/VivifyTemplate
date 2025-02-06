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

                    QuestSetup.State = BackgroundTaskState.Idle;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static Task InstallPackages(string editor, string project)
        {
            try
            {
                QuestSetup.State = BackgroundTaskState.AddingPackages;
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = editor;
                    process.StartInfo.Arguments = $"-projectPath \"{project}\" -executeMethod VivifyTemplate.Exporter.Scripts.Editor.QuestSupport.InstallPackages.Setup";

                    process.Start();

                    process.WaitForExit();

                    QuestSetup.State = BackgroundTaskState.Idle;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return Task.CompletedTask;
        }

        public static Task BuildProject(string editor, string project)
        {
            try
            {;
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = editor;
                    process.StartInfo.Arguments = $"-projectPath \"{project}\" -executeMethod VivifyTemplate.Exporter.Scripts.Editor.QuestSupport.BuildProject.Build";

                    process.Start();

                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return Task.CompletedTask;
        }
    }
}
