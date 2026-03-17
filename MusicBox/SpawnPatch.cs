using HarmonyLib;
using MusicBox;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;

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
        }
        else
        {
            SpawnLocal(spawnPos, 0);
        }
        MusicBoxPlugin.Log.LogInfo("MusicBox spawneado!");
    }

    static void SpawnLocal(Vector3 pos, int viewID)
    {
        ValuableObject[] valuables = Resources.FindObjectsOfTypeAll<ValuableObject>();
        if (valuables.Length == 0) return;

        GameObject prefab = valuables[0].gameObject;

        GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        obj.name = "MusicBoxObject";

        var valuable = obj.GetComponent<ValuableObject>();
        if (valuable != null) Object.Destroy(valuable);

        PhotonView view = obj.AddComponent<PhotonView>();
        view.ViewID = viewID;

        obj.AddComponent<MusicBoxItem>();
    }
}