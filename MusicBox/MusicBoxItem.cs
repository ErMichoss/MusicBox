using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MusicBox
{
    public class MusicBoxItem : MonoBehaviour
    {
        private AudioSource audioSource;
        private List<string> songPaths = new List<string>();
        private int currentIndex = 0;
        private bool showMenu = false;
        private bool isPlaying = false;
        private Vector2 scrollPos = Vector2.zero;
        public PhotonView myView;
        private float volume = 0.8f;

        void Awake()
        {
            MusicBoxPlugin.Log.LogInfo("MusicBoxItem Awake llamado!");
        }

        void Start()
        {
            MusicBoxPlugin.Log.LogInfo("MusicBoxItem Start llamado!");
            //myView = GetComponent<PhotonView>();
            MusicBoxPlugin.Log.LogInfo($"MusicBoxItem Start - PhotonView ID: {myView?.ViewID}");
            // Eliminar mesh visual del valuable clonado
            foreach (var r in GetComponentsInChildren<Renderer>())
                Destroy(r);

            // Cubo rojo propio
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            visual.GetComponent<Renderer>().material.color = Color.red;
            Destroy(visual.GetComponent<Collider>());

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            LoadSongs();
        }

        void Update()
        {
            if (Time.frameCount % 300 == 0)
            {
                MusicBoxPlugin.Log.LogInfo($"[UPDATE] Vivo! showMenu={showMenu}");
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.M))
            {
                showMenu = !showMenu;
                MusicBoxPlugin.Log.LogInfo($"[UPDATE] M PULSADA! showMenu={showMenu}");

                if (showMenu)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        void LoadSongs()
        {
            songPaths.Clear();
            songPaths.AddRange(Directory.GetFiles(MusicBoxPlugin.SongsFolder, "*.mp3"));
            songPaths.AddRange(Directory.GetFiles(MusicBoxPlugin.SongsFolder, "*.wav"));
            MusicBoxPlugin.Log.LogInfo($"MusicBox: {songPaths.Count} canciones encontradas.");
        }

        void PlayRequest(string songName)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
                myView.RPC("RPC_Play", RpcTarget.All, songName);
            else
                RPC_Play(songName);
        }

        void StopRequest()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
                myView.RPC("RPC_Stop", RpcTarget.All);
            else
                RPC_Stop();
        }

        [PunRPC]
        void RPC_Play(string songName) {
            string foundPath = FindSongPath(songName);
            if (foundPath != null)
                StartCoroutine(PlaySong(foundPath));
        }

        [PunRPC]
        void RPC_Stop() {
            StopSong();
        }

        string FindSongPath(string songName)
        {
            foreach (string path in songPaths)
            {
                if (Path.GetFileNameWithoutExtension(path) == songName)
                    return path;
            }
            return null;
        }

        void OnGUI()
        {
            if (!showMenu) return;

            GUILayout.BeginArea(new Rect(Screen.width / 2 - 175, Screen.height / 2 - 250, 350, 480));
            GUILayout.BeginVertical("box");
            GUILayout.Label("🎵 MusicBox");
            GUILayout.Space(10);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            for (int i = 0; i < songPaths.Count; i++)
            {
                string songName = Path.GetFileNameWithoutExtension(songPaths[i]);
                string prefix = (i == currentIndex && isPlaying) ? "▶ " : "   ";
                if (GUILayout.Button(prefix + songName))
                {
                    currentIndex = i;
                    PlayRequest(songName);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Label($"🔊 Volumen: {Mathf.RoundToInt(volume * 100)}%");
            float newVolume = GUILayout.HorizontalSlider(volume, 0f, 1f);
            if (newVolume != volume)
            {
                volume = newVolume;
                audioSource.volume = volume;
            }

            GUILayout.Space(5);
            if (GUILayout.Button(isPlaying ? "⏹ Stop" : "▶ Play"))
            {
                if (isPlaying)
                    StopRequest();
                else if (songPaths.Count > 0)
                    PlayRequest(Path.GetFileNameWithoutExtension(songPaths[currentIndex]));
            }
            if (GUILayout.Button("✕ Cerrar"))
            {
                showMenu = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        IEnumerator PlaySong(string path)
        {
            StopSong();
            AudioType audioType = path.EndsWith(".wav") ? AudioType.WAV : AudioType.MPEG;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, audioType))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = clip;
                    audioSource.Play();
                    isPlaying = true;
                    MusicBoxPlugin.Log.LogInfo($"Reproduciendo: {Path.GetFileNameWithoutExtension(path)}");
                }
                else
                {
                    MusicBoxPlugin.Log.LogInfo($"Error cargando canción: {www.error}");
                }
            }
        }

        void StopSong()
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }
}