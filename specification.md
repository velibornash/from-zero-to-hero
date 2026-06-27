Evo cele specifikacije u jednom bloku. Copy-paste u `specification.md`:

```markdown
# From Zero to Hero — Chapter I: The Awakening
# Game Specification

## 1. Concept
A modern 2026 iPhone medieval hero-driven city builder. The player controls a Knight (Serbian hero) who defends a small village from waves of barbarians and wolves. The game has Travian-style UI with ornate gold-bordered parchment panels, popup notifications when buildings complete, and a top-down 3D world view.

## 2. Initial State

When the game starts:
- Hero (Knight, scale 2.2x) at world position (0, 0, -55), facing north
- Church tile visible at (0, 0, 0), UNLOCKED, cost 10 gold
- Flag tile at (-2, 0, -48), LOCKED
- 4 tower tiles at corners, LOCKED
- 4 mage tile positions, LOCKED
- 10 starting gold
- 100/100 hero health
- No fences, no enemies

## 3. Camera Setup
- Orthographic camera
- Y: 28, Z: -30
- Pitch: 35° down
- Ortho size: 30
- Background color: grass green (0.25, 0.5, 0.18)
- CameraFollow3D component that follows the hero
- Pitch: 35°, yaw: 0°

## 4. World Setup
- Ground: 1600 × 1200 × 1600 cube, position (0, -600, 0) — top face at y=0
- Ground material: grass texture (T_Landscape_Grass.png), tiled 200x200
- Trees: 80+ pine trees in the western forest
- Lake on east side (decorative)
- Rocks and flowers scattered

## 5. Building Chain (Sequential Unlocks)

| Step | Build | Cost | Unlocks |
|------|-------|------|---------|
| 1 | Church | 10g | Flag tile |
| 2 | Flag (Serbian tricolor) | 10g | All 4 corner towers |
| 3-6 | Tower x4 (any order) | 10g each | After all 4 built: all 4 mage tiles |
| 7-10 | Mage Tile x4 | 10g each | Adds powerful mage defender |

## 6. Tile Positions (World Space)

### Church
- Position: (0, 0, 0)
- Model: Polytope building_church_green.fbx
- Scale: 8x
- Worker NPC patrols around it

### Flag
- Position: (-2, 0, -48)
- Model: procedural (pole 10 units, flag 4.5x3, Serbian tricolor with coat of arms)
- No worker

### Corner Towers
- SW: (-70, 0, -50) — index 2
- SE: (70, 0, -50) — index 3
- NE: (70, 0, 50) — index 4
- NW: (-70, 0, 50) — index 5
- Model: Polytope building_tower_A_green.fbx
- Scale: (4, 7.5, 4), rotation (0, 45, 0)
- AddComponent<TowerShooter> on build

### Mage Tiles (15 units inward from each tower)
- Mage 1: (-55, 0, -35) — SW — index 6
- Mage 2: (55, 0, -35) — SE — index 7
- Mage 3: (55, 0, 35) — NE — index 8
- Mage 4: (-55, 0, 35) — NW — index 9
- Spawns KayKit Mage.fbx (scale 2.0) on build
- Has its own TowerShooter with purple projectile

## 7. Build Mechanic
- Hero stands on tile → auto-build starts
- 1 gold deducted every 0.12s until cost paid
- Click tile → instant finish (pays remaining gold)
- If gold runs out → build pauses (tile shows remaining cost)
- SmokePuff on complete (30 gray puffs, 2.5s expand+fade)
- BuildingPopup shows near completed building (TAB/ESC/X to close)
- Reports panel event: "X built!"

## 8. Fence System
- Only builds between ADJACENT corner towers
- South fence: SW + SE (with gate at center, gap 4m each side)
- East fence: SE + NE
- North fence: NE + NW
- West fence: NW + SW
- Each fence segment: 1.4 × 2.5 × 1.0 box collider (non-trigger, blocks everything)
- Fence spacing: 1 unit between segments

### Gate (South fence only)
- 2 wooden posts (offset ±4 from center)
- Gate bar: 8m wide, 0.3m thick
- When hero within 6m: gate bar rises to y=5 (open)
- When hero farther: gate bar lowers to y=0.5 (closed)
- Gate collider activates when closed, deactivates when open
- Hero can pass through open gate
- Enemies blocked by closed gate

## 9. Enemies

### Wolf
- HP: 2, gold: 5, speed: 6.5, scale: 3.5x
- Spawns at 4 western forest points
- Gray color, smaller

### Barbarian
- HP: 4, gold: 8, speed: 5.5, scale: 2.8x
- Brown color, larger

### Spawn
- Starts when Church is built
- Wave every 6s, 3 per wave, max 12 concurrent
- 4 spawn points: (-90,0,-60), (-105,0,0), (-90,0,60), (-80,0,80)
- y=0.5 spawn height

### AI Behavior
- Rigidbody-based movement (linearVelocity)
- Walks toward hero
- Stops at 1.5m, attacks
- Knockback when hit (0.25s stun)
- Death: smoke + burst + +gold text

## 10. Tower Combat

### TowerShooter component
- Range: 30
- Fire rate: 0.7s
- Damage: 1
- Projectile speed: 25
- Projectile color: yellow (0.9, 0.8, 0.3)
- Rotates to face closest enemy

### Projectile
- Spawns 12 units above tower
- Sphere with trigger collider + kinematic Rigidbody
- Trail puffs every 0.04s (small smoke spheres, 0.35s lifetime)
- Muzzle flash: 0.3 scale, 0.6 alpha, 0.25s
- Light component for glow

## 11. Mage Combat

### Mage TowerShooter (different values)
- Range: 35
- Fire rate: 0.45s
- Damage: 2
- Projectile speed: 30
- Projectile color: purple (0.6, 0.3, 1.0)

## 12. Hero System

### Movement
- WASD on PC
- On-screen joystick on mobile (bottom-left, auto-hide on PC)
- Speed: 10
- CharacterController component
- Walk bob procedural (sin wave)

### Combat
- Auto-attack via Physics.OverlapSphere, radius 2.2m
- 1 damage per hit, 0.35s rate
- SimpleWeapon.Swing() animation

### Health System
- Max HP: 100 (static int maxHealth)
- Public static int Health (so HUD can read)
- Public static bool IsDead
- HeroDamage per hit: 15
- Enemy attackInterval: 0.8s
- Regen rate: 10 HP/s
- Regen delay: 2s after last hit
- Village radius: 70m (only regen in village)
- TakeDamage(int) public method
- Die() shows GameOverScreen after 1.5s delay

## 13. UI Layout

### Top Ribbon (110px height, full width)
- Gold icon + count (left, 64x64)
- Wood icon + count
- Food icon + count
- Chapter title: "Chapter I: The Awakening" (28 bold gold)
- Anchor: top, pivot: top-center

### Health Bar (bottom-left, 280x44)
- Green→yellow→red fill (Image with fillAmount)
- "HP X/Y" text overlay
- 8px padding from edges
- Gold ornate border (Make9SliceBorder)

### Reports Panel (top-right, 360x320)
- Anchor: (1, 1), position (-18, -130)
- "REPORTS" header (32 bold gold)
- Last 5 events (18 bold)
- Auto-fade after 8 seconds
- 40px left padding for text

### Minimap (bottom-right, 396x396)
- Static world camera at (0, 140, 0) looking down
- Anchor: (1, 0), position (-16, 16)
- Forced to bottom-right EVERY frame in LateUpdate
- Cleanup of stale MinimapCamera/Minimap in Start
- Player marker (green), enemy markers (red), slot markers (yellow)

### Joystick (bottom-left, 180x180)
- Auto-hide on PC, show on mobile
- 82x82 handle (gold)
- Gold ring around base
- Center indicator

### BuildSlot Tile (world-space canvas)
- Brown square 10x10 quad on ground
- Billboard canvas 4.5 units above
- Name, cost, progress bar
- Hidden when locked or built

### BuildingPopup
- 540x300 parchment panel with gold border
- Red banner with title
- Body text (18 bold)
- CLOSE button (top-right corner of panel)
- Appears at world-to-screen + (0, 100) offset
- Clamped to stay on screen
- Dismissed by: TAB, ESC, X, click backdrop, CLOSE button
- Overlay alpha 0.5 (was 0.7)

### HUDInfoPanel (intro popup)
- Shows on first game launch
- "Chapter I: The Awakening" + "From Zero to Hero"
- Body text intro
- "Press any key to begin"
- Dismissed by any key or click

### GameOverScreen
- 540x340 ornate panel
- "DEFEAT" title (56 bold gold)
- "The village has fallen" subtitle
- Body text (lore)
- TRY AGAIN button
- Time.timeScale = 0 when shown
- Restart: delete enemies, reset health, reset hero position

## 14. Sound (Future)
- Tower firing sound
- Enemy death sound
- Building complete sound
- Background music
- (Not implemented yet)

## 15. Known Issues Being Worked On
- Minimap sometimes stays in center after Clean+Build
- Mage tile sometimes shows too early (fix in SlotManager)
- Popup text sometimes extends past background frame
- Fences only build between ADJACENT towers (intentional)

## 16. Tech Stack
## 16. Tech Stack
- Unity 6 (6000.5.0f1)
- C# scripts
- Polytope Studio Medieval Village pack
- KayKit Adventurers 2.0 FREE (hero, enemies, mage)
- Polytope Polytope vegetation
- Procedural assets for UI (parchment, gold borders via UIStyleHelper)
- No paid assets

## 17. Scripts Architecture

### Core
- `Assets/Scripts/PlayerController3D.cs` - Hero movement, attack, health
- `Assets/Scripts/Enemy.cs` - Enemy AI, Rigidbody movement, knockback
- `Assets/Scripts/EnemySpawner.cs` - Wave management
- `Assets/Scripts/Projectile.cs` - Projectile physics
- `Assets/Scripts/TowerShooter.cs` - Tower and mage shooter

### Building
- `Assets/Scripts/BuildSlot.cs` - Tile system
- `Assets/Scripts/BuildSlotData.cs` - ScriptableObject for slot data
- `Assets/Scripts/SlotManager.cs` - Unlock chain
- `Assets/Scripts/VillageWalls.cs` - Fence and gate

### Hero
- `Assets/Scripts/CameraFollow3D.cs` - Smooth camera follow
- `Assets/Scripts/FloatAndFade.cs` - +gold floating text
- `Assets/Scripts/ProceduralWalk.cs` - Walk bob
- `Assets/Scripts/Joystick.cs` - Touch joystick
- `Assets/Scripts/MobileControls.cs` - On-screen joystick builder

### UI
- `Assets/Scripts/HUDController.cs` - Top ribbon, reports, health bar
- `Assets/Scripts/HUDInfoPanel.cs` - Intro popup
- `Assets/Scripts/MinimapController.cs` - Bottom-right minimap
- `Assets/Scripts/MinimapMarker.cs` - Marker on minimap
- `Assets/Scripts/BuildingPopup.cs` - Building complete popup
- `Assets/Scripts/GameOverScreen.cs` - Death screen
- `Assets/Scripts/UIStyleHelper.cs` - Procedural UI textures

### Editor
- `Assets/Editor/Setup3DScene.cs` - Main scene builder
- `Assets/Editor/SceneCleanup.cs` - Cleanup utility

## 18. Game Flow

### Phase 1: Wake Up
1. Game launches, intro popup appears
2. Player presses any key
3. Hero visible at (0, 0, -55)

### Phase 2: First Building
1. Player walks to church tile (0, 0, 0)
2. Stands on tile, auto-build starts
3. 10 gold deducted over ~1.2s
4. Church appears with smoke puff
5. "Church Raised" popup next to church
6. Reports: "Church built! The village has a heart."
7. Flag tile unlocks, first wave of enemies spawns

### Phase 3: Build Flag
1. Player walks to flag tile
2. Builds flag
3. "Serbian Banner Flies" popup
4. All 4 tower tiles unlock

### Phase 4: Towers
1. Player builds towers (any order)
2. Each tower spawns with TowerShooter
3. Towers auto-shoot at enemies
4. As towers are built, fences appear between adjacent pairs
5. South fence has gate

### Phase 5: Mage Tiles
1. After 4th tower built, all 4 mage tiles unlock
2. Player builds each mage tile
3. Each spawns KayKit Mage.fbx with purple projectiles
4. Mages are more powerful than towers

### Phase 6: Defense
1. Waves continue every 6s
2. Max 12 enemies
3. Towers and mages auto-defend
4. Hero can also attack
5. Hero regenerates in village

### Phase 7: Death (Optional)
1. If hero HP reaches 0
2. Game over screen appears
3. Time pauses
4. Player clicks TRY AGAIN
5. Enemies deleted, hero respawns

## 19. Wave Difficulty
Currently fixed (every 6s, 3 enemies). Future scaling:
- Day 1-3: wolves only
- Day 4-7: wolves + 1 barbarian
- Day 8+: more barbarians, faster, more HP

## 20. Win Condition
Currently no formal win. Future:
- Survive N days/nights
- Build all buildings
- Reach population milestone

## 21. Save System
Currently no save. Future:
- Auto-save on building complete
- Save gold, health, day
- Load on game start

## 22. Build Process
1. Tools → Clean Cache (deletes generated scene file + assets)
2. Tools → Build 3D Scene (regenerates everything)
3. Press Play
4. If issues: close Unity, reopen, try again

## 23. Known Issues
- Minimap position can be inconsistent on first Build
- Mage tile unlock logic (fixed - wait for all 4 towers)
- Popup text alignment (border is now child of panel)
- Health regeneration (now works in village + 2s delay)

## 24. Future Roadmap
- Day/night cycle
- More buildings (Blacksmith, Farm, Tavern)
- More enemy types (mounted raiders)
- Hero leveling system
- Multiple heroes
- Sound and music
- Save/load
- iOS build optimization
- Touch UI refinements
- 3D model improvements (better hero, enemies)
- Quest system
- Multiplayer (co-op village defense)
```
