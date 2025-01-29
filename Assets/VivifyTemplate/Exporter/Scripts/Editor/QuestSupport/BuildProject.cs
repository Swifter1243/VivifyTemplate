using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using VivifyTemplate.Exporter.Scripts.Editor.Sockets;

namespace VivifyTemplate.Exporter.Scripts.Editor.QuestSupport
{
    public static class BuildProject
    {
        public static void Build()
        {
            RemoteSocket.Initialize();
        }
    }
}