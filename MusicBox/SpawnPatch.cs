using ExitGames.Client.Photon;
using HarmonyLib;
using MusicBox;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[HarmonyPatch(typeof(LevelGenerator), "GenerateDone")]
public class SpawnPatch
{
    static void Postfix()
    {
        if (!SemiFunc.RunIsLevel()) return;

        // Crear spawner si no existe
        if (GameObject.Find("MusicBoxNetworkSpawner") == null)
        {
            GameObject spawnerObj = new GameObject("MusicBoxNetworkSpawner");
            spawnerObj.AddComponent<MusicBoxNetworkSpawner>();
            GameObject.DontDestroyOnLoad(spawnerObj);
            MusicBoxPlugin.Log.LogInfo("Spawner creado!");
        }

        if (GameObject.Find("MusicBoxObject") != null) return;

        Vector3 spawnPos = new Vector3(0f, 1f, 3f);

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int viewID = PhotonNetwork.AllocateViewID(true);
            object[] data = new object[] { viewID, spawnPos };

            PhotonNetwork.RaiseEvent(
                77,
                data,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All,
                    CachingOption = EventCaching.AddToRoomCache
                },
                SendOptions.SendReliable
            );
            MusicBoxPlugin.Log.LogInfo($"Evento 77 enviado con viewID={viewID}");
        }
        else
        {
            ValuableObject[] valuables = Resources.FindObjectsOfTypeAll<ValuableObject>();
            if (valuables.Length == 0) return;

            GameObject obj = GameObject.Instantiate(valuables[0].gameObject, spawnPos, Quaternion.identity);
            obj.name = "MusicBoxObject";

            var valuable = obj.GetComponent<ValuableObject>();
            if (valuable != null) Object.Destroy(valuable);

            obj.AddComponent<MusicBoxItem>();
            MusicBoxPlugin.Log.LogInfo("MusicBox spawneado (singleplayer)!");
        }
    }
}