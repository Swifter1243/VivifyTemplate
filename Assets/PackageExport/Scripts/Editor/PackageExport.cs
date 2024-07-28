using System.IO;
using UnityEditor;
using UnityEngine;

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
        ExportCgIncludes();
    }

    static void ExportAll()
    {
        string[] assetPaths = {
            "Assets/VivifyTemplate"
        };
        ExportPackage(assetPaths, "VivifyTemplate-All");
    }

    static void ExportExporter()
    {
        string[] assetPaths = {
            "Assets/VivifyTemplate/Exporter",
        };
        ExportPackage(assetPaths, "VivifyTemplate-Exporter");
    }

    static void ExportExamples()
    {
        string[] assetPaths = {
            "Assets/VivifyTemplate/Examples",
            "Assets/VivifyTemplate/CGIncludes",
        };
        ExportPackage(assetPaths, "VivifyTemplate-Examples");
    }

    static void ExportCgIncludes()
    {
        string[] assetPaths = {
            "Assets/VivifyTemplate/CGIncludes",
        };
        ExportPackage(assetPaths, "VivifyTemplate-CGIncludes");
    }
}
