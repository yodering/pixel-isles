# Music Setup Guide for Pixel Isles

## Setup Instructions

### Step 1: Create the AudioManager GameObject

1. Open your **LoadingScreen** scene (since this is the first scene that loads)
2. Create an empty GameObject: `Right-click in Hierarchy → Create Empty`
3. Rename it to `AudioManager`
4. Add the **AudioManager** component to it
5. Add the **SceneMusicManager** component to it

### Step 2: Configure AudioManager Component

In the AudioManager Inspector:

#### Audio Mixer Groups
1. Open `Assets/Sounds/Mixer1.mixer` in the Project window
2. Drag the **SFX** group to the `Sfx Mixer Group` field
3. Drag the **Music** group to the `Music Mixer Group` field

#### Music Tracks (Leave empty - SceneMusicManager will handle this)
- You can leave the music section empty since we're using SceneMusicManager

#### SFX Clips (Add later when you have sound effects)
- Drag and drop your sound effect files to the appropriate fields
- You can leave these empty for now if you don't have SFX yet

### Step 3: Configure SceneMusicManager Component

In the SceneMusicManager Inspector:

#### Music Tracks
1. **Loading Music**: Drag `Assets/Sounds/Music/loading.mp3` here
2. **Tutorial Music**: Drag `Assets/Sounds/Music/loading.mp3` here (same as loading)
3. **Default Music**: Drag `Assets/Sounds/Music/default.mp3` here
4. **Scene 1 Music**: Drag `Assets/Sounds/Music/scene-1.mp3` here
5. **Scene 2 Music**: Drag `Assets/Sounds/Music/scene-2.mp3` here

#### Settings
- **Fade Between Tracks**: ✓ (checked)
- **Fade Duration**: 1.5 seconds (default)
- **Play On Awake**: ✓ (checked)

### Step 4: Audio Import Settings

For each music file in `Assets/Sounds/Music/`:

1. Select the music file in Project window
2. In the Inspector, configure:
   - **Load Type**: Streaming (for music files)
   - **Compression Format**: Vorbis (good balance of quality/size)
   - **Quality**: 70-100 (higher = better quality, larger file)
   - Click **Apply**

### Step 5: Test the Music System

1. Press Play in the Unity Editor
2. Music should automatically start playing for the current scene
3. When you transition to different scenes, music should fade and change automatically

## Music Mapping

The system automatically plays the correct music for each scene:

| Scene Name | Music File |
|------------|------------|
| LoadingScreen | loading.mp3 |
| tutorial | loading.mp3 |
| default | default.mp3 |
| ice (scene-1) | scene-1.mp3 |
| autumn (scene-2) | scene-2.mp3 |

## Troubleshooting

### No music is playing
- Check that AudioManager GameObject has both AudioManager and SceneMusicManager components
- Verify that music clips are assigned in SceneMusicManager Inspector
- Check that Audio Mixer groups are assigned
- Make sure "Play On Awake" is checked in SceneMusicManager

### Music doesn't change between scenes
- Verify that AudioManager is marked as DontDestroyOnLoad (it does this automatically)
- Check the Console for any error messages
- Make sure SceneMusicManager is on the same GameObject as AudioManager

### Music is too loud/quiet
- Adjust volume in the Audio Mixer (Assets/Sounds/Mixer1.mixer)
- Open the mixer, select the Music group, adjust the volume slider

## Future: Adding Sound Effects

When you're ready to add sound effects:

1. Place SFX files in `Assets/Sounds/SFX/` folder
2. Select each file and set Import Settings:
   - **Load Type**: Decompress On Load (for short SFX) or Compressed In Memory
   - **Compression Format**: PCM for short sounds, Vorbis for longer sounds
3. Assign clips to AudioManager's SFX fields in Inspector
4. Add sound effect calls in your scripts:

```csharp
// Example: Play attack sound
if (AudioManager.Instance != null)
    AudioManager.Instance.PlayPlayerAttack(isRanged: true);
```

Refer to the main README for more details on integrating sound effects into gameplay scripts.
