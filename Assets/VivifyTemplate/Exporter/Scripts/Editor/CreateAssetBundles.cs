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

namespace VivifyTemplate.Exporter.Scripts.Editor
{
	public static class CreateAssetBundles
	{
		private enum BuildVersion
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

		private static BuildVersion WorkingVersion
		{
			get
			{
				string pref = PlayerPrefs.GetString("workingVersion", null);

				if (!Enum.TryParse(pref, out BuildVersion ver))
				{
					var defaultVersion = BuildVersion.Windows2019;
					PlayerPrefs.SetString("workingVersion", defaultVersion.ToString());
					return defaultVersion;
				}

				return ver;
			}
			set => PlayerPrefs.SetString("workingVersion", value.ToString());
		}

		private static bool ExportAssetInfo
		{
			get => PlayerPrefs.GetInt("exportAssetInfo", 1) == 1;
			set => PlayerPrefs.SetInt("exportAssetInfo", value ? 1 : 0);
		}

		private static bool BuildAndroidVersions
		{
			get => PlayerPrefs.GetInt("buildAndroidVersions", 1) == 1;
			set => PlayerPrefs.SetInt("buildAndroidVersions", value ? 1 : 0);
		}

		// Set Working Version
		[MenuItem("Vivify/Set Working Version/2019")]
		private static void SetWorkingVersion_2019()
		{
			WorkingVersion = BuildVersion.Windows2019;
		}
		[MenuItem("Vivify/Set Working Version/2019", true)]
		private static bool ValidateWorkingVersion_2019() { return WorkingVersion != BuildVersion.Windows2019; }

		[MenuItem("Vivify/Set Working Version/2021")]
		private static void SetWorkingVersion_2021()
		{
			WorkingVersion = BuildVersion.Windows2021;
		}
		[MenuItem("Vivify/Set Working Version/2021", true)]
		private static bool ValidateWorkingVersion_2021() { return WorkingVersion != BuildVersion.Windows2021; }

		// Export Asset Info
		[MenuItem("Vivify/Export Asset Info/True")]
		private static void ExportAssetInfo_True()
		{
			ExportAssetInfo = true;
		}
		[MenuItem("Vivify/Export Asset Info/True", true)]
		private static bool ValidateExportAssetInfo_True() { return !ExportAssetInfo; }

		[MenuItem("Vivify/Export Asset Info/False")]
		private static void ExportAssetInfo_False()
		{
			ExportAssetInfo = false;
		}
		[MenuItem("Vivify/Export Asset Info/False", true)]
		private static bool ValidateExportAssetInfo_False() { return ExportAssetInfo; }

		// Build Android Versions
		[MenuItem("Vivify/Build/Build Android Versions/True")]
		private static void BuildAndroidVersions_True()
		{
			BuildAndroidVersions = true;
		}
		[MenuItem("Vivify/Build/Build Android Versions/True", true)]
		private static bool ValidateBuildAndroidVersions_True() { return !BuildAndroidVersions; }

		[MenuItem("Vivify/Build/Build Android Versions/False")]
		private static void BuildAndroidVersions_False()
		{
			BuildAndroidVersions = false;
		}
		[MenuItem("Vivify/Build/Build Android Versions/False", true)]
		private static bool ValidateBuildAndroidVersions_False() { return BuildAndroidVersions; }

		private static string GetCachePath()
		{
			string path = Path.Combine(Application.temporaryCachePath, "bundleBuilds");
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			return path;
		}

		private static bool IsNewXRPluginInstalled()
		{
			// Check if the XR Management namespace exists
			Type xrManagementType = Type.GetType("UnityEngine.XR.Management.XRGeneralSettings, Unity.XR.Management");
			if (xrManagementType != null)
			{
				return true;
			}

			// Alternatively, check for another specific class from the new XR plugins
			Type xrManagerSettingsType = Type.GetType("UnityEngine.XR.Management.XRManagerSettings, Unity.XR.Management");
			return xrManagerSettingsType != null;
		}

		private struct BuildReport
		{
			public string tempBundlePath;
			public string fixedBundlePath;
			public string bundleOutput;
			public string manifestOutput;
			public bool shaderKeywordsFixed;
			public bool isAndroid;
			public BuildTarget buildTarget;
		}

		private static BuildReport Build(
			string outputDirectory,
			BuildAssetBundleOptions buildOptions,
			BuildVersion version
		)
		{
			// Check output directory exists
			if (!Directory.Exists(outputDirectory))
			{
				throw new DirectoryNotFoundException($"The directory '{outputDirectory}' doesn't exist.");
			}

			var projectBundle = BundleName.ProjectBundle;

			// Check bundle isn't empty
			var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(projectBundle);
			if (assetPaths.Length == 0)
			{
				throw new Exception($"The bundle '{projectBundle}' is empty.");
			}

			// Check correct packages are being used for XR
			var isAndroid = version == BuildVersion.Android2019 || version == BuildVersion.Android2021;
			var is2019 = version == BuildVersion.Windows2019 || version == BuildVersion.Android2019;

			if (is2019 && IsNewXRPluginInstalled()) {
				var name = Enum.GetName(typeof(BuildVersion), version);
				throw new Exception($"Version '{name}' requires Single Pass which doesn't exist on the new XR packages. Please go to Window > Package Manager and remove them.");
			}

			// TODO: Report no android SDK?

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

			// Empty build location directory
			var tempDir = GetCachePath();
			Directory.Delete(tempDir, true);
			Directory.CreateDirectory(tempDir);

			// Build
			var tempBundlePath = tempDir + "/" + projectBundle;
			var manifestPath = tempBundlePath + ".manifest"; // new .manifest isn't built from ShaderKeywordRewriter
			var builtBundlePath = tempBundlePath;
			var fixedBundlePath = "";
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
				throw new Exception("The build was unsuccessful. Check above for possible errors reported by the build pipeline.");
			}

			// Fix new shader keywords
			var shaderKeywordsFixed = !is2019;
			if (shaderKeywordsFixed)
			{
				// Run Process
				var processPath = Path.Combine(
					Application.dataPath,
					"VivifyTemplate/Exporter/Scripts/ShaderKeywordRewriter/ShaderKeywordRewriter.exe"
				);
				var processInfo = new ProcessStartInfo(processPath, tempBundlePath);
				Process.Start(processInfo).WaitForExit();

				// Check if fixed bundle generated
				if (File.Exists(fixedBundlePath))
				{
					builtBundlePath = fixedBundlePath;
				}
				else
				{
					shaderKeywordsFixed = false;
				}
			}

			// Move into project
			var fileName = GetBundleFileName(version);
			var bundleOutput = outputDirectory + "/" + fileName;
			var manifestOutput = outputDirectory + "/" + fileName + ".manifest";

			File.Copy(builtBundlePath, bundleOutput, true);
			File.Copy(manifestPath, manifestOutput, true);
			Debug.Log($"Successfully built bundle '{projectBundle}' to '{bundleOutput}'.");

			return new BuildReport
			{
				tempBundlePath = tempBundlePath,
				fixedBundlePath = fixedBundlePath,
				bundleOutput = bundleOutput,
				manifestOutput = manifestOutput,
				shaderKeywordsFixed = shaderKeywordsFixed,
				isAndroid = isAndroid,
				buildTarget = buildTarget
			};
		}

		[MenuItem("Vivify/Build/Build Working Version Quick _F5")]
		private static void QuickBuild()
		{
			// Get Directory
			string outputDirectory = GetOutputDirectory();
			if (outputDirectory == "") return; // window was exited

			// Build Asset Bundle
			var build = Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, WorkingVersion);

			// Build Asset JSON For Scripting
			if (ExportAssetInfo)
			{
				GenerateAssetJson.Run(build.tempBundlePath, outputDirectory);
			}
		}

		[MenuItem("Vivify/Build/Build All Versions Compressed")]
		private static void FinalBuild()
		{
			// Get Directory
			string outputDirectory = GetOutputDirectory();
			if (outputDirectory == "") return; // window was exited

			// Build Asset Bundle
			var build = Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2019);
			Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2021);

			if (ExportAssetInfo)
			{
				GenerateAssetJson.Run(build.tempBundlePath, outputDirectory);
			}

			if (BuildAndroidVersions)
			{
				Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2019);
				Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2021);
			}

			Debug.Log("All builds done!");
		}

		private static string GetOutputDirectory()
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
		private static void ClearAssetBundleLocation()
		{
			PlayerPrefs.DeleteKey("bundleDir");
		}
	}
}