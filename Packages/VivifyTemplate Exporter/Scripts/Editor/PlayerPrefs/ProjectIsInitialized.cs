namespace VivifyTemplate.Exporter.Scripts.Editor.PlayerPrefs
{
	public class ProjectIsInitialized
	{
		private readonly static string PlayerPrefsKey = "projectIsInitialized";

		public static bool Value
		{
			get => UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKey, 0) == 1;
			set => UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKey, value ? 1 : 0);
		}
	}
}
