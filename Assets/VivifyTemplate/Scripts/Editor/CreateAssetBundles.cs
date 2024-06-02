using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEditor.Build.Content;

public class CreateAssetBundles
{
	enum BuildVersion
	{
		Windows2019,
		Windows2021,
		Android2019,
		Android2021
	}

	private static string GetBundleFileName(BuildVersion version)
	{
		switch (version)
		{
			case BuildVersion.Windows2019: return "bundle_windows2019";
			case BuildVersion.Windows2021: return "bundle_windows2021";
			case BuildVersion.Android2019: return "bundle_android2019";
			case BuildVersion.Android2021: return "bundle_android2021";
		}

		return "";
	}

	static BuildVersion workingVersion
	{
		get
		{
			string pref = PlayerPrefs.GetString("workingVer", null);

			if (!Enum.TryParse(pref, out BuildVersion ver))
			{
				var defaultVersion = BuildVersion.Windows2019;
				PlayerPrefs.SetString("workingVer", defaultVersion.ToString());
				return defaultVersion;
			}

			return ver;
		}
		set => PlayerPrefs.SetString("workingVer", value.ToString());
	}

	static bool exportAssetInfo
	{
		get => PlayerPrefs.GetInt("exportAssetInfo", 1) == 1;
		set => PlayerPrefs.SetInt("exportAssetInfo", value ? 1 : 0);
	}

	// Set Working Version
	[MenuItem("Vivify/Set Working Version/2019")]
	static void SetWorkingVersion_2019()
	{
		workingVersion = BuildVersion.Windows2019;
	}
	[MenuItem("Vivify/Set Working Version/2019", true)]
	static bool ValidateWorkingVersion_2019() { return workingVersion != BuildVersion.Windows2019; }

	[MenuItem("Vivify/Set Working Version/2021")]
	static void SetWorkingVersion_2021()
	{
		workingVersion = BuildVersion.Windows2021;
	}
	[MenuItem("Vivify/Set Working Version/2021", true)]
	static bool ValidateWorkingVersion_2021() { return workingVersion != BuildVersion.Windows2021; }

	// Export Asset Info
	[MenuItem("Vivify/Export Asset Info/True")]
	static void ExportAssetInfo_True()
	{
		exportAssetInfo = true;
	}
	[MenuItem("Vivify/Export Asset Info/True", true)]
	static bool ValidateExportAssetInfo_True() { return !exportAssetInfo; }

	[MenuItem("Vivify/Export Asset Info/False")]
	static void ExportAssetInfo_False()
	{
		exportAssetInfo = false;
	}
	[MenuItem("Vivify/Export Asset Info/False", true)]
	static bool ValidateExportAssetInfo_False() { return exportAssetInfo; }

	static string GetCachePath()
	{
		string path = Path.Combine(Application.temporaryCachePath, "bundleBuilds");
		if (!Directory.Exists(path)) Directory.CreateDirectory(path);
		return path;
	}

	static bool Build(
		string outputDirectory,
		BuildAssetBundleOptions buildOptions,
		BuildVersion version
	)
	{
		// Check output directory exists
		if (!Directory.Exists(outputDirectory))
		{
			throw new DirectoryNotFoundException($"The directory \"{outputDirectory}\" doesn't exist.");
		}

		var projectBundle = BundleName.projectBundle;

		// Check bundle exists
		var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(projectBundle);
		if (assetPaths.Length == 0)
		{
			throw new Exception($"The bundle '{projectBundle}' is empty.");
		}

		// Ensure rebuild
		buildOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

		// Set Single Pass Mode
		switch (version)
		{
			case BuildVersion.Windows2019:
			case BuildVersion.Android2019:
				PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
				break;
			case BuildVersion.Windows2021:
			case BuildVersion.Android2021:
				PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
				break;
		}

		// Build
		var tempDir = GetCachePath();

		var isAndroid = version == BuildVersion.Android2019 || version == BuildVersion.Android2021;
		var buildTarget = isAndroid ? BuildTarget.Android : EditorUserBuildSettings.activeBuildTarget;

		AssetBundleBuild[] builds = {
			new AssetBundleBuild
			{
				assetBundleName = projectBundle,
				assetNames = assetPaths
			}
		};

		AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(tempDir, builds, buildOptions, buildTarget);

		if (!manifest)
		{
			throw new Exception("The build was unsuccessful for some stupid fucking reason.");
		}

		// Fix new shader keywords
		if (PlayerSettings.stereoRenderingPath == StereoRenderingPath.Instancing)
		{
			var bundlePath = tempDir + "/" + projectBundle;
			var processPath = Path.Combine(
				Application.dataPath,
				"VivifyTemplate/Scripts/net6.0/ShaderKeywordRewriter.exe"
			);
			var processInfo = new ProcessStartInfo(processPath, bundlePath);
			Process.Start(processInfo).WaitForExit();
			var fixedBundle = bundlePath + "_fixed";
			if (File.Exists(fixedBundle))
			{
				File.Copy(fixedBundle, bundlePath, true);
				File.Delete(fixedBundle);
			}
		}

		// Move into project
		var fileName = GetBundleFileName(version);
		var bundleOutput = outputDirectory + "/" + fileName;
		var manifestOutput = outputDirectory + "/" + fileName + ".manifest";

		File.Copy(tempDir + "/" + projectBundle, bundleOutput, true);
		File.Copy(tempDir + $"/{projectBundle}.manifest", manifestOutput, true);
		Debug.Log($"Successfully built bundle '{projectBundle}' to {bundleOutput}.");

		return true;
	}

	[MenuItem("Vivify/Build/Build Working Version Quick _F5")]
	static void QuickBuild()
	{
		// Get Directory
		string outputDirectory = GetOutputDirectory();
		if (outputDirectory == "") return;

		// Build Asset Bundle
		Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, workingVersion);

		// Build Asset JSON For Scripting
		GenerateAssetJson.Run(Path.Combine(GetCachePath(), BundleName.projectBundle), outputDirectory);
	}

	[MenuItem("Vivify/Build/Build All Versions Compressed")]
	static void FinalBuild()
	{
		// Get Directory
		string outputDirectory = GetOutputDirectory();
		if (outputDirectory == "") return;

		// Build Asset Bundle
		Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2019);
		if (exportAssetInfo) GenerateAssetJson.Run(Path.Combine(GetCachePath(), BundleName.projectBundle), outputDirectory);
		Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2021);

		Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2019);
		Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2021);

		Debug.Log("All builds done!");
	}

	static string GetOutputDirectory()
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

	[MenuItem("Vivify/Clear Asset Bundle Location")]
	static void ClearAssetBundleLocation()
	{
		PlayerPrefs.DeleteKey("bundleDir");
	}
}