using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;
using Newtonsoft.Json;

public class GenerateAssetJson
{
	public static void Run(string bundlePath, string outputPath)
	{
		var bundle = AssetBundle.LoadFromFile(bundlePath);
		var names = bundle.GetAllAssetNames();

		var materialNames = names.Where(x => x.Contains(".mat"));
		var prefabNames = names.Where(x => x.Contains(".prefab"));

		var assetInfo = new AssetInfo();

		foreach (var name in materialNames)
		{
			var material = bundle.LoadAsset<Material>(name);

            var materialInfo = new MaterialInfo
            {
                path = name
            };

            int propertyCount = ShaderUtil.GetPropertyCount(material.shader);
			for (int i = 0; i < propertyCount; i++)
			{
				string propertyName = ShaderUtil.GetPropertyName(material.shader, i);
				ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(material.shader, i);

				void AddProperty(string type, string value)
				{
					materialInfo.properties.Add(
						propertyName,
						new Dictionary<string, string>
						{
							{ type, value }
						}
					);
				}

				switch (propertyType)
				{
					case ShaderUtil.ShaderPropertyType.Color:
						{
							var val = material.GetColor(propertyName);
							AddProperty("Color", $"[{val.r}, {val.g}, {val.b}, {val.a}]");
						}
						break;
					case ShaderUtil.ShaderPropertyType.Float:
						{
							var val = material.GetFloat(propertyName);
							AddProperty("Float", $"{val}");
						}
						break;
					case ShaderUtil.ShaderPropertyType.Range:
						{
							var val = material.GetFloat(propertyName);
							AddProperty("Float", $"{val}");
						}
						break;
					case ShaderUtil.ShaderPropertyType.Vector:
						{
							var val = material.GetVector(propertyName);
							AddProperty("Vector", $"[{val.x}, {val.y}, {val.z}, {val.w}]");
						}
						break;
					case ShaderUtil.ShaderPropertyType.TexEnv:
						AddProperty("Texture", "");
						break;
				}
			}

			var filename = Path.GetFileNameWithoutExtension(name);
			assetInfo.materials.Add(filename, materialInfo);
		}

		foreach (var name in prefabNames)
		{
			var filename = Path.GetFileNameWithoutExtension(name);
			assetInfo.prefabs.Add(filename, name);
		}

		string json = JsonConvert.SerializeObject(assetInfo);
		File.WriteAllText(outputPath + "/assetinfo.json", json);
	}

	[System.Serializable]
	public class AssetInfo
	{
		public Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>();
		public Dictionary<string, string> prefabs = new Dictionary<string, string>();
	}

	[System.Serializable]
	public class MaterialInfo
	{
		public string path;
		public Dictionary<string, Dictionary<string, string>> properties = new Dictionary<string, Dictionary<string, string>> { };
	}
}