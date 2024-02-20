using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
	[MenuItem("Assets/Vivify/Quick Build Asset Bundles _F5")]
	static void QuickBuild()
	{
		// Get Directory
		string assetBundleDirectory = GetDirectory();
		if (assetBundleDirectory == "") return;

		// Ensure Directory Exists
		if (!Directory.Exists(Application.streamingAssetsPath))
			Directory.CreateDirectory(assetBundleDirectory);

		// Build Asset JSON For Scripting
		//GenerateAssetJson.Run(assetBundleDirectory);

		// Build Asset Bundle
		BuildPipeline.BuildAssetBundles(assetBundleDirectory,
		BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
	}

	[MenuItem("Assets/Vivify/Build Asset Bundles")]
	static void FinalBuild()
	{
		// Get Directory
		string assetBundleDirectory = GetDirectory();
		if (assetBundleDirectory == "") return;

		// Ensure Directory Exists
		if (!Directory.Exists(Application.streamingAssetsPath))
			Directory.CreateDirectory(assetBundleDirectory);

		// Build Asset JSON For Scripting
		//GenerateAssetJson.Run(assetBundleDirectory);

		// Build Asset Bundle
		BuildPipeline.BuildAssetBundles(assetBundleDirectory,
		BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
	}

	static string GetDirectory()
	{
		if (
			!PlayerPrefs.HasKey("bundleDir") ||
			PlayerPrefs.GetString("bundleDir") == ""
		)
		{
			var assetBundleDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
			PlayerPrefs.SetString("bundleDir", assetBundleDirectory);
			return assetBundleDirectory;
		}

		return PlayerPrefs.GetString("bundleDir");
	}

	[MenuItem("Assets/Vivify/Clear Asset Bundle Location")]
	static void ClearAssetBundleLocation()
	{
		PlayerPrefs.DeleteKey("bundleDir");
	}
}