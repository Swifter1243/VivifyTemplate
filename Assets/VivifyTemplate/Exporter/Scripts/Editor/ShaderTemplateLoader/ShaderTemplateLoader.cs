using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.ShaderTemplateLoader
{
    public class ShaderTemplateLoader
    {
        private static readonly string TemplateDirectory = "Assets/VivifyTemplate/Examples/Shaders/Shader Template";
    
        [MenuItem("Assets/Create/Shader/Vivify/Standard", false, 69)]
        private static void CreateStandardShader()
        {
            CreateShader("Standard");
        }
    
        [MenuItem("Assets/Create/Shader/Vivify/Blit", false, 69)]
        private static void CreateBlitShader()
        {
            CreateShader("Blit");
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
                Path.Combine(AssetDatabase.GetAssetPath(Selection.activeObject), $"{shaderName}.shader"),
                EditorGUIUtility.IconContent("Shader Icon").image as Texture2D,
                File.ReadAllText(templatePath)
            );
        }

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