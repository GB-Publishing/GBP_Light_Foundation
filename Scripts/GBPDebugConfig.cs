using System;
using System.Collections;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Networking;

namespace Gamebee
{
    [Serializable]
    public class GBPDebugConfig
    {
        public bool ShowFPS;
        public int[] DebugRect;
        public int FontSize;

        private static GBPDebugConfig instance;
        private static Rect debugRect = new Rect(0, 0, 0, 0);

        private static int multiplier = 1024 * 1024;
        private static ProfilerRecorder totalReservedMemoryRecorder;
        private static ProfilerRecorder gcReservedMemoryRecorder;
        private static ProfilerRecorder systemUsedMemoryRecorder;


        public static IEnumerator Load()
        {
            var url = Application.streamingAssetsPath + "/GBPDebugConfig.json";
            if (Application.isEditor) url = "file://" + url;
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) yield break;

            instance = JsonUtility.FromJson<GBPDebugConfig>(request.downloadHandler.text);

            totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
            gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

            debugRect = new Rect(instance.DebugRect[0], instance.DebugRect[1], instance.DebugRect[2], instance.DebugRect[3]);
        }

        private static StringBuilder sb = new StringBuilder();
        private static long app, gc, sys, total;

        public static void CalculateStats()
        {
            if (instance is not { ShowFPS: true }) return;

            app = totalReservedMemoryRecorder.LastValue / multiplier;
            gc = gcReservedMemoryRecorder.LastValue / multiplier;
            sys = systemUsedMemoryRecorder.LastValue / multiplier;
            total = app + gc + sys;

            sb.Clear();
            sb.AppendLine($"FPS:\t{GBPManager.CurrentFPS}");
            sb.AppendLine($"Memory:\t{total} / {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"-> App:\t{app} MB");
            sb.AppendLine($"-> GC:\t{gc} MB");
            sb.AppendLine($"-> Sys:\t{sys} MB");
        }

        public static void OnGUI()
        {
            if (instance is not { ShowFPS: true }) return;

            var style = GUI.skin.box;
            style.fontSize = instance.FontSize;
            style.alignment = TextAnchor.UpperLeft;
            GUI.Box(debugRect, sb.ToString());
        }
    }
}