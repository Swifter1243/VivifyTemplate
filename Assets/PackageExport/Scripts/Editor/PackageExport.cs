using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEditor.Build.Content;

public class PackageExport
{
    static void ExportPackage(string[] assetPaths, string packageName)
    {
        string outputPath = "Assets/PackageExport/Output";
        string packageFile = $"{packageName}.unitypackage";
        string packagePath = Path.Combine(outputPath, packageFile);
        AssetDatabase.ExportPackage(assetPaths, packagePath, ExportPackageOptions.Recurse);
        Debug.Log($"'{packageFile}' was exported to '{outputPath}'");
    }

    [MenuItem("Package Export/Run")]
    static void Run()
    {
        ExportAll();
        ExportExporter();
        ExportExamples();
    }

    static void ExportAll()
    {
        string[] assetPaths = new string[]
        {
            "Assets/VivifyTemplate"
        };
        ExportPackage(assetPaths, "VivifyTemplate-All");
    }

    static void ExportExporter()
    {
        string[] assetPaths = new string[]
        {
            "Assets/VivifyTemplate/Dependencies",
            "Assets/VivifyTemplate/Scripts"
        };
        ExportPackage(assetPaths, "VivifyTemplate-Exporter");
    }

    static void ExportExamples()
    {
        string[] assetPaths = new string[]
        {
            "Assets/VivifyTemplate/Materials",
            "Assets/VivifyTemplate/Models",
            "Assets/VivifyTemplate/Scenes",
            "Assets/VivifyTemplate/Shaders",
            "Assets/VivifyTemplate/Textures",
        };
        ExportPackage(assetPaths, "VivifyTemplate-Examples");
    }
}
