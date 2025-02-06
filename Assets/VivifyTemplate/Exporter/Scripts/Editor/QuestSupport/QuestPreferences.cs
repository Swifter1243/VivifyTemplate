namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class QuestPreferences
    {
        public static readonly string QuestProjectPlayerPrefsKey = "questPath";
        public static readonly string UnityEditorPlayerPrefsKey = "unityEditor";

        public static readonly string UnityHubPath = "C:/Program Files/Unity Hub/Unity Hub.exe"; //This *should* be the same for everyone, need more testing

        public static string ProjectPath
        {
            get => UnityEngine.PlayerPrefs.GetString(QuestProjectPlayerPrefsKey, "");
            set => UnityEngine.PlayerPrefs.SetString(QuestProjectPlayerPrefsKey, value);
        }

        public static string UnityEditor
        {
            get => UnityEngine.PlayerPrefs.GetString(UnityEditorPlayerPrefsKey, "");
            set => UnityEngine.PlayerPrefs.SetString(UnityEditorPlayerPrefsKey, value);
        }
    }
}