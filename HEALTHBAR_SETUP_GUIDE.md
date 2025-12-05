# Player Health Bar Setup Guide

## Overview
Replace the boring text-only health display with a nice visual health bar that:
- ✅ Changes color based on health (green → yellow → red)
- ✅ Smoothly animates when taking damage
- ✅ Shows current/max health or percentage
- ✅ Automatically connects to player health

---

## Part 1: Create the Health Bar UI

### Step 1: Find Your GameUI Canvas
1. In Hierarchy, find **GameUI** (where your current HealthText is)
2. This is where we'll build the health bar

### Step 2: Create Health Bar Container
1. **Right-click on GameUI** → **UI** → **Panel**
2. **Rename** it to: `HealthBarContainer`
3. **Configure Rect Transform:**
   - **Anchors:** Top-Left
     - Click anchor preset → Hold **Alt** → Click **top-left**
   - **Pivot:** X: `0`, Y: `1`
   - **Pos X:** `20`
   - **Pos Y:** `-20`
   - **Width:** `300`
   - **Height:** `40`

4. **Style the panel (optional background):**
   - **Image Component:**
     - Color: Dark transparent (R: 0.1, G: 0.1, B: 0.1, A: 0.7)
     - Or set Color alpha to 0 for no background

### Step 3: Create Health Bar Background
1. **Right-click HealthBarContainer** → **UI** → **Image**
2. **Rename** to: `HealthBarBackground`
3. **Configure Rect Transform:**
   - **Anchors:** Stretch (bottom-right preset with Alt+Shift)
   - **Offsets:** Left: `10`, Right: `10`, Top: `10`, Bottom: `10`

4. **Style the background:**
   - **Image Component:**
     - Color: Dark gray/red (R: 0.2, G: 0.1, B: 0.1, A: 1)

### Step 4: Create Health Bar Fill (The Important Part!)
1. **Right-click HealthBarBackground** → **UI** → **Image**
2. **Rename** to: `HealthBarFill`
3. **Configure Rect Transform:**
   - **Anchors:** Stretch (bottom-right preset with Alt+Shift)
   - **Offsets:** All `0` (Left: 0, Right: 0, Top: 0, Bottom: 0)

4. **Configure Image Component:**
   - **Color:** Green (R: 0.2, G: 0.8, B: 0.2, A: 1)
   - **Image Type:** `Filled`
   - **Fill Method:** `Horizontal`
   - **Fill Origin:** `Left`
   - **Fill Amount:** `1` (will be controlled by script)

### Step 5: Create Health Text (Optional)
1. **Right-click HealthBarContainer** → **UI** → **Text - TextMeshPro**
2. **Rename** to: `HealthBarText`
3. **Configure Rect Transform:**
   - **Anchors:** Stretch (bottom-right preset with Alt+Shift)
   - **Offsets:** All `0`

4. **Configure TextMeshPro:**
   - **Text:** `100 / 100` (placeholder)
   - **Font Size:** `20`
   - **Alignment:** Center-Middle
   - **Color:** White
   - **Enable Auto Size:** No
   - **Outline:** Enabled, black, thickness 0.2

---

## Part 2: Add the Script

### Step 1: Add Script to HealthBarContainer
1. **Select HealthBarContainer** in Hierarchy
2. **Add Component** → Type `PlayerHealthBar`
3. Click on the script to add it

### Step 2: Configure Script References
With **HealthBarContainer** selected, configure the **PlayerHealthBar (Script)**:

**References:**
- **Player Health:** Drag your **Player** (DeathKnight) from Hierarchy here
- **Health Bar Fill:** Drag **HealthBarFill** from Hierarchy here
- **Health Text:** Drag **HealthBarText** from Hierarchy here (if you created it)

**Visual Settings:**
```
☑ Show Health Text: CHECKED
☐ Show Percentage: UNCHECKED (shows "50/100" instead of "50%")

Full Health Color: R: 0.2, G: 0.8, B: 0.2 (green)
Mid Health Color: R: 0.9, G: 0.7, B: 0.2 (yellow)
Low Health Color: R: 0.9, G: 0.2, B: 0.2 (red)

Low Health Threshold: 0.3 (30% = red)
Mid Health Threshold: 0.6 (60% = yellow)
```

**Animation:**
```
☑ Smooth Transition: CHECKED (smooth animation)
Transition Speed: 5
```

---

## Part 3: Clean Up Old Health Text

### Remove/Hide Old HealthText:
1. Find the old **HealthText** under GameUI
2. Either:
   - **Disable it:** Uncheck the checkbox at top of Inspector, OR
   - **Delete it:** Right-click → Delete

---

## Final Hierarchy

Your GameUI should look like:
```
GameUI
├── HealthBarContainer (Panel + PlayerHealthBar script)
│   ├── HealthBarBackground (dark red/gray image)
│   │   └── HealthBarFill (green filled image - animated!)
│   └── HealthBarText (optional text showing "HP / MaxHP")
├── EnemyCountText
├── InstructionsText
└── ... (other UI elements)
```

---

## Customization Options

### Different Style: Horizontal Bar at Bottom
Change HealthBarContainer position:
```
Anchors: Bottom-Center (hold Alt, click bottom-center)
Pivot: X: 0.5, Y: 0
Pos X: 0
Pos Y: 20
Width: 400
Height: 30
```

### Different Style: Vertical Bar
Change HealthBarFill:
```
Image Type: Filled
Fill Method: Vertical
Fill Origin: Bottom
```

### Show Percentage Instead
In PlayerHealthBar script:
```
☑ Show Percentage: CHECKED
```
Shows "85%" instead of "85/100"

### Different Colors (Pokemon Style)
```
Full Health Color: R: 0.3, G: 0.9, B: 0.3 (bright green)
Mid Health Color: R: 1, G: 0.9, B: 0 (bright yellow)
Low Health Color: R: 1, G: 0.2, B: 0.2 (bright red)
```

### No Smooth Animation (Instant)
```
☐ Smooth Transition: UNCHECKED
```

### Hide Text, Show Only Bar
```
☐ Show Health Text: UNCHECKED
```
Delete or disable HealthBarText GameObject

### Add Border/Outline
1. Right-click HealthBarBackground → UI → Image
2. Name it "Border"
3. Make it same size as HealthBarBackground
4. Set color to white/black
5. Use Outline effect or make it slightly larger

---

## Testing

1. **Press Play**
2. The health bar should show **full green**
3. **Take damage** (let enemies hit you)
4. Watch the bar:
   - Smoothly decrease
   - Turn **yellow** around 60% health
   - Turn **red** around 30% health
5. Text should update showing current/max HP

---

## Troubleshooting

**Bar doesn't move:**
- Check that Player Health is assigned in script
- Make sure HealthBarFill Image Type is set to "Filled"
- Verify Fill Method is "Horizontal"

**Bar is wrong color:**
- Check Full/Mid/Low Health Color values
- Adjust thresholds (0.3 = 30%, 0.6 = 60%)

**Text doesn't update:**
- Make sure Health Bar Text is assigned
- Check "Show Health Text" is enabled
- Verify text object is active

**Bar is in wrong position:**
- Adjust HealthBarContainer Rect Transform
- Check anchors and pivot points

**No smooth animation:**
- Enable "Smooth Transition"
- Increase "Transition Speed" (try 8-10)

---

## Advanced: Multiple Health Bars

To add health bars for enemies, use the existing **EnemyHealthBar.cs** script (you already have this!). Each enemy can have their own health bar above their head.

For the player, this PlayerHealthBar creates a fixed UI element that stays on screen.

---

You now have a professional-looking health bar! 🎮❤️
