using HarmonyLib;
using MusicBox;
using UnityEngine;

[HarmonyPatch(typeof(LevelGenerator), "GenerateDone")]
public class SpawnPatch
{
    static void Postfix()
    {
        if (!SemiFunc.RunIsLevel()) return;
        if (GameObject.Find("MusicBoxObject") != null) return;

        ValuableObject[] valuables = Resources.FindObjectsOfTypeAll<ValuableObject>();

        if (valuables.Length == 0)
        {
            MusicBoxPlugin.Log.LogInfo("No se encontraron valuables.");
            return;
        }

        // Usar el primero que encuentre
        GameObject prefab = valuables[0].gameObject;
        MusicBoxPlugin.Log.LogInfo($"Usando valuable: {prefab.name}");

        Vector3 spawnPos = new Vector3(0f, 1f, 3f);
        GameObject obj = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
        obj.name = "MusicBoxObject";
        obj.AddComponent<MusicBoxItem>();
        MusicBoxPlugin.Log.LogInfo("MusicBox spawneado como valuable!");
    }
}