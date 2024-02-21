using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

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

	static string GetCachePath() => Application.temporaryCachePath;

	static bool Build(
		string outputDirectory,
		BuildAssetBundleOptions buildOptions,
		BuildVersion version
	)
	{
		// Ensure rebuild
		buildOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

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
		var temp = GetCachePath();
		var manifest = BuildPipeline.BuildAssetBundles(temp,
		buildOptions, EditorUserBuildSettings.activeBuildTarget);

		if (!manifest) return false;

		// Fix new shader keywords
		if (version == BuildVersion._2021)
		{
			var bundlePath = temp + "/bundle";
			var processPath = Path.Combine(
				Application.dataPath, 
				"VivifyTemplate/Scripts/net6.0/ShaderKeywordRewriter.exe"
			);
			var processInfo = new ProcessStartInfo(processPath, bundlePath);
			Process.Start(processInfo).WaitForExit();
			var fixedBundle = bundlePath + "_fixed";
			if (File.Exists(fixedBundle))
				File.Copy(bundlePath + "_fixed", bundlePath, true);
		}

		// Move into project
		var fileName = version == BuildVersion._2019 ? "bundle_2019" : "bundle_2021";
		var bundleOutput = outputDirectory + "/" + fileName;
		var manifestOutput = outputDirectory + "/" + fileName + ".manifest";

		File.Copy(temp + "/bundle", bundleOutput, true);
		File.Copy(temp + "/bundle.manifest", manifestOutput, true);

		return true;
	}

	[MenuItem("Assets/Vivify/Quick Build Asset Bundles _F5")]
	static void QuickBuild()
	{
		// Get Directory
		string outputDirectory = GetOutputDirectory();
		if (outputDirectory == "") return;

		// Build Asset Bundle
		Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, workingVersion);

		// Build Asset JSON For Scripting
		GenerateAssetJson.Run(Path.Combine(GetCachePath(), "bundle"), outputDirectory);
	}

	[MenuItem("Assets/Vivify/Build Asset Bundles")]
	static void FinalBuild()
	{
		// Get Directory
		string outputDirectory = GetOutputDirectory();
		if (outputDirectory == "") return;

		// Build Asset Bundle
		Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion._2021);
		Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion._2019);

		// Build Asset JSON For Scripting
		GenerateAssetJson.Run(Path.Combine(GetCachePath(), "bundle"), outputDirectory);
	}

	static string GetOutputDirectory()
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