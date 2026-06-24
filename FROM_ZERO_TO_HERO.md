# From Zero to Hero

Living build log for the project.

## Stack

- Unity 6000.5 (C#)
- Built-in 3D pipeline
- Free asset packs from the Unity Asset Store

## Design Goal

A 3D city-builder driven by a physically-present hero. The hero walks across the village, activates buildings, and pushes development through Serbian historical eras. Visual target: a clean 3D diorama in the style of mobile city-builders (Outlet Rush, Whiteout Survival).

## Current Build Target

The game should open into a 3D village scene with:

- a visible hero (third-person controller)
- visible buildings and props placed on the terrain
- an invisible logical grid for placement
- only the current interaction square highlighted
- walking, building, and economy
- story events that change the rules

## Current State

- Unity 6000.5.0f1 project at repo root
- `Assets/Scripts/` contains the gameplay scripts: `PlayerController3D`, `CameraFollow3D`, `BuildZone`, `Combat`, `NPCPatrol`, `HUDController`, `HUDInfoPanel`, `MinimapController`, `FloatingText`, `Billboard`
- `Assets/Editor/Setup3DScene.cs` bootstraps a 3D scene (player, camera, ground, props)
- `Assets/nova scena.unity` is the active hand-built scene
- Asset packs already in the repo: EmaceArt, Environment Starter Pack, GuiPack2DFree, InfinityPBR - Magic Pig Games, MyDreamGameStudio, Polytope Studio, 3D

## What's Next

1. Wire `BuildZone` to the logical grid
2. Building prefabs (kafana, pekara, toranj, crkva)
3. Resource UI + economy loop
4. Story events and era progression
5. NPC behavior (idle, patrol, talk)

## How To Run

```bash
open -a "/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app" /Users/velja/IdeaProjects/from-zero-to-hero
# Then press Play in the editor (or Cmd+B to build)
```

## Project Layout

```
Assets/         source art, scenes, scripts
Packages/       Unity package manifest
ProjectSettings/ Unity project config
UserSettings/   per-user editor settings (gitignored)
Library/        Unity build cache (gitignored)
Logs/           Unity editor logs (gitignored)
```
