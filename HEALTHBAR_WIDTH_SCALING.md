# Health Bar Width Scaling

## New Feature: Bar Shrinks in Size!

The PlayerHealthBar script now supports **three different visual modes** for showing health:

---

## Scale Modes

### 1. Fill Amount (Default)
**Traditional filled bar** - The bar stays the same size, but fills/empties from left to right.

```
100% HP: █████████████████████
 50% HP: ██████████░░░░░░░░░░░
  0% HP: ░░░░░░░░░░░░░░░░░░░░░
```

**Use when:** You want a classic health bar look

---

### 2. Width ⭐ (What you want!)
**Bar physically shrinks** - The entire bar gets smaller/larger based on health.

```
100% HP: █████████████████████
 50% HP: ██████████
  0% HP: (bar disappears)
```

**Use when:** You want a more dramatic visual effect

---

### 3. Both
**Combines both effects** - Bar shrinks AND empties at the same time.

```
100% HP: █████████████████████ (full size, filled)
 50% HP: ██████████░░░░ (half size, half filled)
  0% HP: (tiny, empty)
```

**Use when:** You want maximum visual feedback

---

## How to Enable Width Scaling

### In Unity Inspector:

1. **Select HealthBarContainer** (the GameObject with PlayerHealthBar script)
2. Find the **Bar Style** section
3. Change **Scale Mode** dropdown:
   - `Fill Amount` = Traditional (default)
   - `Width` = Bar shrinks ⭐
   - `Both` = Shrinks AND empties

### Important Setup for Width Mode:

For **Width** or **Both** modes to work, you need to set the anchor properly:

1. **Select HealthBarFill** in Hierarchy
2. In **Rect Transform**, click the **Anchor Preset**
3. Choose **Top-Left** (NOT stretch!)
   - Anchors should be: Min (0, 1), Max (0, 1)
4. Set **Pivot** to: X: `0`, Y: `0.5`

This makes the bar scale from the left edge instead of the center.

---

## Setup Comparison

### For Fill Amount Mode:
```
HealthBarFill Rect Transform:
  Anchors: Stretch (Min: 0,0 Max: 1,1)
  Offsets: All 0

HealthBarFill Image:
  Type: Filled
  Fill Method: Horizontal
```

### For Width Mode:
```
HealthBarFill Rect Transform:
  Anchors: Left-Center (Min: 0,0.5 Max: 0,0.5)
  Pivot: (0, 0.5)
  Pos X: 0
  Pos Y: 0
  Width: (same as parent width)
  Height: (same as parent height)

HealthBarFill Image:
  Type: Simple (NOT Filled!)
  Color: Green
```

### For Both Mode:
```
HealthBarFill Rect Transform:
  Anchors: Left-Center (Min: 0,0.5 Max: 0,0.5)
  Pivot: (0, 0.5)
  Pos X: 0
  Pos Y: 0
  Width: (same as parent width)
  Height: (same as parent height)

HealthBarFill Image:
  Type: Filled
  Fill Method: Horizontal
```

---

## Quick Config for Width Scaling

If you want the **bar to shrink** (Width mode):

### 1. Update HealthBarFill Setup:

**Delete the current HealthBarFill and recreate it:**

1. **Right-click HealthBarBackground** → UI → Image
2. **Rename** to: `HealthBarFill`
3. **Rect Transform:**
   - Click anchor preset → **Middle-Left**
   - Pivot: `(0, 0.5)`
   - Pos X: `0`
   - Pos Y: `0`
   - Width: `280` (match parent width minus padding)
   - Height: `20` (match parent height minus padding)
4. **Image:**
   - Color: Green (0.2, 0.8, 0.2, 1)
   - Type: `Simple` (NOT Filled!)

### 2. Set Script to Width Mode:

In **PlayerHealthBar** script:
- **Scale Mode:** `Width`

### 3. Test:
Press Play and take damage - the bar should physically shrink!

---

## Visual Examples

### Width Mode at Different Health Levels:

**100% Health:**
```
█████████████████████████████ 100/100
```

**75% Health:**
```
█████████████████████ 75/100
```

**50% Health:**
```
██████████████ 50/100
```

**25% Health:**
```
███████ 25/100
```

**0% Health:**
```
 0/100 (bar invisible)
```

---

## Recommendations

**For dramatic effect:** Use `Width` mode
- Clear visual feedback
- Works great with color changes
- Easy to see at a glance

**For traditional RPG feel:** Use `Fill Amount` mode
- Classic health bar look
- Familiar to players

**For maximum impact:** Use `Both` mode
- Very dramatic
- Double visual feedback
- Best for intense action games

---

## Troubleshooting

**Bar scales from center instead of left:**
- Set Pivot to (0, 0.5)
- Set Anchors to Left-Center

**Bar doesn't scale at all:**
- Make sure Scale Mode is set to "Width" or "Both"
- Check that HealthBarFill has a RectTransform

**Bar scales weird/stretchy:**
- Set Image Type to "Simple" (not Filled) for Width mode
- Check anchor settings

**Bar disappears:**
- At 0% health, the bar scale becomes 0
- This is normal behavior for Width mode
- Text should still show "0/100"

---

Try **Width** mode for the shrinking effect you want! 🎮
