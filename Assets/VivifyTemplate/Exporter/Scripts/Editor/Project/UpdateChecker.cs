using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace VivifyTemplate.Exporter.Scripts.Editor.Project
{
	[InitializeOnLoad]
	public static class UpdateChecker
	{
		private readonly static Version TemplateVersion = new Version("1.1.2");
		private readonly static HttpClient Client = new HttpClient();
		private const string INITIALIZE_BOOL = "UpdateCheckerInitialized";
		private const string REPO = "Swifter1243/VivifyTemplate";

		public struct UpdateAvailableData
		{
			public Version OldVersion;
			public Version NewVersion;
		}

		public static UpdateAvailableData? PossibleUpdate;

		static UpdateChecker()
		{
			if (SessionState.GetBool(INITIALIZE_BOOL, false))
			{
				return;
			}

			SessionState.SetBool(INITIALIZE_BOOL, true);
			CheckForUpdates();
		}

		private static void CheckForUpdates()
		{
			Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"Swifter1243/VivifyTemplate/{TemplateVersion}");

			Version remoteVersion;

			try
			{
				remoteVersion = GetGitHubVersion();
			}
			catch
			{
				Debug.Log($"Could not connect to {REPO}.");
				return;
			}

			bool updateAvailable = remoteVersion.CompareTo(TemplateVersion) > 0;

			if (updateAvailable)
			{
				PossibleUpdate = new UpdateAvailableData
				{
					OldVersion = TemplateVersion,
					NewVersion = remoteVersion
				};
			}
		}

		private static Version GetGitHubVersion()
		{
			try
			{
				Task<HttpResponseMessage> response = Client.GetAsync($"https://api.github.com/repos/{REPO}/tags");
				response.Wait();
				response.Result.EnsureSuccessStatusCode();
				Task<string> responseBody = response.Result.Content.ReadAsStringAsync();
				responseBody.Wait();
				GithubVersion[] versions = Newtonsoft.Json.JsonConvert.DeserializeObject<GithubVersion[]>(responseBody.Result);
				return Version.Parse(versions[0].name);
			}
			catch (HttpRequestException e)
			{
				Debug.LogException(e);
			}

			throw new ApplicationException("Failed to get latest version from GitHub");
		}

		[Serializable]
		private class GithubVersion
		{
			public string name;
		}
	}
}
