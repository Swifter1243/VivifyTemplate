using System;
using UnityEditor;
using UnityEngine;
namespace VivifyTemplate.Exporter.Scripts
{
	[InitializeOnLoad]
	public static class UpdateChecker
	{
		private readonly static Version TemplateVersion = new Version("1.0.0");
		private readonly static string InitializeBool = "UpdateCheckerInitialized";

		static UpdateChecker()
		{
			if (SessionState.GetBool(InitializeBool, false))
			{
				return;
			}

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
			throw new NotImplementedException();
		}
	}
}
