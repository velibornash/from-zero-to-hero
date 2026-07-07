# From Zero to Hero — Game & Technical Guide

---

## Part 1: User's Guide — Chapter I: The Awakening

### Story

You are a Serbian knight, the last hope of a small valley village. Wolves howl in the forests, barbarians gather beyond the hills, and your people look to you. Build the town, defend the realm, and forge a hero from nothing.

### What's Done in Chapter 1

#### The World
- 3D valley terrain with grass ground, pine forests, a lake, rocks, and flowers
- Hero (Knight model, scale 2.2x) walks the terrain with a third-person camera
- Orthographic camera follows the hero; right-click drag to orbit, scroll to zoom, touch-drag on mobile
- Day/night cycle ready, background fog hides terrain edges

#### Building Chain (Sequential Unlocks)

| Step | Building | Cost | Effect |
|------|----------|------|--------|
| 1 | **Church** | 10 gold | Unlocks the Flag tile |
| 2 | **Serbian Flag** | 10 gold | Unlocks all 4 Watchtower tiles |
| 3-6 | **Watchtowers** (x4) | 10g each | When all 4 built, unlocks 4 Mage tiles |
| 7-10 | **Mage Towers** (x4) | 10g each | Adds a powerful mage defender to help the hero |

Each building is placed on a designated tile in the world. Walk up to a tile, press Tab to open the build menu, and spend gold to construct. A glowing building icon appears on the tile once built.

#### Hero
- Third-person movement: WASD / arrow keys to walk, or touch drag on mobile
- Health bar (top right of the screen): starts at 100/100 HP
- Dies when health reaches 0 (game over, with restart option)
- Currently the hero explores freely; enemies arrive in future chapters

#### Economy
- 4 resources tracked in the top ribbon:
  - **Gold** (yellow) — earned by mining, quests, or event triggers. Win condition: 300 gold
  - **Wood** (brown) — earned by logging; used for future upgrades
  - **Stone** (grey) — earned by quarrying; used for future upgrades
  - **Wheat/Food** (golden) — earned by farming; feeds the village
- Resources display in the top ribbon bar with icons and counters
- Ribbon bar has gold trim and dark red background (Travian-style)

#### Building Placement
- 10 building slots arranged in a cross pattern on the terrain
- Each slot has a cost label, building icon, and gold coin image
- Walk-snapping: when the hero is near a slot, the camera frames it and the slot activates
- Press Tab to open the ActionMenu and build
- Popup messages confirm construction

#### UI Layout
- **Top ribbon**: spans full screen width, 140px tall with stretched background image. Contains:
  - Resource icons + counters (gold coin, wood log, stone block, wheat sheaf)
  - Vertical gold separator
  - Chapter title: "Chapter I: The Awakening"
  - Health bar with green-to-red fill and HP counter
- **Reports panel**: top-right, shows event log (welcome message, building completions, story beats)
- **Building popups**: ornate dark-brown parchment panels with gold text, appear when interacting
- **ActionMenu**: tab-triggered build menu with parchment styling
- **Minimap**: top-right corner, shows hero position and building locations

#### Controls

| Input | Action |
|-------|--------|
| WASD / Arrow keys | Move hero |
| Right-click + drag | Orbit camera |
| Scroll wheel | Zoom in/out |
| Tab | Open build menu (ActionMenu) |
| One-finger touch drag (mobile) | Orbit camera |
| Two-finger pinch (mobile) | Zoom in/out |

#### Win Condition
Collect 300 gold and build all 10 buildings (Church + Flag + 4 Towers + 4 Mages) to win Chapter I. A victory popup congratulates you and teases Chapter II.

---

## Part 2: Future Plans

### Chapter II: The Gathering Storm (Planned)
- Waves of barbarian raiders and wolf packs attack the village
- Hero engages in real-time combat (sword slashes, blocking)
- NPC defenders (archers, spearmen) help protect buildings
- City wall construction around the village perimeter
- Economy upgrades: mines, farms, lumber mills produce resources automatically

### Chapter III: Kingdom Rising (Planned)
- Multiple villages connected by roads
- Trade routes with neighboring lords
- Castle construction with customizable keeps and towers
- Hero mounts a horse for faster travel
- Siege warfare against barbarian strongholds

### Chapter IV: The Crusade (Planned)
- Hero leads an army beyond the valley
- Conquer enemy territory tile-by-tile
- Ally NPC heroes with unique abilities
- Epic boss battles (giant warlords, mythical creatures)

### City Development Features (Long-term)
- **Zoning**: designate residential, military, and industrial districts
- **Upgrades**: each building can level up (Lv 1→5) with better visuals and output
- **Citizens**: population system with housing, happiness, and tax revenue
- **Defenses**: walls, gates, moats, watchtower archers
- **Missions**: daily quests from town elders, reward gold and items
- **Technology tree**: unlock upgrades through a medieval tech tree (blacksmith → armor, mill → better bread, etc.)

### Planned UI Improvements
- HQ popup panel with hero stats and level
- Building detail panel showing output, upgrades, and workers
- Quest log sidebar with active and completed missions
- Pause menu with settings, save/load, and credits

---

## Part 3: Technical Documentation

### Technologies Used

| Layer | Technology |
|-------|------------|
| Engine | Unity 6000.5.0f1 |
| Rendering | Built-in 3D pipeline (Forward) |
| UI | Unity Canvas (Screen Space — Overlay), raw Text + Image components |
| Input | Unity legacy Input system (`Input.GetAxis`, `Input.GetMouseButton`, `Input.touchCount`) |
| Scripting | C# .NET (MonoBehaviour-based) |
| Assets | EmaceArt, Environment Starter Pack, InfinityPBR, Polytope Studio, MyDreamGameStudio, 3D |

### Project Structure

```
Assets/
├── Editor/
│   ├── BuildScript.cs            # CLI build methods (BuildMac, BuildWebGL)
│   └── Setup3DScene.cs           # Scene bootstrapper (generates terrain, props, slots)
├── Resources/
│   ├── HUDIcons/                 # Ribbon background + resource icons (PNG, custom)
│   ├── BuildingIcons/            # 17 building icons, chroma-key transparency
│   └── Tiles/                    # Tile slot textures (784x784, square PNG)
├── Scenes/
│   └── nova scena.unity          # Main playable scene (hand-built)
├── Scripts/
│   ├── ActionMenu.cs             # Tab-triggered build popup
│   ├── Billboard.cs              # UI element always facing camera
│   ├── BuildSlot.cs              # Individual tile slot (icon, cost, build logic)
│   ├── BuildZone.cs              # Manages all tile slots + walk-snapping
│   ├── BuildingPopup.cs          # Popup notification when buildings complete
│   ├── CameraFollow3D.cs         # Third-person camera with orbit/zoom/touch
│   ├── Combat.cs                 # Damage system (enemy attack data)
│   ├── FloatingText.cs           # Animated damage numbers / floating labels
│   ├── HUDController.cs          # Top ribbon, resources, health bar, events
│   ├── HUDInfoPanel.cs           # Info overlays on game objects
│   ├── MinimapController.cs      # Corner minimap rendering
│   ├── NPCPatrol.cs              # NPC wandering behavior
│   ├── PlayerController3D.cs     # Hero movement (WASD / navmesh / touch)
│   ├── PopupBase.cs              # Base class for ornate dark-brown popups
│   ├── SlotActivator3D.cs        # Proximity slot activation + camera framing
│   └── UIStyleHelper.cs          # UI factory: ornate icons, panels, separators
├── Models/                       # Player, NPCs, buildings (FBX/prefab)
├── Materials/                    # Terrain, building, and prop materials
├── Plugins/                      # Third-party DLLs / native plugins
└── Resources.meta
```

### Main Modules Explained

#### 1. Player Controller (`PlayerController3D.cs`)
- Handles WASD/keyboard movement and mobile touch joystick
- Uses `CharacterController` for physics-based movement
- NavMeshAgent-based pathfinding for click-to-move
- Health system (`Health`/`maxHealth` static fields)
- Death callback triggers game over popup
- References `BuildZone` for walk-snapping near buildings

#### 2. Camera System (`CameraFollow3D.cs`)
- Smoothly follows the hero with configurable distance and angle
- Orbital controls: right-click drag to rotate (yaw/pitch), scroll to zoom
- Mobile support: one-finger drag orbits, two-finger pinch zooms
- Pitch clamped 15°–80°, distance clamped 15–55
- Fog enabled (linear, start 30, end 55, dark green) to hide terrain edges when zoomed out
- Uses `Quaternion.Euler(pitch, yaw, 0)` to compute camera direction vector

#### 3. Building System (`BuildSlot.cs` + `BuildZone.cs`)
- 10 tile slots arranged in a cross pattern on the terrain
- Each slot has a `slotIndex`, `cost`, `isUnlocked`, `IsBuilt` state
- Slots are physical game objects with a Canvas overlay:
  - Building icon image (104×104, centered above slot)
  - Cost text label (gold amount)
  - Coin icon (from tile texture)
- `BuildZone` manages the array of slots and provides lookups by position
- Building flow: hero walks near → slot highlights → press Tab → ActionMenu shows → select to build

#### 4. HUD (`HUDController.cs`)
- Singleton instance, builds UI elements in `BuildHUD()`
- Top ribbon: full-width bar with resource icons, separator, chapter title, health bar
- Resources update every frame from static fields (`Gold`, `Wood`, `Stone`, `Food`)
- Events panel: shows recent game events (auto-fades after 8 seconds)
- Win condition check: 300 gold + all buildings → victory popup
- `BuildResourceSlot()`: creates icon + text per resource, returns the text component for live updates
- The ribbon background is a cropped PNG (1168×329, stretched to fill 1920×140 display rect) — cropped to remove white padding, only the decorative gold/red pattern remains

#### 5. Popups (`PopupBase.cs`, `BuildingPopup.cs`, `ActionMenu.cs`)
- `PopupBase<T>`: generic popup with dark-brown background, gold trim, close button, title + body
- `BuildingPopup`: static `Show()` method for notifications (building complete, victory, game over)
- `ActionMenu`: static singleton with `Toggle()`, shows build options when Tab is pressed
- Both use `UIStyleHelper` for consistent styling

#### 6. Tile Textures (Resources/Tiles/)
- Original JPG tiles converted to PNG with chroma-key transparency (white→transparent)
- Square cropped (784×784) to prevent stretching on the slot canvas
- Each tile has the building visual plus a coin icon (gold cost indicator)
- `BuildSlot.cs` creates an `Image` on the slot's Canvas for the tile + another Image for the building icon

#### 7. Asset Pipeline
- All original JPGs from user's Downloads folder converted to PNG with chroma-key transparency
- Icons: white backgrounds removed, filter mode set to Bilinear
- Ribbon background: white padding cropped out, only the visible decorative pattern remains
- Files stored in `Assets/Resources/` for runtime `Resources.Load()` access
- No Sprite Atlases — each image loaded individually at startup

#### 8. Scene Bootstrapping (`Setup3DScene.cs`)
- `CreateScene()`: generates ground plane, trees, lake, rocks, flowers
- `CreatPlayer()`: places hero at (0, 0, -55) with CharacterController
- `SetupCamera()`: configures camera rotation and parent
- `CreateSlotGameObject()`: creates each tile with mesh, collider, and canvas overlay
- All 10 building tile positions hardcoded in world space

### Building and Running

```bash
# Open in Unity Editor
open -a "/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app" /path/to/project

# Build for macOS from CLI
/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode -projectPath /path/to/project \
  -logfile /tmp/build.log -executeMethod BuildScript.BuildMac

# Build for WebGL
/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode -projectPath /path/to/project \
  -logfile /tmp/build.log -executeMethod BuildScript.BuildWebGL
```

### Key Design Decisions

- **No TextMeshPro:** using legacy `UnityEngine.UI.Text` with `LegacyRuntime.ttf` for simplicity and mobile performance
- **Static resource fields:** resources stored as static ints on HUDController, updated by events — simpler than a full MVC for current scope
- **Canvas Overlay:** all UI in Screen Space — Overlay mode, no world-space canvas (except building slot canvases which are world-space)
- **No addressables:** all assets loaded via `Resources.Load()` at startup — fine for current scope, should migrate to Addressables for larger content
- **Fog instead of skybox:** linear fog set to dark green to obscure terrain edges when camera is zoomed out, avoiding the need for a skybox or horizon mesh

---
