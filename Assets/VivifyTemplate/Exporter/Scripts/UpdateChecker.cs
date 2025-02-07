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
		private static readonly Version TemplateVersion = new Version("1.0.0");
		private static readonly HttpClient Client = new HttpClient();
		private const string InitializeBool = "UpdateCheckerInitialized";
		private const string Repo = "Swifter1243/Remapper";

		static UpdateChecker()
		{
			Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"Swifter1243/VivifyTemplate/{TemplateVersion}");
			SessionState.SetBool(InitializeBool, true);
			CheckForUpdates();
		}

		private static void CheckForUpdates()
		{
			var remoteVersion = GetGitHubVersion();
			bool updateAvailable = remoteVersion.CompareTo(TemplateVersion) > 0;

			if (updateAvailable)
			{
				Debug.Log("A new update for VivifyTemplate is available! <a href=\\\"https://github.com/Swifter1243/VivifyTemplate/releases/latest\\\">Click here to get it.</a>");
			}
		}

		private static Version GetGitHubVersion()
		{
			try
			{
				Task<HttpResponseMessage> response = Client.GetAsync($"https://api.github.com/repos/{Repo}/tags");
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
