using System.Diagnostics;
using System.IO;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class Symlink
    {
        public static void MakeSymlink(string target, string dest)
        {
            using (Process myProcess = new Process())
            {
#if UNITY_EDITOR_WIN
                myProcess.StartInfo = new ProcessStartInfo("cmd.exe", $"/k mklink /D \"{dest}\" \"{target}\"");
#elif UNITY_EDITOR_OSX
                string parent = Directory.GetParent(dest).FullName;
                myProcess.StartInfo = new ProcessStartInfo("ln", $"-s \"{target}\" \"{parent}\"");
#endif
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.Verb = "runas";
                myProcess.Start();
            }
        }
    }
}