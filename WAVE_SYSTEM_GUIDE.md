# Wave Spawner System - Quick Guide

## Overview
The WaveSpawner system spawns enemies in configurable waves. When all enemies in a wave are defeated, the next wave automatically starts after a delay. Perfect for creating escalating difficulty and varied enemy encounters.

## Features
- Multiple waves with custom configurations
- Mix different enemy types in each wave
- Control spawn timing and intervals
- Automatic wave progression
- Optional endless mode (loop waves)
- Event system for UI integration

## Setup in Unity

### 1. Create Wave Spawner GameObject
1. Create an empty GameObject in your scene
2. Name it "WaveSpawner"
3. Add the `WaveSpawner` component to it

### 2. Configure Spawn Areas
- The system auto-finds spawn areas (GameObjects with "spawn" in their name)
- Make sure these have `BoxCollider2D` components set as triggers
- Or manually assign spawn areas in the Inspector

### 3. Create Waves

#### Step-by-step:
1. In Inspector, find **"Wave Configuration"** section
2. Click the **+ button** next to "Waves" to add a wave
3. Expand the new wave element (click the arrow)
4. Set the **Wave Name** (e.g., "Wave 1", "Easy Start", etc.)
5. Click the **+ button** next to "Enemies" to add an enemy type
6. Expand the enemy entry and configure:
   - **Enemy Prefab**: Drag from `Assets/Prefabs/Enemies/` (Archer, Knight, Mage, etc.)
   - **Count**: Number of this enemy to spawn (e.g., 3)
   - **Spawn Interval**: Seconds between spawning each one (e.g., 0.5)
7. Add more enemy types by repeating steps 5-6
8. Set **Delay Before Wave**: Seconds to wait after previous wave clears (e.g., 3.0)
9. Repeat steps 2-8 to create more waves

## Example Wave Configuration

### Wave 1 - "Easy Start"
- 3x Archers (spawn interval: 0.5s)
- 2x Knights (spawn interval: 1.0s)
- Delay: 3 seconds

### Wave 2 - "Magic Attack"
- 2x Wizards (spawn interval: 0.5s)
- 2x Mages (spawn interval: 0.5s)
- 1x Paladin (spawn interval: 1.0s)
- Delay: 5 seconds

### Wave 3 - "All Out Assault"
- 2x CamoArchers (spawn interval: 0.3s)
- 3x Knights (spawn interval: 0.5s)
- 2x Wizards (spawn interval: 0.5s)
- 1x Paladin (spawn interval: 1.0s)
- Delay: 5 seconds

## Available Enemy Types
Located in `Assets/Prefabs/Enemies/`:
- Archer
- CamoArcher
- Knight
- Mage
- Paladin
- Wizard

## Settings Explained

### Auto Start First Wave
- **Checked**: Wave 1 starts automatically when scene loads
- **Unchecked**: You must call `StartNextWave()` manually via code or event

### Loop Waves
- **Checked**: After final wave, restart from Wave 1 (endless mode)
- **Unchecked**: Stops after final wave and fires `OnAllWavesComplete` event

### Show Debug Logs
- **Checked**: Prints wave info to console + Press N to skip to next wave
- **Unchecked**: Silent operation

## Events (for UI)
You can hook up these events to update your UI:

- **OnWaveStart** - Fires when wave begins (passes wave number)
- **OnWaveComplete** - Fires when all enemies defeated (passes wave number)
- **OnAllWavesComplete** - Fires when all waves finished
- **OnEnemyCountChanged** - Fires when enemy count updates (passes current/total)

## Tips

### Difficulty Progression
- Start with fewer, weaker enemies (Archers, Knights)
- Gradually increase enemy counts
- Introduce tougher enemies (Wizards, Paladins) in later waves
- Mix enemy types to create interesting combat scenarios

### Spawn Timing
- Use shorter intervals (0.3-0.5s) for fast-paced action
- Use longer intervals (1-2s) to give players breathing room
- Vary intervals between enemy types in same wave

### Wave Delays
- Give players 3-5 seconds between waves to prepare
- Increase delay time after harder waves
- Use delay for UI messages ("Wave 2 incoming!")

## Scripting Reference

### Public Methods
```csharp
StartNextWave()        // Manually start next wave
RestartWaves()         // Reset and start from wave 1
GetCurrentWaveNumber() // Returns current wave (1-indexed)
GetTotalWaves()        // Returns total number of waves
GetActiveEnemyCount()  // Returns number of living enemies
IsSpawningWave()       // Returns true if currently spawning
```

### Example: Start Wave on Button Press
```csharp
public WaveSpawner waveSpawner;

public void OnStartButtonClicked()
{
    waveSpawner.StartNextWave();
}
```

## Troubleshooting

### Enemies not spawning?
- Check spawn areas exist and have BoxCollider2D (trigger enabled)
- Verify enemy prefabs are assigned in wave configuration
- Check console for debug logs (enable Show Debug Logs)

### Wave not progressing?
- Make sure enemies have the "Enemy" tag
- Verify enemies are actually being destroyed (not just disabled)
- Check that enemy count reaches 0

### Enemies spawning in wrong location?
- Verify spawn area GameObjects have "spawn" in their name
- Check BoxCollider2D bounds are positioned correctly
- Enable Gizmos in Scene view to see spawn areas (cyan wireframe boxes)
