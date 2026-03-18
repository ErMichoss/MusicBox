using ExitGames.Client.Photon;
using MusicBox;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MusicBoxNetworkSpawner : MonoBehaviour, IOnEventCallback
{
    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != 77) return;

        object[] data = (object[])photonEvent.CustomData;
        int viewID = (int)data[0];
        Vector3 pos = (Vector3)data[1];

        MusicBoxPlugin.Log.LogInfo($"Evento 77 recibido! viewID={viewID}");
        SpawnLocal(pos, viewID);
    }

    void SpawnLocal(Vector3 pos, int viewID)
    {
        if (GameObject.Find("MusicBoxObject") != null) return;

        ValuableObject[] valuables = Resources.FindObjectsOfTypeAll<ValuableObject>();
        if (valuables.Length == 0) return;

        GameObject obj = GameObject.Instantiate(valuables[0].gameObject, pos, Quaternion.identity);
        obj.name = "MusicBoxObject";

        var valuable = obj.GetComponent<ValuableObject>();
        if (valuable != null) Object.Destroy(valuable);

        // Reutilizar el PhotonView que ya viene con el objeto clonado
        PhotonView view = obj.GetComponent<PhotonView>();
        if (view == null)
            view = obj.AddComponent<PhotonView>();

        view.ViewID = viewID;
        PhotonNetwork.RegisterPhotonView(view);
        MusicBoxPlugin.Log.LogInfo($"PhotonView registrado con ID: {view.ViewID}");

        var item = obj.AddComponent<MusicBoxItem>();
        item.myView = view;

        MusicBoxPlugin.Log.LogInfo("MusicBoxObject spawneado via evento!");
    }
}