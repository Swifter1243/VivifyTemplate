using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Structures;

namespace VivifyTemplate.Exporter.Scripts
{
	public static class BundleInfoProcessor
	{
		public const string BUNDLE_INFO_FILENAME = "bundleinfo.json";

		public static void Serialize(
			string outputPath,
			bool prettify,
			BundleInfo bundleInfo,
			Logger logger
		)
		{
			AssetBundle bundle = AssetBundle.LoadFromFile(bundleInfo.bundleFiles[0]);
			string[] names = bundle.GetAllAssetNames();

			IEnumerable<string> materialNames = names.Where(x => x.Contains(".mat"));
			IEnumerable<string> prefabNames = names.Where(x => x.Contains(".prefab"));

			foreach (var name in materialNames)
			{
				SerializeMaterial(bundleInfo, bundle, name);
			}

			foreach (string name in prefabNames)
			{
				SerializePrefab(bundleInfo, name);
			}

			Formatting formatting = prettify ? Formatting.Indented : Formatting.None;
			string json = JsonConvert.SerializeObject(bundleInfo, formatting);
			string assetInfoPath = Path.Combine(outputPath, BUNDLE_INFO_FILENAME);
			File.WriteAllText(assetInfoPath, json);
			logger.Log($"Successfully wrote {BUNDLE_INFO_FILENAME} for bundle '{ProjectBundle.Value}' to '{assetInfoPath}'");
		}

		private static void SerializePrefab(BundleInfo bundleInfo, string name)
		{
			string filename = Path.GetFileNameWithoutExtension(name);
			string key = filename;
			int variation = 0;
			while (bundleInfo.prefabs.ContainsKey(key))
			{
				key = $"{filename} ({++variation})";
			}
			bundleInfo.prefabs.Add(key, name);
		}

		private static void SerializeMaterial(BundleInfo bundleInfo, AssetBundle bundle, string name)
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
				SerializeMaterialProperty(materialInfo, propertyName, propertyType, material);
			}

			string filename = Path.GetFileNameWithoutExtension(name);
			string key = filename;
			int variation = 0;
			while (bundleInfo.materials.ContainsKey(key))
			{
				key = $"{filename} ({++variation})";
			}
			bundleInfo.materials.Add(key, materialInfo);
		}

		private static void SerializeMaterialProperty(MaterialInfo materialInfo, string propertyName,
			ShaderUtil.ShaderPropertyType propertyType, Material material)
		{
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
					Color val = material.GetColor(propertyName);
					AddProperty("Color", $"[{val.r}, {val.g}, {val.b}, {val.a}]");
				}
					break;
				case ShaderUtil.ShaderPropertyType.Float:
				{
					float val = material.GetFloat(propertyName);
					AddProperty("Float", $"{val}");
				}
					break;
				case ShaderUtil.ShaderPropertyType.Range:
				{
					float val = material.GetFloat(propertyName);
					AddProperty("Float", $"{val}");
				}
					break;
				case ShaderUtil.ShaderPropertyType.Vector:
				{
					Vector4 val = material.GetVector(propertyName);
					AddProperty("Vector", $"[{val.x}, {val.y}, {val.z}, {val.w}]");
				}
					break;
				case ShaderUtil.ShaderPropertyType.TexEnv:
					AddProperty("Texture", "");
					break;
			}
		}
	}
}
