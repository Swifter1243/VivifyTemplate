using System.IO;
using UnityEditor;
using UnityEngine;

namespace PackageExport.Scripts.Editor
{
    public static class PackageExport
    {
        private const string OUTPUT_PATH = "Assets/PackageExport/Output";

        private static void ExportPackage(string[] assetPaths, string packageName)
        {
            string packageFile = $"{packageName}.unitypackage";
            string packagePath = Path.Combine(OUTPUT_PATH, packageFile);
            AssetDatabase.ExportPackage(assetPaths, packagePath, ExportPackageOptions.Recurse);
            Debug.Log($"'{packageFile}' was exported to '{OUTPUT_PATH}'");
        }

        private static void OpenFolderInProject(string projectPath)
        {
            string absolutePath = Path.GetFullPath(projectPath);
            Application.OpenURL($"file://{absolutePath}");
        }

        [MenuItem("Package Export/Run")]
        public static void Run()
        {
            ExportAll();
            ExportExporter();
            ExportExamples();
            ExportCgIncludes();
            OpenFolderInProject(OUTPUT_PATH);
        }

        private static void ExportAll()
        {
            string[] assetPaths = {
                "Assets/VivifyTemplate"
            };
            ExportPackage(assetPaths, "VivifyTemplate-All");
        }

        private static void ExportExporter()
        {
            string[] assetPaths = {
                "Assets/VivifyTemplate/Exporter",
            };
            ExportPackage(assetPaths, "VivifyTemplate-Exporter");
        }

        private static void ExportExamples()
        {
            string[] assetPaths = {
                "Assets/VivifyTemplate/Examples",
                "Assets/VivifyTemplate/CGIncludes",
            };
            ExportPackage(assetPaths, "VivifyTemplate-Examples");
        }

        private static void ExportCgIncludes()
        {
            string[] assetPaths = {
                "Assets/VivifyTemplate/CGIncludes",
            };
            ExportPackage(assetPaths, "VivifyTemplate-CGIncludes");
        }
    }
}
