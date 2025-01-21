using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;

public class QuestSetup : EditorWindow
{

    private static readonly string QuestProjectPlayerPrefsKey = "questPath";
    private static readonly string UnityEditorPlayerPrefsKey = "unityEditor";

    private static readonly string UnityHubPath = "C:/Program Files/Unity Hub/Unity Hub.exe"; //This *should* be the same for everyone, need more testing

    public static string ProjectPath
    {
        get => UnityEngine.PlayerPrefs.GetString(QuestProjectPlayerPrefsKey, "");
        set => UnityEngine.PlayerPrefs.SetString(QuestProjectPlayerPrefsKey, value);
    }

    public static string UnityEditor
    {
        get => UnityEngine.PlayerPrefs.GetString(UnityEditorPlayerPrefsKey, "");
        set => UnityEngine.PlayerPrefs.SetString(UnityEditorPlayerPrefsKey, value);
    }

    private ConcurrentDictionary<string, string> _unityVersions = new ConcurrentDictionary<string, string>() { };

    private async Task GetUnityVersions()
    {
        _unityVersions.Clear();
        using (Process myProcess = new Process())
        {
            UnityEngine.Debug.Log("skibidi");

            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.FileName = UnityHubPath;
            myProcess.StartInfo.Arguments = "-- --headless editors --installed";

            myProcess.Start();

            var read = await myProcess.StandardOutput.ReadToEndAsync();
            myProcess.WaitForExit();
            UnityEngine.Debug.Log(read);

            var lines = read.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var split = line.Split(',');
                if (split.Length != 2) continue;

                _unityVersions.TryAdd(split[0].Trim(), split[1].Trim().Substring(13));
            }
            //TODO: Set dirty here somehow;
        }
    }

    private void OnEnable()
    {
        if (UnityEditor == "")
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                await GetUnityVersions();

            }).Start();
        }
    }

    private bool EditorChecks()
    {
        if (UnityEditor != "") //debug
        {
            if (GUILayout.Button("Reset"))
            {
                UnityEditor = "";
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await GetUnityVersions();

                }).Start();
            }
        }
        if (UnityEditor == "" && _unityVersions.Count == 0)
        {
            EditorGUILayout.LabelField("Searching for Unity editors...", EditorStyles.boldLabel);
            return false;
        }
        if (UnityEditor == "" && _unityVersions.Count > 0)
        {
            string foundVersion;
            if (_unityVersions.TryGetValue("2021.3.16f1", out foundVersion))
            {
                UnityEngine.Debug.Log(foundVersion);
                UnityEditor = foundVersion;
            }
            else
            {
                EditorGUILayout.LabelField("Could not find Unity Editor version 2021.3.16f1. This version is required to build quest bundles.");
                if (GUILayout.Button("Download"))
                {

                }
                return false;
            }
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
        var verticalStyle = new GUIStyle(GUI.skin.button);
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
            
        };

        EditorGUILayout.BeginVertical(verticalStyle);

        EditorGUILayout.LabelField("What?", headerStyle);
        EditorGUILayout.TextArea("To build vivify bundles for quest predictably, accurately, and easily, you need to build with Unity 2021.3.16f1", paragraphStyle);
        EditorGUILayout.TextArea("Luckily, this template will handle all of that for you! It will setup the project for you, copy your assets, and build your bundle all on its own!", paragraphStyle);

        EditorGUILayout.EndVertical();
    }

    private void OnGUI()
    {
        if (!EditorChecks()) return;

        var style = new GUIStyle(GUI.skin.scrollView);

        Vector2 scrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, 0), style);

        Header();
        Info();

        EditorGUILayout.EndScrollView();
    }

    [MenuItem("Vivify/Quest Setup")]
    private static void CreatePopup()
    {
        QuestSetup window = CreateInstance<QuestSetup>();
        window.titleContent = new GUIContent("Setup Quest Project");
        window.position = new Rect(300, 300, 800, 900);
        window.minSize = new Vector2(800, 900);
        window.ShowUtility();
    }
}
