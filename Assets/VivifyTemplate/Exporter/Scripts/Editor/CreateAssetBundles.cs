using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Structures;
using Debug = UnityEngine.Debug;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
	public static class CreateAssetBundles
	{
		private static readonly SimpleTimer Timer = new SimpleTimer();

		[MenuItem("Vivify/Build/Build Working Version Uncompressed _F5")]
		private static void BuildWorkingVersionUncompressed()
		{
			BuildSingleUncompressed(WorkingVersion.Value);
		}

		[MenuItem("Vivify/Build/Build All Versions Compressed")]
		private static void BuildAllVersionsCompressed()
		{
			IEnumerable<BuildVersion> versions = Enum.GetValues(typeof(BuildVersion)).OfType<BuildVersion>();
			BuildAll(new List<BuildVersion>(versions), BuildAssetBundleOptions.None);
		}

		[MenuItem("Vivify/Build/Build Windows Versions Compressed")]
		private static void BuildWindowsVersionsCompressed()
		{
			BuildAll(new List<BuildVersion>
			{
				BuildVersion.Windows2019,
				BuildVersion.Windows2021
			}, BuildAssetBundleOptions.None);
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

		private static Task<uint?> FixShaderKeywords(string bundlePath, string targetPath, Logger logger)
		{
			return Task.Run(() => ShaderKeywordRewriter.ShaderKeywordRewriter.Rewrite(bundlePath, targetPath, logger));
		}

		private static async Task<BuildReport> Build(
			BuildSettings buildSettings,
			BuildAssetBundleOptions buildOptions,
			BuildVersion buildVersion,
			Logger logger
		)
		{
			logger.Log($"Building bundle '{ProjectBundle.Value}' for version '{buildVersion.ToString()}'");

			// Check output directory exists
			if (!Directory.Exists(buildSettings.OutputDirectory))
			{
				throw new DirectoryNotFoundException($"The directory '{buildSettings.OutputDirectory}' doesn't exist.");
			}

			// Check bundle isn't empty
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(buildSettings.ProjectBundle);
			if (assetPaths.Length == 0)
			{
				throw new Exception($"The bundle '{buildSettings.ProjectBundle}' is empty.");
			}

			// Check correct packages are being used for XR
			bool isAndroid = buildVersion == BuildVersion.Android2019 || buildVersion == BuildVersion.Android2021;
			bool is2019 = buildVersion == BuildVersion.Windows2019 || buildVersion == BuildVersion.Android2019;
			bool tryToFixShaderKeywords = !is2019;

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
			string tempDir = VersionTools.GetTempDirectory(buildVersion);
			Directory.Delete(tempDir, true);
			Directory.CreateDirectory(tempDir);

			// Set build to uncompressed if it will be compressed by ShaderKeywordsRewriter
			if (tryToFixShaderKeywords)
			{
				buildOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
			}

			// Build
			string builtBundlePath = Path.Combine(tempDir, buildSettings.ProjectBundle); // This is the path to the bundle built by BuildPipeline.
			string fixedBundlePath = null; // This is the path to the bundle built by ShaderKeywordsRewriter.
			string usedBundlePath = builtBundlePath; // This is the path to the bundle actually cloned to the chosen output directory.

			BuildTarget buildTarget = isAndroid ? BuildTarget.Android : EditorUserBuildSettings.activeBuildTarget;

			AssetBundleBuild[] builds = {
				new AssetBundleBuild
				{
					assetBundleName = buildSettings.ProjectBundle,
					assetNames = assetPaths
				}
			};

			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(tempDir, builds, buildOptions, buildTarget);
			if (!manifest)
			{
				throw new Exception("The build was unsuccessful. Check above for possible errors reported by the build pipeline.");
			}

			// Fix new shader keywords
			uint crc = 0;

			bool shaderKeywordsFixed = tryToFixShaderKeywords;
			if (shaderKeywordsFixed)
			{
				logger.Log("2021 version detected, attempting to rebuild shader keywords...");

				string expectedOutput = builtBundlePath + ".fixed";
				uint? resultCRC = await FixShaderKeywords(builtBundlePath, expectedOutput, logger);

				fixedBundlePath = expectedOutput;
				usedBundlePath = expectedOutput;

				if (resultCRC.HasValue)
				{
					crc = resultCRC.Value;
				}
				else
				{
					shaderKeywordsFixed = false;
				}
			}

			if (!shaderKeywordsFixed)
			{
				BuildPipeline.GetCRCForAssetBundle(usedBundlePath, out uint crcOut);
				crc = crcOut;
			}

			// Move into project
			string fileName = VersionTools.GetBundleFileName(buildVersion);
			string outputBundlePath = buildSettings.OutputDirectory + "/" + fileName;

			File.Copy(usedBundlePath, outputBundlePath, true);
			logger.Log($"Successfully built bundle '{buildSettings.OutputDirectory}' to '{outputBundlePath}'.");

			return new BuildReport
			{
				BuiltBundlePath = builtBundlePath,
				FixedBundlePath = fixedBundlePath,
				OutputBundlePath = outputBundlePath,
				ShaderKeywordsFixed = shaderKeywordsFixed,
				CRC = crc,
				IsAndroid = isAndroid,
				BuildTarget = buildTarget,
				BuildVersion = buildVersion
			};
		}

		private static async void BuildSingleUncompressed(BuildVersion version)
		{
			Timer.Mark();
			Logger logger = new Logger();
			BuildSettings buildSettings = BuildSettings.Snapshot();

			if (ShouldExportBundleInfo.Value)
			{
				List<string> bundleFiles = new List<string>();
				Dictionary<string, uint> bundleCRCs = new Dictionary<string, uint>();

				BuildReport build = await Build(buildSettings, BuildAssetBundleOptions.UncompressedAssetBundle, version, logger);
				string versionPrefix = VersionTools.GetVersionPrefix(version);
				bundleCRCs[versionPrefix] = build.CRC;
				bundleFiles.Add(build.OutputBundlePath);

				BundleInfo bundleInfo = new BundleInfo
				{
					bundleFiles = bundleFiles,
					bundleCRCs = bundleCRCs,
					isCompressed = false
				};

				BundleInfoProcessor.Serialize(buildSettings.OutputDirectory, buildSettings.ShouldPrettifyBundleInfo, bundleInfo, logger);
			}
			else
			{
				await Build(buildSettings, BuildAssetBundleOptions.UncompressedAssetBundle, version, logger);
			}

			Debug.Log($"Build done in {Timer.Mark()}s!");
			Debug.Log($"Output: {logger.GetOutput()}");
		}

		public static async void BuildAll(List<BuildVersion> buildVersions, BuildAssetBundleOptions buildOptions)
		{
			Timer.Mark();
			BuildProgressWindow buildProgressWindow = BuildProgressWindow.CreatePopup();
			BuildSettings buildSettings = BuildSettings.Snapshot();

			IEnumerable<Task<BuildReport?>> buildTasks = buildVersions.Select(async version =>
			{
				BuildTask task = buildProgressWindow.AddIndividualBuild(version);

				try
				{
					BuildReport build = await Build(buildSettings, buildOptions, version, task.GetLogger());
					task.Success();
					return (BuildReport?)build;
				}
				catch (Exception e)
				{
					task.Fail($"Error trying to build: {e}");
					return null;
				}
			});
			BuildReport?[] builds = await Task.WhenAll(buildTasks);

			if (buildSettings.ShouldExportBundleInfo)
			{
				ExportBundleInfo(buildOptions, builds.OfType<BuildReport>(), buildProgressWindow, buildSettings);
			}

			buildProgressWindow.FinishBuild($"Build done in {Timer.Mark()}s!");
		}

		private static void ExportBundleInfo(BuildAssetBundleOptions buildOptions, IEnumerable<BuildReport> builds,
			BuildProgressWindow buildProgressWindow, BuildSettings buildSettings)
		{
			bool isCompressed = !buildOptions.HasFlag(BuildAssetBundleOptions.UncompressedAssetBundle);

			BundleInfo bundleInfo = new BundleInfo
			{
				bundleFiles = new List<string>(),
				bundleCRCs = new Dictionary<string, uint>(),
				isCompressed = isCompressed
			};

			foreach (BuildReport build in builds)
			{
				string versionPrefix = VersionTools.GetVersionPrefix(build.BuildVersion);
				bundleInfo.bundleFiles.Add(build.OutputBundlePath);
				bundleInfo.bundleCRCs.Add(versionPrefix, build.CRC);
			}

			SerializeBundleInfo(buildProgressWindow, buildSettings, bundleInfo);
		}

		private static void SerializeBundleInfo(BuildProgressWindow buildProgressWindow, BuildSettings buildSettings, BundleInfo bundleInfo)
		{
			BuildTask serializeTask = buildProgressWindow.StartSerialization();

			try
			{
				BundleInfoProcessor.Serialize(buildSettings.OutputDirectory, buildSettings.ShouldPrettifyBundleInfo, bundleInfo, serializeTask.GetLogger());
				serializeTask.Success();
			}
			catch (Exception e)
			{
				serializeTask.Fail($"Error trying to serialize: {e}");
			}
		}
	}
}
