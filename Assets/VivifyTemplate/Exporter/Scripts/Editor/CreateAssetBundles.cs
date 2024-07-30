using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
	public static class CreateAssetBundles
	{
		private static BuildVersion WorkingVersion
		{
			get
			{
				string pref = PlayerPrefs.GetString("workingVersion", null);

				if (!Enum.TryParse(pref, out BuildVersion ver))
				{
					BuildVersion defaultVersion = BuildVersion.Windows2019;
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
		[MenuItem("Vivify/Settings/Set Working Version/2019")]
		private static void SetWorkingVersion_2019()
		{
			WorkingVersion = BuildVersion.Windows2019;
		}
		[MenuItem("Vivify/Settings/Set Working Version/2019", true)]
		private static bool ValidateWorkingVersion_2019() { return WorkingVersion != BuildVersion.Windows2019; }

		[MenuItem("Vivify/Settings/Set Working Version/2021")]
		private static void SetWorkingVersion_2021()
		{
			WorkingVersion = BuildVersion.Windows2021;
		}
		[MenuItem("Vivify/Settings/Set Working Version/2021", true)]
		private static bool ValidateWorkingVersion_2021() { return WorkingVersion != BuildVersion.Windows2021; }

		// Export Asset Info
		[MenuItem("Vivify/Settings/Export Asset Info/True")]
		private static void ExportAssetInfo_True()
		{
			ExportAssetInfo = true;
		}
		[MenuItem("Vivify/Settings/Export Asset Info/True", true)]
		private static bool ValidateExportAssetInfo_True() { return !ExportAssetInfo; }

		[MenuItem("Vivify/Settings/Export Asset Info/False")]
		private static void ExportAssetInfo_False()
		{
			ExportAssetInfo = false;
		}
		[MenuItem("Vivify/Settings/Export Asset Info/False", true)]
		private static bool ValidateExportAssetInfo_False() { return ExportAssetInfo; }

		// Build Android Versions
		[MenuItem("Vivify/Settings/Build Android Versions/True")]
		private static void BuildAndroidVersions_True()
		{
			BuildAndroidVersions = true;
		}
		[MenuItem("Vivify/Settings/Build Android Versions/True", true)]
		private static bool ValidateBuildAndroidVersions_True() { return !BuildAndroidVersions; }

		[MenuItem("Vivify/Settings/Build Android Versions/False")]
		private static void BuildAndroidVersions_False()
		{
			BuildAndroidVersions = false;
		}
		[MenuItem("Vivify/Settings/Build Android Versions/False", true)]
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
			public string outputBundlePath;
			public bool shaderKeywordsFixed;
			public uint? crc;
			public bool isAndroid;
			public BuildTarget buildTarget;
			public BuildVersion buildVersion;
		}

		private static bool FixShaderKeywords(string bundlePath, string expectedOutput)
		{
			// Run Process
			string processPath = Path.Combine(
				Application.dataPath,
				@"VivifyTemplate\Exporter\Dependencies\ShaderKeywordRewriter\ShaderKeywordRewriter.exe"
			);
			ProcessStartInfo processInfo = new ProcessStartInfo(processPath,  $"\"{bundlePath}\"")
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false, // Required for redirection
				CreateNoWindow = true
			};

			Process process = Process.Start(processInfo);
			if (process == null)
			{
				throw new InvalidOperationException("Shader Keyword Rewriter program was null.");
			}
			
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();
					
			process.WaitForExit();
				
			Debug.Log($"ShaderKeywordRewriter: {output}");
			if (!string.IsNullOrEmpty(error))
			{
				throw new Exception($"Error from ShaderKeywordsRewriter: {error}");
			}
			
			// Check if fixed bundle generated
			return File.Exists(expectedOutput);
		}

		private static BuildReport Build(
			string outputDirectory,
			BuildAssetBundleOptions buildOptions,
			BuildVersion buildVersion
		)
		{
			// Check output directory exists
			if (!Directory.Exists(outputDirectory))
			{
				throw new DirectoryNotFoundException($"The directory '{outputDirectory}' doesn't exist.");
			}

			string projectBundleName = BundleName.ProjectBundle;

			// Check bundle isn't empty
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(projectBundleName);
			if (assetPaths.Length == 0)
			{
				throw new Exception($"The bundle '{projectBundleName}' is empty.");
			}

			// Check correct packages are being used for XR
			bool isAndroid = buildVersion == BuildVersion.Android2019 || buildVersion == BuildVersion.Android2021;
			bool is2019 = buildVersion == BuildVersion.Windows2019 || buildVersion == BuildVersion.Android2019;

			if (is2019 && IsNewXRPluginInstalled()) {
				string name = Enum.GetName(typeof(BuildVersion), buildVersion);
				throw new Exception($"Version '{name}' requires Single Pass which doesn't exist on the new XR packages. Please go to Window > Package Manager and remove them.");
			}

			// TODO: Report no android SDK?

			// Ensure rebuild
			buildOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

			// Set Single Pass Mode
			switch (buildVersion)
			{
				case BuildVersion.Windows2019:
				case BuildVersion.Android2019:
					PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
					break;
				case BuildVersion.Windows2021:
				case BuildVersion.Android2021:
					PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(buildVersion), buildVersion, null);
			}

			// Empty build location directory
			string tempDir = GetCachePath();
			Directory.Delete(tempDir, true);
			Directory.CreateDirectory(tempDir);

			// Build
			string tempBundlePath = Path.Combine(tempDir, projectBundleName);
			string builtBundlePath = tempBundlePath;
			string fixedBundlePath = "";
			BuildTarget buildTarget = isAndroid ? BuildTarget.Android : EditorUserBuildSettings.activeBuildTarget;

			AssetBundleBuild[] builds = {
				new AssetBundleBuild
				{
					assetBundleName = projectBundleName,
					assetNames = assetPaths
				}
			};

			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(tempDir, builds, buildOptions, buildTarget);
			if (!manifest)
			{
				throw new Exception("The build was unsuccessful. Check above for possible errors reported by the build pipeline.");
			}

			// Fix new shader keywords
			bool shaderKeywordsFixed = !is2019;
			if (shaderKeywordsFixed)
			{
				string expectedOutput = Path.ChangeExtension(tempBundlePath, ".mod.avatar");
				bool success = FixShaderKeywords(tempBundlePath, expectedOutput);
				if (success)
				{
					fixedBundlePath = expectedOutput;
					builtBundlePath = expectedOutput;
				}
				else
				{
					shaderKeywordsFixed = false;
				}
			}

			// Move into project
			string fileName = VersionTools.GetBundleFileName(buildVersion);
			string outputBundlePath = outputDirectory + "/" + fileName;

			File.Copy(builtBundlePath, outputBundlePath, true);
			Debug.Log($"Successfully built bundle '{projectBundleName}' to '{outputBundlePath}'.");
			
			// Get CRC if shader keywords not fixed
			uint? crc = null;
			if (!shaderKeywordsFixed)
			{
				BuildPipeline.GetCRCForAssetBundle(builtBundlePath, out uint crcOut);
				crc = crcOut;
			}

			return new BuildReport
			{
				tempBundlePath = tempBundlePath,
				fixedBundlePath = fixedBundlePath,
				outputBundlePath = outputBundlePath,
				shaderKeywordsFixed = shaderKeywordsFixed,
				crc = crc,
				isAndroid = isAndroid,
				buildTarget = buildTarget,
				buildVersion = buildVersion
			};
		}

		private static async void BuildSingleUncompressed(BuildVersion version)
		{
			// Get Directory
			string outputDirectory = GetOutputDirectory();

			if (ExportAssetInfo)
			{
				GenerateAssetJson.AssetInfo assetInfo = new GenerateAssetJson.AssetInfo();
				BuildReport build = Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, version);
				string versionPrefix = VersionTools.GetVersionPrefix(version);
				uint crc = build.crc ?? await CRCGrabber.GetCRCFromFile(build.outputBundlePath);
				assetInfo.bundleCRCs[versionPrefix] = crc;
				GenerateAssetJson.Run(build.outputBundlePath, outputDirectory, assetInfo);
			}
			else
			{
				Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, version);
			}
			
			Debug.Log("Build done!");
		}

		[MenuItem("Vivify/Build/Build Windows 2019 Uncompressed")]
		private static void BuildWindows2019Uncompressed()
		{
			BuildSingleUncompressed(BuildVersion.Windows2019);
		}
		
		[MenuItem("Vivify/Build/Build Windows 2021 Uncompressed")]
		private static void BuildWindows2021Uncompressed()
		{
			BuildSingleUncompressed(BuildVersion.Windows2021);
		}
		
		[MenuItem("Vivify/Build/Build Android 2019 Uncompressed")]
		private static void BuildAndroid2019Uncompressed()
		{
			BuildSingleUncompressed(BuildVersion.Android2019);
		}
		
		[MenuItem("Vivify/Build/Build Android 2021 Uncompressed")]
		private static void BuildAndroid2021Uncompressed()
		{
			BuildSingleUncompressed(BuildVersion.Android2021);
		}

		[MenuItem("Vivify/Build/Build Working Version Uncompressed _F5")]
		private static void BuildWorkingVersionUncompressed()
		{
			BuildSingleUncompressed(WorkingVersion);
		}

		[MenuItem("Vivify/Build/Build All Versions Compressed")]
		private static async void BuildAllCompressed()
		{
			// Get Directory
			string outputDirectory = GetOutputDirectory();

			List<BuildReport> builds = new List<BuildReport>
			{
				// Build Asset Bundle
				Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2019),
				Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2021)
			};

			if (BuildAndroidVersions)
			{
				try
				{
					builds.Add(Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2019));
					builds.Add(Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2021));
				}
				catch (Exception e)
				{
					Debug.LogError($"Error trying to build for Android: {e}");
				}
			}

			if (ExportAssetInfo)
			{
				GenerateAssetJson.AssetInfo assetInfo = new GenerateAssetJson.AssetInfo();
				
				IEnumerable<Task> tasks = builds.Select(async build =>
				{
					uint crc = build.crc ?? await CRCGrabber.GetCRCFromFile(build.outputBundlePath);
					string versionPrefix = VersionTools.GetVersionPrefix(build.buildVersion);
					assetInfo.bundleCRCs[versionPrefix] = crc;
				});
				await Task.WhenAll(tasks);
				
				GenerateAssetJson.Run(builds[0].outputBundlePath, outputDirectory, assetInfo);
			}

			Debug.Log("All builds done!");
		}

		private static string GetOutputDirectory()
		{
			if (PlayerPrefs.HasKey("bundleDir"))
			{
				return PlayerPrefs.GetString("bundleDir");
			}
			
			string outputDirectory = EditorUtility.OpenFolderPanel("Select Directory", "", "");
			if (outputDirectory == "")
			{
				throw new Exception("User closed the directory window.");
			}
			PlayerPrefs.SetString("bundleDir", outputDirectory);
			return outputDirectory;
		}

		[MenuItem("Vivify/Clear Asset Bundle Location")]
		private static void ClearAssetBundleLocation()
		{
			PlayerPrefs.DeleteKey("bundleDir");
		}
	}
}