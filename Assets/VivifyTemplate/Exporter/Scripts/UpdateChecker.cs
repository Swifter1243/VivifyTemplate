using System;
using UnityEditor;
using UnityEngine;
namespace VivifyTemplate.Exporter.Scripts
{
	[InitializeOnLoad]
	public static class UpdateChecker
	{
		private static Version _templateVersion = new Version("1.0.0");

		static UpdateChecker()
		{
			CheckForUpdates();
		}

		private static void CheckForUpdates()
		{
			var remoteVersion = GetGitHubVersion();
			bool updateAvailable = remoteVersion.CompareTo(_templateVersion) > 0;

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
