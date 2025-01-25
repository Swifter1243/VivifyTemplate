using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Structures;
using Debug = UnityEngine.Debug;

namespace VivifyTemplate.Exporter.Scripts.Editor
{
	public static class BuildAssetBundles
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

		private static Task<uint> FixShaderKeywords(string bundlePath, string targetPath, Logger logger, bool compress)
		{
			return Task.Run(() => ShaderKeywordRewriter.ShaderKeywordRewriter.Rewrite(bundlePath, targetPath, logger, compress));
		}

		private static BuildVersionBuildInfo BuildVersionBuildInfo(BuildVersion version)
		{
			bool is2019 = version == BuildVersion.Windows2019;

			return new BuildVersionBuildInfo
			{
				IsAndroid = version == BuildVersion.Android2021,
				Is2019 = is2019,
				NeedsShaderKeywordsFixed = version == BuildVersion.Windows2021,
			};
		}

		private static void ResetStereoRenderingPath()
		{
			PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
			AssetDatabase.SaveAssets();
		}

		private static async Task<BuildReport> Build(
			BuildSettings buildSettings,
			BuildAssetBundleOptions buildOptions,
			BuildVersion buildVersion,
			Logger mainLogger,
			Action<BuildTask> shaderKeywordRewriterAction
		)
		{
			mainLogger.Log($"Building bundle '{ProjectBundle.Value}' for version '{buildVersion.ToString()}'");

			// Check output directory exists
			IOHelper.AssertDirectoryExists(buildSettings.OutputDirectory);

			// Get asset bundle paths
			string[] assetPaths = GetBundleAssetPaths(buildSettings.ProjectBundle);

			// Get info about build version
			BuildVersionBuildInfo buildVersionBuildInfo = BuildVersionBuildInfo(buildVersion);

			// Check that the right XR packages are being used
			CheckXRPackages(buildVersion, buildVersionBuildInfo);

			// Check if bundle is compressed
			bool isCompressed = !buildOptions.HasFlag(BuildAssetBundleOptions.UncompressedAssetBundle);

			// Adjust build options
			buildOptions = AdjustBuildOptionsForBuild(buildOptions, buildVersionBuildInfo);

			// Set Single Pass mode
			VersionTools.SetSinglePassMode(buildVersion);

			// Empty build location directory
			string tempDirectory = VersionTools.GetTempDirectory(buildVersion);
			IOHelper.WipeDirectory(tempDirectory);

			// Build
			string builtBundlePath = Path.Combine(tempDirectory, buildSettings.ProjectBundle); // This is the path to the bundle built by BuildPipeline.
			string fixedBundlePath = null; // This is the path to the bundle built by ShaderKeywordsRewriter.
			string usedBundlePath = builtBundlePath; // This is the path to the bundle actually cloned to the chosen output directory.

			BuildTarget buildTarget = DoBuild(buildSettings, buildOptions, buildVersionBuildInfo, assetPaths, tempDirectory);

			// Set Single Pass mode back
			ResetStereoRenderingPath();

			// Fix new shader keywords
			uint crc = 0;

			bool shaderKeywordsFixed = buildVersionBuildInfo.NeedsShaderKeywordsFixed;
			if (shaderKeywordsFixed)
			{
				mainLogger.Log("2021 version detected, attempting to rebuild shader keywords...");

				string expectedOutput = builtBundlePath + ".fixed";

				BuildTask buildTask = new BuildTask("Rewriting Shader Keywords for " + buildVersion);
				shaderKeywordRewriterAction.Invoke(buildTask);

				try
				{
					crc = await FixShaderKeywords(builtBundlePath, expectedOutput, buildTask.GetLogger(), isCompressed);
					fixedBundlePath = expectedOutput;
					usedBundlePath = expectedOutput;

					buildTask.Success();
				}
				catch (Exception e)
				{
					buildTask.Fail("There was an error trying to rewrite shader keywords: " + e);
				}
			}
			else
			{
				BuildPipeline.GetCRCForAssetBundle(usedBundlePath, out uint crcOut);
				crc = crcOut;
			}

			// Move into project
			string fileName = VersionTools.GetBundleFileName(buildVersion);
			string outputBundlePath = buildSettings.OutputDirectory + "/" + fileName;

			File.Copy(usedBundlePath, outputBundlePath, true);
			mainLogger.Log($"Successfully built bundle '{buildSettings.OutputDirectory}' to '{outputBundlePath}'.");

			return new BuildReport
			{
				BuiltBundlePath = builtBundlePath,
				FixedBundlePath = fixedBundlePath,
				OutputBundlePath = outputBundlePath,
				ShaderKeywordsFixed = shaderKeywordsFixed,
				CRC = crc,
				BuildVersionBuildInfo = buildVersionBuildInfo,
				BuildTarget = buildTarget,
				BuildVersion = buildVersion
			};
		}

		private static BuildTarget DoBuild(BuildSettings buildSettings, BuildAssetBundleOptions buildOptions,
			BuildVersionBuildInfo buildVersionBuildInfo, string[] assetPaths, string tempDirectory)
		{
			BuildTarget buildTarget = buildVersionBuildInfo.IsAndroid ? BuildTarget.Android : EditorUserBuildSettings.activeBuildTarget;

			AssetBundleBuild[] builds = {
				new AssetBundleBuild
				{
					assetBundleName = buildSettings.ProjectBundle,
					assetNames = assetPaths
				}
			};

			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(tempDirectory, builds, buildOptions, buildTarget);
			if (!manifest)
			{
				throw new Exception("The build was unsuccessful. Check above for possible errors reported by the build pipeline.");
			}

			return buildTarget;
		}

		private static BuildAssetBundleOptions AdjustBuildOptionsForBuild(BuildAssetBundleOptions buildOptions,
			BuildVersionBuildInfo buildVersionBuildInfo)
		{
			// Ensure rebuild
			buildOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

			// Set build to uncompressed if it will be compressed by ShaderKeywordsRewriter
			if (buildVersionBuildInfo.NeedsShaderKeywordsFixed)
			{
				buildOptions |= BuildAssetBundleOptions.UncompressedAssetBundle;
			}

			return buildOptions;
		}

		private static string[] GetBundleAssetPaths(string bundleName)
		{
			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
			if (assetPaths.Length == 0)
			{
				throw new Exception($"The bundle '{bundleName}' contained no assets. Try adding assets to the asset bundle.");
			}

			return assetPaths;
		}

		private static void CheckXRPackages(BuildVersion buildVersion, BuildVersionBuildInfo buildVersionBuildInfo)
		{
			if (buildVersionBuildInfo.Is2019 && XRPluginHelper.IsInstalled()) {
				string name = Enum.GetName(typeof(BuildVersion), buildVersion);
				throw new Exception($"Version '{name}' requires Single Pass which doesn't exist on the new XR packages. Please go to Window > Package Manager and remove them.");
			}
		}

		private static async void BuildSingleUncompressed(BuildVersion version)
		{
			Timer.Reset();
			Logger mainLogger = new Logger();
			Logger shaderKeywordsLogger = null;
			BuildSettings buildSettings = BuildSettings.Snapshot();

			Debug.Log($"Building '{buildSettings.ProjectBundle}' for '{version}' uncompressed to '{buildSettings.OutputDirectory}'...");

			void OnShaderKeywordsRewritten(BuildTask buildTask)
			{
				shaderKeywordsLogger = buildTask.GetLogger();
			}

			if (ShouldExportBundleInfo.Value)
			{
				BundleInfo bundleInfo = new BundleInfo
				{
					bundleFiles = new List<string>(),
					bundleCRCs = new Dictionary<string, uint>(),
					isCompressed = false
				};

				BuildReport build = await Build(buildSettings, BuildAssetBundleOptions.UncompressedAssetBundle, version, mainLogger, OnShaderKeywordsRewritten);
				string versionPrefix = VersionTools.GetVersionPrefix(version);
				bundleInfo.bundleCRCs[versionPrefix] = build.CRC;
				bundleInfo.bundleFiles.Add(build.OutputBundlePath);

				BundleInfoProcessor.Serialize(buildSettings.OutputDirectory, buildSettings.ShouldPrettifyBundleInfo, bundleInfo, mainLogger);
			}
			else
			{
				await Build(buildSettings, BuildAssetBundleOptions.UncompressedAssetBundle, version, mainLogger, OnShaderKeywordsRewritten);
			}

			Debug.Log($"Build done in {Timer.Reset()}s!");
			Debug.Log($"--- Main Output --- \n{mainLogger.GetOutput()}");

			if (shaderKeywordsLogger != null)
			{
				Debug.Log($"--- ShaderKeywordsRewriter Output --- \n{shaderKeywordsLogger.GetOutput()}");
			}
		}

		public static async void BuildAll(List<BuildVersion> buildVersions, BuildAssetBundleOptions buildOptions)
		{
			BuildProgressWindow buildProgressWindow = BuildProgressWindow.CreatePopup();
			BuildSettings buildSettings = BuildSettings.Snapshot();

			IEnumerable<Task<BuildReport?>> buildTasks = buildVersions.Select(async version =>
			{
				BuildTask buildTask = buildProgressWindow.AddIndividualBuild(version);

				try
				{
					await Task.Delay(100);
					BuildReport build = await Build(buildSettings, buildOptions, version, buildTask.GetLogger(),
						buildProgressWindow.AddShaderKeywordsRewriterTask);
					buildTask.Success();
					return (BuildReport?)build;
				}
				catch (Exception e)
				{
					buildTask.Fail($"Error trying to build: {e}");
					return null;
				}
			});
			BuildReport?[] builds = await Task.WhenAll(buildTasks);

			if (buildSettings.ShouldExportBundleInfo)
			{
				await Task.Delay(100);
				IEnumerable<BuildReport> successfulBuilds = builds.OfType<BuildReport>();

				string bundleInfoPath = Path.Combine(buildSettings.OutputDirectory, BundleInfoProcessor.BUNDLE_INFO_FILENAME);
				bool bundleInfoExists = File.Exists(bundleInfoPath);
				if (TryGetAndroid2021Build(successfulBuilds, out BuildReport androidBuild) && bundleInfoExists)
				{
					// temp jank fix to prevent me from having to manually merge bundleinfo.json files
					string bundleInfoText = File.ReadAllText(bundleInfoPath);
					BundleInfo bundleInfo = JsonConvert.DeserializeObject<BundleInfo>(bundleInfoText);
					string versionPrefix = VersionTools.GetVersionPrefix(androidBuild.BuildVersion);
					bundleInfo.bundleCRCs[versionPrefix] = androidBuild.CRC;

					Formatting formatting = buildSettings.ShouldPrettifyBundleInfo ? Formatting.Indented : Formatting.None;
					bundleInfo.bundleFiles = bundleInfo.bundleFiles.Where(x => x != androidBuild.OutputBundlePath).ToList();
					bundleInfo.bundleFiles.Add(androidBuild.OutputBundlePath);
					string bundleInfoOutputText = JsonConvert.SerializeObject(bundleInfo, formatting);
					File.WriteAllText(bundleInfoPath, bundleInfoOutputText);
				}
				else
				{
					ExportBundleInfo(buildOptions, builds.OfType<BuildReport>(), buildProgressWindow, buildSettings);
				}
			}

			buildProgressWindow.FinishBuild(buildSettings);
		}

		private static bool TryGetAndroid2021Build(IEnumerable<BuildReport> builds, out BuildReport androidBuild)
		{
			foreach (var build in builds)
			{
				if (build.BuildVersion == BuildVersion.Android2021)
				{
					androidBuild = build;
					return true;
				}
			}

			androidBuild = default;
			return false;
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
