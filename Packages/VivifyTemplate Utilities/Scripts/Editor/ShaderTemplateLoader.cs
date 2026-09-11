using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace VivifyTemplate.Utilities.Scripts.Editor
{
    public static class ShaderTemplateLoader
    {
        private readonly static string TemplateDirectory = "Assets/VivifyTemplate/Utilities/Shaders/Templates";

        [MenuItem("Assets/Create/Shader/Vivify/Standard", false, 69)]
        private static void CreateStandardShader()
        {
            CreateShader("Standard");
        }

        [MenuItem("Assets/Create/Shader/Vivify/GrabPass", false, 69)]
        private static void CreateGrabPassShader()
        {
            CreateShader("GrabPass");
        }

        [MenuItem("Assets/Create/Shader/Vivify/Blit", false, 69)]
        private static void CreateBlitShader()
        {
            CreateShader("Blit");
        }

        [MenuItem("Assets/Create/Shader/Vivify/BareBones", false, 69)]
        private static void CreateBareBonesShader()
        {
            CreateShader("BareBones");
        }

        private static void CreateShader(string shaderName)
        {
            string templatePath = Path.Combine(TemplateDirectory, shaderName + ".shader");

            if (!File.Exists(templatePath))
            {
                Debug.LogError($"Unable to locate template shader at '{templatePath}'. Please report this.");
                return;
            }

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateShader>(),
                Path.Combine(GetCurrentDir(), $"{shaderName}.shader"),
                EditorGUIUtility.IconContent("Shader Icon").image as Texture2D,
                File.ReadAllText(templatePath)
            );
        }

        private static string GetCurrentDir()
        {
            string path = "Assets";
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(assetPath) && AssetDatabase.IsValidFolder(assetPath))
                    path = assetPath;
                else
                    path = Path.GetDirectoryName(assetPath);
            }
            return path; }

        private class DoCreateShader : EndNameEditAction
        {
            public override void Action(int instanceId, string path, string text)
            {
                // Replace shader path and rewrite
                string pattern = "Shader\\s+\"[^\"]+\"";
                text = Regex.Replace(
                    text,
                    pattern,
                    $"Shader \"Custom/{Path.GetFileNameWithoutExtension(path)}\""
                );
                File.WriteAllText(path, text);
                AssetDatabase.Refresh();
            }
        }
    }
}
