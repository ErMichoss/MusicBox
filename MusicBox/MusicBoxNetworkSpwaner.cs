using ExitGames.Client.Photon;
using MusicBox;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MusicBoxNetworkSpawner : MonoBehaviour, IOnEventCallback
{
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != 77) return;

        object[] data = (object[])photonEvent.CustomData;

        int viewID = (int)data[0];
        Vector3 pos = (Vector3)data[1];

        SpawnLocal(pos, viewID);
        Debug.Log("Evento recibido: " + photonEvent.Code);
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        Debug.Log("Spawner activo y escuchando eventos");
    }
    void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    void SpawnLocal(Vector3 pos, int viewID)
    {
        if (GameObject.Find("MusicBoxObject") != null) return;

        ValuableObject[] valuables = Resources.FindObjectsOfTypeAll<ValuableObject>();
        if (valuables.Length == 0) return;

        GameObject prefab = valuables[0].gameObject;

        GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        obj.name = "MusicBoxObject";

        // Limpiar
        var valuable = obj.GetComponent<ValuableObject>();
        if (valuable != null) Object.Destroy(valuable);

        // 🔥 CLAVE: PhotonView
        PhotonView view = obj.AddComponent<PhotonView>();
        view.ViewID = viewID;

        obj.AddComponent<MusicBoxItem>();
    }
}