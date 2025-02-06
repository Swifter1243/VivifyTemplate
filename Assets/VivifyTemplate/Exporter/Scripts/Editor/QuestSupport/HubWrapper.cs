using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using System.IO;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class HubWrapper
    {
        private static ConcurrentDictionary<string, string> _unityVersions = new ConcurrentDictionary<string, string>() { };

        public static async Task GetUnityVersions()
        {
            QuestSetup.State = BackgroundTaskState.SearchingEditors;
            _unityVersions.Clear();
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo.FileName = QuestPreferences.UnityHubPath;
                myProcess.StartInfo.Arguments = "-- --headless editors --installed";

                myProcess.Start();

                var read = await myProcess.StandardOutput.ReadToEndAsync();
                myProcess.WaitForExit();

                var lines = read.Split('\n');
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var split = line.Split(',');
                    if (split.Length != 2) continue;
                    _unityVersions.TryAdd(split[0].Trim(), split[1].Trim().Substring(13));
                }

                QuestSetup.State = BackgroundTaskState.Idle;
            }
        }

        public static bool FinishedGettingEditors() => _unityVersions.Count > 0;

        public static bool TryGetUnityEditor(string version, out string path) =>
            _unityVersions.TryGetValue(version, out path);
    }
}