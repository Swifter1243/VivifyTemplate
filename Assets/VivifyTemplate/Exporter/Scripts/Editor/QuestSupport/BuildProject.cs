using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class BuildProject
{
    public static void Build()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        Debug.Log("GetCommandLineArgs: " + string.Join(", ", arguments));
    }
}
