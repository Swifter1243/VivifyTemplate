using AssetsTools.NET.Extra;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class Symlink
    {
        public static void MakeSymlink(string target, string dest)
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo = new ProcessStartInfo("cmd.exe", $"/k mklink /D \"{dest}\" \"{target}\"");
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.Verb = "runas";
                //myProcess.StartInfo.RedirectStandardOutput = true;

                myProcess.Start();

                //var read = await myProcess.StandardOutput.ReadToEndAsync();
                //myProcess.WaitForExit();
                //UnityEngine.Debug.Log(read);
            }
        }
    }
}