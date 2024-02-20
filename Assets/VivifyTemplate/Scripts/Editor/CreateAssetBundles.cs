using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;

public class CreateAssetBundles
{
	enum BuildVersion
	{
		_2019,
		_2021
	}

	static BuildVersion workingVersion
	{
		get
		{
			string pref = EditorPrefs.GetString("workingVer", null);

			if (!Enum.TryParse(pref, out BuildVersion ver))
			{
				var defaultVersion = BuildVersion._2019;
				EditorPrefs.SetString("workingVer", defaultVersion.ToString());
				return defaultVersion;
			}

			return ver;
		}
		set => EditorPrefs.SetString("workingVer", value.ToString());
	}

	[MenuItem("Assets/Vivify/Set Working Version/2019")]
	static void SetWorkingVersion_2019()
	{
		workingVersion = BuildVersion._2019;
	}
	[MenuItem("Assets/Vivify/Set Working Version/2019", true)]
	static bool ValidateWorkingVersion_209()
	{
		return workingVersion != BuildVersion._2019;
	}

	[MenuItem("Assets/Vivify/Set Working Version/2021")]
	static void SetWorkingVersion_2021()
	{
		workingVersion = BuildVersion._2021;
	}
	[MenuItem("Assets/Vivify/Set Working Version/2021", true)]
	static bool ValidateWorkingVersion_2021()
	{
		return workingVersion != BuildVersion._2021;
	}

	static bool Build(
		string directory,
		BuildAssetBundleOptions buildOptions,
		BuildVersion version
	)
	{
		// Set Single Pass Mode
		switch (version)
		{
			case BuildVersion._2019:
				PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
				break;
			case BuildVersion._2021:
				PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
				break;
		}

		// Build
		var temp = Application.temporaryCachePath;
		var manifest = BuildPipeline.BuildAssetBundles(temp,
		buildOptions, EditorUserBuildSettings.activeBuildTarget);

		if (!manifest) return false;

		// Rename 2021 bundle
		if (version == BuildVersion._2021)
		{
			string source = temp + "/bundle";
			string destination = temp + "/bundle_2021";
			File.Copy(source, destination, true);
		}

		// Move into project
		foreach (string file in Directory.GetFiles(temp, "*."))
		{
			var fileDestination = directory + "/" + Path.GetFileName(file);
			File.Copy(file, fileDestination, true);
		}

		return true;
	}

	[MenuItem("Assets/Vivify/Quick Build Asset Bundles _F5")]
	static void QuickBuild()
	{
		// Get Directory
		string assetBundleDirectory = GetDirectory();
		if (assetBundleDirectory == "") return;

		// Build Asset JSON For Scripting
		//GenerateAssetJson.Run(assetBundleDirectory);

		// Build Asset Bundle
		Build(assetBundleDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, workingVersion);
	}

	[MenuItem("Assets/Vivify/Build Asset Bundles")]
	static void FinalBuild()
	{
		// Get Directory
		string assetBundleDirectory = GetDirectory();
		if (assetBundleDirectory == "") return;

		// Build Asset JSON For Scripting
		//GenerateAssetJson.Run(assetBundleDirectory);

		// Build Asset Bundle
		Build(assetBundleDirectory, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildVersion._2021);
		Build(assetBundleDirectory, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildVersion._2019);
	}

	static string GetDirectory()
	{
		if (
			!EditorPrefs.HasKey("bundleDir") ||
			EditorPrefs.GetString("bundleDir") == ""
		)
		{
			var assetBundleDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
			EditorPrefs.SetString("bundleDir", assetBundleDirectory);
			return assetBundleDirectory;
		}

		return EditorPrefs.GetString("bundleDir");
	}

	[MenuItem("Assets/Vivify/Clear Asset Bundle Location")]
	static void ClearAssetBundleLocation()
	{
		EditorPrefs.DeleteKey("bundleDir");
	}
}