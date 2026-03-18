# 🎵 MusicBox Mod — R.E.P.O.

A BepInEx mod for R.E.P.O. that spawns a interactive music box object in every level. Players can play local MP3/WAV files from a songs folder, with full multiplayer sync via Photon.

---

## 📦 Features

- Spawns a MusicBox item in every level (not in the lobby)
- Plays local `.mp3` and `.wav` files from a configurable songs folder
- Scrollable GUI menu to browse and select songs (toggle with `M`)
- Multiplayer support — audio synced across all players via Photon RaiseEvent
- The object is physical and can be grabbed/moved like any other item

---

## 🚀 Installation

### Via Thunderstore Mod Manager (recommended)

1. Open Thunderstore Mod Manager and select your R.E.P.O. profile
2. Search for **MusicBox** and install it
3. Drop your `.mp3` / `.wav` files into:
   ```
   BepInEx/plugins/ErMichos-MusicBox/songs/
   ```
4. Launch the game — the MusicBox will appear in every level

### Manual Installation

1. Install [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx) for R.E.P.O.
2. Download the latest `MusicBox.dll` from [Releases](../../releases)
3. Place it in:
   ```
   BepInEx/plugins/ErMichos-MusicBox/MusicBox.dll
   ```
4. Create a `songs` folder next to the DLL and add your music files:
   ```
   BepInEx/plugins/ErMichos-MusicBox/songs/mysong.mp3
   ```
5. Launch the game

---

## 🎮 Usage

| Action | Description |
|--------|-------------|
| `M` key | Toggle the MusicBox menu |
| Click a song | Play that song (synced in multiplayer) |
| `▶ Play` button | Play the currently selected song |
| `⏹ Stop` button | Stop playback |
| `✕ Cerrar` button | Close the menu |

The MusicBox object spawns near the center of the level start room. It can be grabbed and moved around like any other item.

---

## 🛠️ Building from Source

### Requirements

- Visual Studio 2022
- .NET Framework 4.7.2
- R.E.P.O. installed via Steam
- BepInEx 5.4.23.5 installed in your R.E.P.O. profile

### Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/ErMichoss/MusicBox.git
   cd MusicBox
   ```

2. Create a `libs/` folder in the project root and copy these DLLs into it:

   **From your BepInEx installation:**
   ```
   BepInEx.dll
   0Harmony.dll
   ```

   **From `R.E.P.O/REPO_Data/Managed/`:**
   ```
   UnityEngine.dll
   UnityEngine.CoreModule.dll
   UnityEngine.AudioModule.dll
   UnityEngine.IMGUIModule.dll
   UnityEngine.UnityWebRequestModule.dll
   UnityEngine.UnityWebRequestAudioModule.dll
   UnityEngine.PhysicsModule.dll
   UnityEngine.InputLegacyModule.dll
   Assembly-CSharp.dll
   PhotonUnityNetworking.dll
   PhotonRealtime.dll
   ```

3. Open `MusicBox.sln` in Visual Studio 2022

4. Build the solution (`Ctrl+Shift+B`)

5. Copy the output DLL to your BepInEx plugins folder:
   ```
   BepInEx/plugins/MusicBox/MusicBox.dll
   ```

### Project Structure

```
MusicBox/
├── MusicBoxPlugin.cs          # Plugin entry point, Awake, SongsFolder path
├── SpawnPatch.cs              # Harmony patch on LevelGenerator.GenerateDone
├── MusicBoxNetworkSpawner.cs  # IOnEventCallback — listens for Photon event 77
└── MusicBoxItem.cs            # MonoBehaviour — GUI, audio playback, RPCs
```

### How It Works

1. **SpawnPatch** fires after `LevelGenerator.GenerateDone` — if we're in a level:
   - Creates the `MusicBoxNetworkSpawner` if it doesn't exist yet
   - In **multiplayer** (Master Client only): allocates a Photon ViewID, sends event code `77` to all players via `RaiseEvent` with `AddToRoomCache`
   - In **singleplayer**: directly instantiates the object

2. **MusicBoxNetworkSpawner** implements `IOnEventCallback`:
   - Receives event `77` with the viewID and spawn position
   - Clones an existing `ValuableObject` prefab, strips the `ValuableObject` component, assigns the ViewID to its `PhotonView`, registers it with Photon, and adds `MusicBoxItem`

3. **MusicBoxItem** handles:
   - Loading songs from the `songs/` folder on `Start()`
   - Drawing the IMGUI scroll menu
   - Calling `myView.RPC("RPC_Play", RpcTarget.All, songName)` to sync audio
   - Loading and playing audio via `UnityWebRequestMultimedia`

### Adding Features

To add new functionality, the key entry points are:

- **New keybinds / menu buttons** → `MusicBoxItem.OnGUI()` and `MusicBoxItem.Update()`
- **New synced actions** → add a `[PunRPC]` method to `MusicBoxItem` and call it via `myView.RPC(...)`
- **Spawn position** → modify `spawnPos` in `SpawnPatch.Postfix()`
- **Event code** → currently `77`, defined in `SpawnPatch.cs` and `MusicBoxNetworkSpawner.cs`

---

## 📁 Songs Folder Location

| Installation Method | Path |
|---|---|
| Thunderstore Mod Manager | `%AppData%/Thunderstore Mod Manager/DataFolder/REPO/profiles/<profile>/BepInEx/plugins/MusicBox/songs/` |
| Manual | `<REPO game folder>/BepInEx/plugins/MusicBox/songs/` |

Supported formats: `.mp3`, `.wav`

---

## ⚠️ Notes

- The MusicBox only spawns in levels, not in the lobby or shop
- In multiplayer, only the **host (Master Client)** triggers the spawn — clients receive the object automatically via the cached Photon event
- Audio files are read from the **local machine** of each player — all players need the same songs in their `songs/` folder for audio to play correctly on their end
- The object uses a single `PhotonView` shared by both the grab system and the audio sync RPCs

---
