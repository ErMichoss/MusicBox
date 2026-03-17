using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace MusicBox
{
    [BepInPlugin("com.tuNombre.musicbox", "MusicBox", "1.0.0")]
    public class MusicBoxPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        public static string SongsFolder;

        private void Awake()
        {
            Log = Logger;

            // Carpeta donde el usuario mete sus .mp3
            SongsFolder = Path.Combine(Paths.PluginPath, "MusicBox", "songs");
            Directory.CreateDirectory(SongsFolder);

            Log.LogInfo($"MusicBox cargado. Carpeta de canciones: {SongsFolder}");

            var harmony = new Harmony("com.tuNombre.musicbox");
            harmony.PatchAll();
        }
    }
}