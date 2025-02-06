using System.IO;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.ShaderTemplateLoader
{
    public class ShaderTemplateLoader
    {
        // TODO: Change shader path to filename before writing.
    
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
        
            ProjectWindowUtil.CreateAssetWithContent(
                shaderName + ".shader",
                File.ReadAllText(templatePath),
                EditorGUIUtility.IconContent("Shader Icon").image as Texture2D
                );
        }
    }
}