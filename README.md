# Pixel Isles

A 2D action game built with Unity featuring wave-based combat, multiple character classes, and an immersive story experience.

![Game Trailer](trailer.gif)

## Features

- **Wave-Based Combat**: Fight through multiple waves of enemies across different environments
- **Multiple Character Classes**: Play as Knight, Archer, Wizard, Paladin, and more
- **Ability System**: Unlock and use special abilities (projectiles, AOE attacks)
- **Story Mode**: Experience narrative-driven dialogue scenes and story images
- **A* Pathfinding**: Intelligent enemy AI with grid-based pathfinding
- **Vampirism Mechanic**: Life steal system for sustained combat

## Controls

- **Movement**: WASD
- **Aim**: Mouse
- **Attack**: E (Sword)
- **Projectile**: Q (unlocks after 2 hits)
- **AOE**: F (unlocks after 5 kills)
- **Crouch**: C
- **Skip Dialogue**: SPACE or Click

## Requirements

- Unity 6000.2.7f2 or later
- Universal Render Pipeline (URP)

## Project Structure

```
Assets/
├── Scripts/          # Game logic and controllers
├── Scenes/           # Unity scenes (gameplay, dialogue, story)
├── Prefabs/          # Enemy and projectile prefabs
├── Backgrounds/      # Environment and story images
├── Animations/       # Character animations
└── Sounds/           # Music and SFX
```

## Scene Flow

1. **Prologue** → Story Image → Dialogue
2. **Dungeon** → Wave Combat → Dialogue
3. **Ice** → Wave Combat → Dialogue  
4. **Green** → Wave Combat → Dialogue
5. **Final Reveal** → Story Image → Dialogue

## Building

1. Open project in Unity
2. Go to File → Build Settings
3. Select target platform
4. Build

## License

All rights reserved.

