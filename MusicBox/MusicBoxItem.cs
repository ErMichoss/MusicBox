using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Photon.Pun;

namespace MusicBox
{
    public class MusicBoxItem : MonoBehaviourPun
    {
        private AudioSource audioSource;
        private List<string> songPaths = new List<string>();
        private int currentIndex = 0;
        private bool showMenu = false;
        private bool isPlaying = false;
        private Vector2 scrollPos = Vector2.zero;

        void Start()
        {
            MusicBoxPlugin.Log.LogInfo("MusicBoxItem Start llamado!");

            // Visual temporal - cubo rojo
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            visual.GetComponent<Renderer>().material.color = Color.red;
            Destroy(visual.GetComponent<Collider>());

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.8f;
            LoadSongs();
        }

        void Update()
        {
            // Log cada pocos segundos para saber si Update corre
            if (Time.frameCount % 300 == 0)
            {
                MusicBoxPlugin.Log.LogInfo($"[UPDATE] Vivo! showMenu={showMenu}");
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.M))
            {
                MusicBoxPlugin.Log.LogInfo("[UPDATE] M PULSADA!");
                showMenu = !showMenu;
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

        void OnGUI()
        {
            if (!showMenu) return;

            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 400));
            GUILayout.BeginVertical("box");
            GUILayout.Label("🎵 MusicBox");
            GUILayout.Space(10);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(280));
            for (int i = 0; i < songPaths.Count; i++)
            {
                string songName = Path.GetFileNameWithoutExtension(songPaths[i]);
                string prefix = (i == currentIndex && isPlaying) ? "▶ " : "   ";
                if (GUILayout.Button(prefix + songName))
                {
                    currentIndex = i;
                    PlayRequest(Path.GetFileNameWithoutExtension(songPaths[i]));
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(5);
            if (GUILayout.Button(isPlaying ? "⏹ Stop" : "▶ Play"))
            {
                if (isPlaying) StopRequest();
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

        [PunRPC]
        void RPC_PlaySong(string songName)
        {
            // Buscar la canción por nombre en la carpeta local de cada cliente
            string foundPath = null;
            foreach (string path in Directory.GetFiles(MusicBoxPlugin.SongsFolder, "*.mp3"))
            {
                if (Path.GetFileNameWithoutExtension(path) == songName)
                {
                    foundPath = path;
                    break;
                }
            }
            if (foundPath == null)
            {
                foreach (string path in Directory.GetFiles(MusicBoxPlugin.SongsFolder, "*.wav"))
                {
                    if (Path.GetFileNameWithoutExtension(path) == songName)
                    {
                        foundPath = path;
                        break;
                    }
                }
            }

            if (foundPath != null)
            {
                StartCoroutine(PlaySong(foundPath));
                MusicBoxPlugin.Log.LogInfo($"RPC: Reproduciendo {songName}");
            }
            else
            {
                MusicBoxPlugin.Log.LogInfo($"RPC: Canción no encontrada localmente: {songName}");
            }
        }

        [PunRPC]
        void RPC_StopSong()
        {
            StopSong();
            MusicBoxPlugin.Log.LogInfo("RPC: Stop recibido");
        }

        IEnumerator PlaySong(string path)
        {
            StopSong();
            AudioType audioType = path.EndsWith(".wav") ? AudioType.WAV : AudioType.MPEG;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, audioType))
            {
                yield return www.SendWebRequest();
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
                isPlaying = true;
            }
        }

        void PlayRequest(string songName)
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("RPC_PlaySong", RpcTarget.All, songName);
            }
            else
            {
                // Singleplayer - reproducir directamente
                RPC_PlaySong(songName);
            }
        }

        void StopRequest()
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("RPC_StopSong", RpcTarget.All);
            }
            else
            {
                RPC_StopSong();
            }
        }

        void StopSong()
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }
}