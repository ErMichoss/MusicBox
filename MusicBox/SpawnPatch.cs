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

        GameObject prefab = valuables[0].gameObject;
        MusicBoxPlugin.Log.LogInfo($"Usando valuable: {prefab.name}");

        Vector3 spawnPos = new Vector3(0f, 1f, 3f);
        GameObject obj = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
        obj.name = "MusicBoxObject";

        // Destruir componentes del juego que causan problemas
        var valuable = obj.GetComponent<ValuableObject>();
        if (valuable != null) Object.Destroy(valuable);

        // Destruir todos los scripts hijos menos el nuestro
        foreach (var mono in obj.GetComponentsInChildren<MonoBehaviour>())
        {
            if (mono == null) continue;
            if (mono.GetType().Name != "MusicBoxItem")
                Object.Destroy(mono);
        }

        obj.AddComponent<MusicBoxItem>();
        MusicBoxPlugin.Log.LogInfo("MusicBox spawneado!");
    }
}