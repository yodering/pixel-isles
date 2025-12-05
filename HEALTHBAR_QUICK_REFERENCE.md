# Health Bar Quick Reference

## TL;DR - Fast Setup

### 1. Create UI Structure
```
GameUI
└── HealthBarContainer (Panel, 300x40, top-left corner)
    ├── HealthBarBackground (Image, stretched with 10px padding, dark color)
    │   └── HealthBarFill (Image, FILLED TYPE, Horizontal, green)
    └── HealthBarText (TextMeshPro, centered, white)
```

### 2. Add Script
- Select **HealthBarContainer**
- Add Component → **PlayerHealthBar**

### 3. Assign References
- Player Health → **DeathKnight**
- Health Bar Fill → **HealthBarFill**
- Health Text → **HealthBarText**

### 4. Done!
Press Play and watch the health bar in action.

---

## Copy-Paste Settings

### HealthBarContainer (Panel)
```
Rect Transform:
  Anchors: Top-Left (0, 1) to (0, 1)
  Pivot: (0, 1)
  Pos X: 20
  Pos Y: -20
  Width: 300
  Height: 40

Image:
  Color: (0.1, 0.1, 0.1, 0.7) or Alpha: 0 for transparent
```

### HealthBarBackground (Image)
```
Rect Transform:
  Anchors: Stretch (0, 0) to (1, 1)
  Left: 10, Right: 10, Top: 10, Bottom: 10

Image:
  Color: (0.2, 0.1, 0.1, 1) - Dark red
  Type: Simple
```

### HealthBarFill (Image) ⭐ IMPORTANT
```
Rect Transform:
  Anchors: Stretch (0, 0) to (1, 1)
  Left: 0, Right: 0, Top: 0, Bottom: 0

Image:
  Color: (0.2, 0.8, 0.2, 1) - Green (will change automatically)
  Type: Filled ← MUST BE FILLED!
  Fill Method: Horizontal
  Fill Origin: Left
  Fill Amount: 1
```

### HealthBarText (TextMeshPro)
```
Rect Transform:
  Anchors: Stretch (0, 0) to (1, 1)
  Left: 0, Right: 0, Top: 0, Bottom: 0

TextMeshPro:
  Text: "100 / 100"
  Font Size: 20
  Alignment: Center
  Color: White (1, 1, 1, 1)
  Outline: Enabled, Black, 0.2
```

### PlayerHealthBar Script
```
References:
  Player Health: [DeathKnight GameObject]
  Health Bar Fill: [HealthBarFill Image]
  Health Text: [HealthBarText TMP]

Visual Settings:
  ☑ Show Health Text
  ☐ Show Percentage
  Full Health Color: (0.2, 0.8, 0.2)
  Mid Health Color: (0.9, 0.7, 0.2)
  Low Health Color: (0.9, 0.2, 0.2)
  Low Health Threshold: 0.3
  Mid Health Threshold: 0.6

Animation:
  ☑ Smooth Transition
  Transition Speed: 5
```

---

## Visual Examples

### Health States

**100% Health (Green):**
```
█████████████████████ 100/100
```

**60% Health (Yellow):**
```
████████████░░░░░░░░░ 60/100
```

**30% Health (Red):**
```
██████░░░░░░░░░░░░░░░ 30/100
```

---

## Alternative Layouts

### Bottom-Center (Pokemon Style)
```
HealthBarContainer:
  Anchors: Bottom-Center
  Pivot: (0.5, 0)
  Pos X: 0
  Pos Y: 80
  Width: 400
  Height: 30
```

### Top-Right Corner
```
HealthBarContainer:
  Anchors: Top-Right
  Pivot: (1, 1)
  Pos X: -20
  Pos Y: -20
  Width: 300
  Height: 40
```

### Larger Bar
```
Width: 400
Height: 50
Font Size: 24
```

---

## Common Issues - One-Line Fixes

| Problem | Fix |
|---------|-----|
| Bar doesn't fill | Set Image Type to **Filled** |
| Bar fills wrong direction | Set Fill Origin to **Left** |
| Bar doesn't change color | Check color values in script |
| Text doesn't show | Assign Health Bar Text in script |
| Bar doesn't update | Assign Player Health in script |
| Bar not smooth | Enable Smooth Transition, increase speed |

---

Ready to build! Takes about 5 minutes. 🎮
