using AssetsTools.NET.Extra;
using System;
using System.Threading.Tasks;
using UnityEngine;

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
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }
}