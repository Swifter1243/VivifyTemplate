using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BackgroundTaskState
{
    Idle,
    SearchingEditors,
    DownloadingEditor,
    DownloadingAndroidBuildSupport,
    CreatingProject,
    AddingPackages,
}