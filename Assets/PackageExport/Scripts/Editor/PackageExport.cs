using System.IO;
using UnityEditor;
using UnityEngine;

namespace PackageExport.Scripts.Editor
{
    public static class PackageExport
    {
        private const string OUTPUT_PATH = "Assets/PackageExport/Output";
        private const string EXPORTER_PATH = "Packages/com.swifter.vivify-template.exporter";
        private const string EXAMPLES_PATH = "Packages/com.swifter.vivify-template.examples";
        private const string UTILITIES_PATH = "Packages/com.swifter.vivify-template.utilities";

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
            Directory.CreateDirectory(OUTPUT_PATH);
            ExportAll();
            ExportExporter();
            ExportExamples();
            ExportUtilities();
            OpenFolderInProject(OUTPUT_PATH);
        }

        private static void ExportAll()
        {
            string[] assetPaths = {
                EXPORTER_PATH,
                EXAMPLES_PATH,
                UTILITIES_PATH,
            };
            ExportPackage(assetPaths, "VivifyTemplate-All");
        }

        private static void ExportExporter()
        {
            string[] assetPaths = {
                EXPORTER_PATH,
            };
            ExportPackage(assetPaths, "VivifyTemplate-Exporter");
        }

        private static void ExportExamples()
        {
            string[] assetPaths = {
                EXAMPLES_PATH,
                UTILITIES_PATH,
            };
            ExportPackage(assetPaths, "VivifyTemplate-Examples");
        }

        private static void ExportUtilities()
        {
            string[] assetPaths = {
                UTILITIES_PATH,
            };
            ExportPackage(assetPaths, "VivifyTemplate-Utilities");
        }
    }
}
