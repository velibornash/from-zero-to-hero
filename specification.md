# From Zero to Hero — Game Specification

## Overview
A medieval hero-driven city builder for iPhone. The player (Knight) defends a Serbian village from waves of wolves and barbarians by building structures and upgrading towers with mages.

## Starting State
- Hero (Knight) at (0, 0, -55) — south of village center
- Church slot at (0, 0, 0) — UNLOCKED (cost 10 gold)
- Flag slot at (-2, 0, -48) — LOCKED
- 4 Tower slots at the 4 corners — LOCKED
- 4 Mage tile slots — LOCKED
- Starting gold: 10
- Starting health: 100/100

## Building Chain
1. **Church** (10g) → unlocks Flag
2. **Flag** (10g) → unlocks all 4 Towers
3. **Tower x4** (10g each) → unlocks all 4 Mage tiles
4. **Mage Tile x4** (10g each) → adds Mage defender

## Tower Positions (world space)
- SW: (-70, 0, -50)
- SE: (70, 0, -50)
- NE: (70, 0, 50)
- NW: (-70, 0, 50)

## Mage Tile Positions (15 units inward from each tower)
- Mage 1: (-55, 0, -35) — SW
- Mage 2: (55, 0, -35) — SE
- Mage 3: (55, 0, 35) — NE
- Mage 4: (-55, 0, 35) — NW

## Fence System
Fences only build between ADJACENT corner towers:
- SW + SE → South fence (with gate at center)
- SE + NE → East fence
- NE + NW → North fence
- NW + SW → West fence
- Gate: opens when hero is within 6m, closes otherwise
- Enemies are blocked by fences AND closed gate
- Hero can pass through open gate

## Enemies
### Wolf
- HP: 2, gold: 5, speed: 6.5, scale: 3.5x
### Barbarian
- HP: 4, gold: 8, speed: 5.5, scale: 2.8x
### Spawn
- Starts when Church is built
- Wave every 6 seconds, 3 per wave, max 12 concurrent
- 4 spawn points in west forest

## Towers
- Range: 30, fire rate: 0.7s, damage: 1
- Yellow projectile with trail and muzzle flash
- Rotates to face closest enemy

## Mages
- Range: 35, fire rate: 0.45s, damage: 2
- Purple projectile
- Uses KayKit Mage.fbx (scale 2.0)
- Auto-aims and shoots

## Hero System
### Health
- Max HP: 100
- Damage per enemy hit: 15
- Regen: 10 HP/s when in village (within 70m of center) + 2s after last hit
- Death: shows Game Over screen

### Combat
- Auto-attack: 1 damage per hit, range 2.2m, rate 0.35s
- Health bar at bottom-left of screen (green → yellow → red)

### Game Over
- Pauses game (Time.timeScale = 0)
- Shows DEFEAT panel with TRY AGAIN button
- Restart: deletes all enemies, resets health, repositions hero

## UI Layout
### Top Ribbon (110px height)
- Gold icon + "10" (left)
- Wood icon + "0"
- Food icon + "0"
- "Chapter I: The Awakening" text

### Reports Panel (top-right, 360x320)
- Last 5 events
- Auto-fade after 8 seconds

### Minimap (bottom-right, 396x396)
- Static world view
- Green circle = hero
- Red circles = enemies
- Yellow circles = unbuilt slots

### Joystick (bottom-left, mobile only)
- On-screen virtual joystick
- Auto-hidden on PC

### BuildingPopup
- Appears next to building
- World-to-screen with 100px upward offset
- Clamped to stay on screen
- Tab/Esc/X to close, click backdrop to close

## Build System
- Auto-build: 1 gold per 0.12s while standing on tile
- Click tile: instant finish
- No gold: build pauses at 0, can be resumed

## Smoke Effect
- 30 puffs when building completes
- Gray color, expand and fade over 2.5s

## Death Effects
- Smoke puff (30 puffs)
- Death burst (20 chunks, fly in random directions, 1.5s)
- +gold floating text above (1.2s fade)
- "Defeated X! +N gold" in Reports

