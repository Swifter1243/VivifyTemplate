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
		private static readonly SimpleTimer Timer = new SimpleTimer();
		private static readonly Dictionary<string, uint> InternalBuildCRCs = new Dictionary<string, uint>();

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

		private static async Task<bool> FixShaderKeywords(string bundlePath, string expectedOutput)
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

			string output = null;
			string error = null;

			await Task.Run(() =>
			{
				Process process = Process.Start(processInfo);
				if (process == null)
				{
					throw new InvalidOperationException("Shader Keyword Rewriter program was null.");
				}
			
				output = process.StandardOutput.ReadToEnd();
				error = process.StandardError.ReadToEnd();
					
				process.WaitForExit();
			});
				
			Debug.Log($"ShaderKeywordRewriter: {output}");
			if (!string.IsNullOrEmpty(error))
			{
				throw new Exception($"Error from ShaderKeywordsRewriter: {error}");
			}
			
			// Check if fixed bundle generated
			return File.Exists(expectedOutput);
		}

		private static async Task<BuildReport> Build(
			string outputDirectory,
			BuildAssetBundleOptions buildOptions,
			BuildVersion buildVersion
		)
		{
			Debug.Log($"Building bundle '{ProjectBundle.Value}' for version '{buildVersion.ToString()}'");
			
			// Check output directory exists
			if (!Directory.Exists(outputDirectory))
			{
				throw new DirectoryNotFoundException($"The directory '{outputDirectory}' doesn't exist.");
			}

			string projectBundleName = ProjectBundle.Value;

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
			string tempDir = VersionTools.GetTempDirectory(buildVersion);
			Directory.Delete(tempDir, true);
			Directory.CreateDirectory(tempDir);

			// Build
			string tempBundlePath = Path.Combine(tempDir, projectBundleName);
			string builtBundlePath = tempBundlePath;
			string fixedBundlePath = null;
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
				Debug.Log("2021 version detected, attempting to rebuild shader keywords...");
				
				string expectedOutput = Path.ChangeExtension(tempBundlePath, ".mod.avatar");
				bool success = await FixShaderKeywords(tempBundlePath, expectedOutput);
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
			BuildSingleUncompressed(WorkingVersion.Value);
		}
		
		private static async void BuildSingleUncompressed(BuildVersion version)
		{
			Timer.Mark();
			
			// Get Directory
			string outputDirectory = OutputDirectory.Get();

			if (ExportAssetInfo.Value)
			{
				GenerateBundleInfo.BundleInfo bundleInfo = new GenerateBundleInfo.BundleInfo();
				BuildReport build = await Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, version);
				string versionPrefix = VersionTools.GetVersionPrefix(version);
				uint crc = build.crc ?? await CRCGrabber.GetCRCFromFile(build.outputBundlePath);
				InternalBuildCRCs[versionPrefix] = crc;
				bundleInfo.bundleCRCs = InternalBuildCRCs;
				GenerateBundleInfo.Run(build.outputBundlePath, outputDirectory, bundleInfo);
			}
			else
			{
				await Build(outputDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, version);
			}
			
			Debug.Log($"Build done in {Timer.Mark()}s!");
		}

		[MenuItem("Vivify/Build/Build All Versions Compressed")]
		private static async void BuildAllCompressed()
		{
			Timer.Mark();
			
			// Get Directory
			string outputDirectory = OutputDirectory.Get();

			Debug.Log($"Building Windows versions for bundle '{ProjectBundle.Value}' compressed.");

			List<BuildReport> builds = new List<BuildReport>
			{
				// Build Asset Bundle
				await Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2019),
				await Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Windows2021)
			};

			if (BuildAndroidVersion.Value)
			{
				Debug.Log($"Building Android versions for bundle '{ProjectBundle.Value}' compressed.");
				
				try
				{
					builds.Add(await Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2019));
					builds.Add(await Build(outputDirectory, BuildAssetBundleOptions.None, BuildVersion.Android2021));
				}
				catch (Exception e)
				{
					Debug.LogError($"Error trying to build for Android: {e}");
				}
			}

			if (ExportAssetInfo.Value)
			{
				GenerateBundleInfo.BundleInfo bundleInfo = new GenerateBundleInfo.BundleInfo();
				
				IEnumerable<Task> tasks = builds.Select(async build =>
				{
					uint crc = build.crc ?? await CRCGrabber.GetCRCFromFile(build.outputBundlePath);
					string versionPrefix = VersionTools.GetVersionPrefix(build.buildVersion);
					InternalBuildCRCs[versionPrefix] = crc;
				});
				await Task.WhenAll(tasks);

				bundleInfo.bundleCRCs = InternalBuildCRCs;
				GenerateBundleInfo.Run(builds[0].outputBundlePath, outputDirectory, bundleInfo);
			}

			Debug.Log($"All builds done in {Timer.Mark()}s!");
		}
	}
}