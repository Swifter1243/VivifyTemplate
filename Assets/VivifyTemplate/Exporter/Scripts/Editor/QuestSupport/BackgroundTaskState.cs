using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public enum BackgroundTaskState
    {
        Idle,
        SearchingEditors,
        DownloadingEditor,
        DownloadingAndroidBuildSupport,
        CreatingProject,
        AddingPackages,
    }
}