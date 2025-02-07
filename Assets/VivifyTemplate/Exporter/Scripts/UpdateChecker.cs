using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts
{
	[InitializeOnLoad]
	public static class UpdateChecker
	{
		private readonly static Version TemplateVersion = new Version("1.0.0");
		private readonly static HttpClient Client = new HttpClient();
		private const string INITIALIZE_BOOL = "UpdateCheckerInitialized";
		private const string REPO = "Swifter1243/Remapper";

		static UpdateChecker()
		{
			if (!SessionState.GetBool(INITIALIZE_BOOL, false))
			{
				return;
			}

			Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"Swifter1243/VivifyTemplate/{TemplateVersion}");
			SessionState.SetBool(INITIALIZE_BOOL, true);
			CheckForUpdates();
		}

		private static void CheckForUpdates()
		{
			var remoteVersion = GetGitHubVersion();
			bool updateAvailable = remoteVersion.CompareTo(TemplateVersion) > 0;

			if (updateAvailable)
			{
				UpdateAvailablePopup.Popup();
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
