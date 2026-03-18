using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace MusicBox
{
    [BepInPlugin("com.ErMichos.musicbox", "MusicBox", "1.0.2")]
    public class MusicBoxPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        public static string SongsFolder;
        private void Awake()
        {
            Log = Logger;
            SongsFolder = Path.Combine(Paths.PluginPath, "ErMichos-MusicBox", "songs");
            Directory.CreateDirectory(SongsFolder);
            Log.LogInfo($"MusicBox cargado. Carpeta de canciones: {SongsFolder}");

            var harmony = new Harmony("com.ErMichos.musicbox");
            harmony.PatchAll();
        }
    }
}