using System;
using UnityEditor;
namespace VivifyTemplate.Exporter.Scripts
{
	[InitializeOnLoad]
	public static class UpdateChecker
	{
		private static Version _templateVersion = new Version("1.0.0");

		static UpdateChecker()
		{

		}

		private static void CheckForUpdates()
		{

		}

		private static Version GetGitHubVersion()
		{
			throw new NotImplementedException();
		}
	}
}
