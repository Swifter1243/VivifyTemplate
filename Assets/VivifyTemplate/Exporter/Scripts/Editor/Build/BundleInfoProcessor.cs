using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
namespace VivifyTemplate.Exporter.Scripts.Editor.Build
{
	public static class BundleInfoProcessor
	{
		private const string BUNDLE_INFO_FILENAME = "bundleinfo.json";

		public static void Serialize(
			string bundleName,
			string outputPath,
			bool prettify,
			BundleInfo bundleInfo,
			Logger logger
		)
		{
			string[] files = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);

			IEnumerable<string> materialFiles = files.Where(x => x.Contains(".mat"));
			IEnumerable<string> prefabFiles = files.Where(x => x.Contains(".prefab"));

			foreach (var name in materialFiles)
			{
				SerializeMaterial(bundleInfo, name);
			}

			foreach (string name in prefabFiles)
			{
				SerializePrefab(bundleInfo, name);
			}

			Formatting formatting = prettify ? Formatting.Indented : Formatting.None;
			string json = JsonConvert.SerializeObject(bundleInfo, formatting);
			string assetInfoPath = Path.Combine(outputPath, BUNDLE_INFO_FILENAME);
			File.WriteAllText(assetInfoPath, json);
			logger.Log($"Successfully wrote {BUNDLE_INFO_FILENAME} for bundle '{ProjectBundle.Value}' to '{assetInfoPath}'");
		}

		private static void SerializePrefab(BundleInfo bundleInfo, string prefabPath)
		{
			prefabPath = prefabPath.ToLower();
			string filename = Path.GetFileNameWithoutExtension(prefabPath);
			string key = filename;
			int variation = 0;
			while (bundleInfo.prefabs.ContainsKey(key))
			{
				key = $"{filename} ({++variation})";
			}
			bundleInfo.prefabs.Add(key, prefabPath);
		}

		private static void SerializeMaterial(BundleInfo bundleInfo, string materialPath)
		{
			materialPath = materialPath.ToLower();
			var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

			var materialInfo = new MaterialInfo
			{
				path = materialPath
			};

			int propertyCount = ShaderUtil.GetPropertyCount(material.shader);
			for (int i = 0; i < propertyCount; i++)
			{
				string propertyName = ShaderUtil.GetPropertyName(material.shader, i);
				ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(material.shader, i);
				SerializeMaterialProperty(materialInfo, propertyName, propertyType, material);
			}

			string filename = Path.GetFileNameWithoutExtension(materialPath);
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
			switch (propertyType)
			{
			case ShaderUtil.ShaderPropertyType.Color:
			{
				Color val = material.GetColor(propertyName);
				AddProperty("Color", new[] { val.r, val.g, val.b, val.a });
				break;
			}
			case ShaderUtil.ShaderPropertyType.Float:
			case ShaderUtil.ShaderPropertyType.Range:
			{
				float val = material.GetFloat(propertyName);
				AddProperty("Float", val);
				break;
			}
			case ShaderUtil.ShaderPropertyType.Vector:
			{
				Vector4 val = material.GetVector(propertyName);
				AddProperty("Vector", new[] { val.x, val.y, val.z, val.w });
				break;
			}
			case ShaderUtil.ShaderPropertyType.TexEnv:
				AddProperty("Texture", "");
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
			}
			return;

			void AddProperty(string type, object value)
			{
				materialInfo.properties.Add(
					propertyName,
					new PropertyValue(type, value)
				);
			}
		}
	}
}
