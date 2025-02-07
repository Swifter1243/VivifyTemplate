using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Build.Structures;
using VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs;
using VivifyTemplate.Exporter.Scripts.Editor.QuestSupport;
using VivifyTemplate.Exporter.Scripts.Editor.UI;
using VivifyTemplate.Exporter.Scripts.Editor.Utility;
using Debug = UnityEngine.Debug;

namespace VivifyTemplate.Exporter.Scripts.Editor.Build
{
	public static class BuildAssetBundles
	{
		private static readonly SimpleTimer Timer = new SimpleTimer();

		private static Task<uint> FixShaderKeywords(string bundlePath, string targetPath, Logger logger, bool compress)
		{
			return Task.Run(() => ShaderKeywordRewriter.ShaderKeywordRewriter.Rewrite(bundlePath, targetPath, logger, compress));
		}

		private static BuildVersionBuildInfo BuildVersionBuildInfo(BuildVersion version)
		{
			bool is2019 = version == BuildVersion.Windows2019;

			string trimmedVersion = Application.unityVersion.Replace("f1", "");
			return new BuildVersionBuildInfo
			{
				IsAndroid = version == BuildVersion.Android2021,
				Is2019 = is2019,
				NeedsShaderKeywordsFixed = !is2019 && !(Version.Parse(trimmedVersion).Major > 2019),
			};
		}

		private static void ResetStereoRenderingPath()
		{
			PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
			AssetDatabase.SaveAssets();
		}

		public static async Task<BuildReport> Build(
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

		public static async void BuildSingleRequestUncompressed(BuildRequest request)
		{
			EnsureQuestProjectReady(request);
			Timer.Reset();
			AccumulatingLogger mainLogger = new AccumulatingLogger();
			AccumulatingLogger shaderKeywordsLogger = null;
			BuildSettings buildSettings;

			try
			{
				buildSettings = BuildSettings.Snapshot();
			}
			catch
			{
				return;
			}

			Debug.Log($"Building '{buildSettings.ProjectBundle}' for '{request.BuildVersion}' uncompressed to '{buildSettings.OutputDirectory}'...");

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

				BuildReport build = await request.BundleBuilder.Build(buildSettings, BuildAssetBundleOptions.UncompressedAssetBundle, request.BuildVersion, mainLogger, OnShaderKeywordsRewritten);
				string versionPrefix = VersionTools.GetVersionPrefix(request.BuildVersion);
				bundleInfo.bundleCRCs[versionPrefix] = build.CRC;
				bundleInfo.bundleFiles.Add(build.OutputBundlePath);

				BundleInfoProcessor.Serialize(buildSettings.ProjectBundle, buildSettings.OutputDirectory, buildSettings.ShouldPrettifyBundleInfo, bundleInfo, mainLogger);
			}
			else
			{
				await Build(buildSettings, BuildAssetBundleOptions.UncompressedAssetBundle, request.BuildVersion, mainLogger, OnShaderKeywordsRewritten);
			}

			Debug.Log($"Build done in {Timer.Reset()}s!");
			Debug.Log($"--- Main Output --- \n{mainLogger.GetOutput()}");

			if (shaderKeywordsLogger != null)
			{
				Debug.Log($"--- ShaderKeywordsRewriter Output --- \n{shaderKeywordsLogger.GetOutput()}");
			}
		}

		private static void EnsureQuestProjectReady(List<BuildRequest> buildRequests)
		{
			buildRequests.ForEach(EnsureQuestProjectReady);
		}
		private static void EnsureQuestProjectReady(BuildRequest buildRequest)
		{
			if (!QuestSetup.IsQuestProjectReady() && buildRequest.BuildVersion == BuildVersion.Android2021)
			{
				QuestSetup.CreatePopup();
				throw new Exception("Your quest project is not setup!");
			}
		}

		public static async void BuildAllRequests(List<BuildRequest> buildRequests, BuildAssetBundleOptions buildOptions)
		{
			EnsureQuestProjectReady(buildRequests);
			BuildProgressWindow buildProgressWindow = BuildProgressWindow.CreatePopup();
			BuildSettings buildSettings;

			try
			{
				buildSettings = BuildSettings.Snapshot();
			}
			catch
			{
				buildProgressWindow.Close();
				return;
			}

			IEnumerable<Task<BuildReport?>> buildTasks = buildRequests.Select(async request =>
			{
				BuildTask buildTask = buildProgressWindow.AddIndividualBuild(request.BuildVersion);

				try
				{
					await Task.Delay(100);
					buildProgressWindow.OnClose += request.BundleBuilder.Cancel;
					BuildReport build = await request.BundleBuilder.Build(buildSettings, buildOptions, request.BuildVersion, buildTask.GetLogger(),
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
				ExportBundleInfo(buildOptions, builds.OfType<BuildReport>(), buildProgressWindow, buildSettings);
			}

			buildProgressWindow.FinishBuild(buildSettings);
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
				BundleInfoProcessor.Serialize(buildSettings.ProjectBundle, buildSettings.OutputDirectory, buildSettings.ShouldPrettifyBundleInfo, bundleInfo, serializeTask.GetLogger());
				serializeTask.Success();
			}
			catch (Exception e)
			{
				serializeTask.Fail($"Error trying to serialize: {e}");
			}
		}
	}
}
