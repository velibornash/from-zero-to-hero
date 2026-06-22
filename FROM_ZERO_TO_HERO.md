# From Zero to Hero

Living build log for the fresh restart.

## Stack

- Unity 6000.5
- C#
- Built-in 3D pipeline for now
- Free asset packs already present in the repo

Why this stack:

- free to use
- built for 3D characters, buildings, camera framing, and mobile-style production UI
- lets us keep the game as a real 3D diorama instead of a visible tile prototype
- reusable asset pipeline for mobile-store style presentation

## Current Build Target

The game should open into a 3D village scene with:

- a visible hero
- visible buildings and props
- an invisible logical grid
- only the current interaction square highlighted
- walking, buying, building, and enemy pressure
- story events that change the rules

## What Is Done

- Unity project scaffold created in `unity-game/`
- project bootstrapping is scripted
- key source art has been moved into Unity-ready resource folders
- living progress document exists

## What Is Left

- Finish asset import into `unity-game/Assets/Resources`
- Add the first playable scene with camera, light, hero, HUD, and interaction square
- Add the first building/shop loop
- Add worker spawning and simple production
- Add the first story events and raid pressure
- Add save/load and meta progression

## How To Run

1. Open `unity-game/` in Unity Hub.
2. Open the project in Unity 6000.5.
3. Let the bootstrap script create the first scene if prompted.

If you need a direct path to the editor, use the installed Unity app inside:

```text
/Applications/Unity/Hub/Editor/6000.5.0f1/Unity.app
```

## What To Expect In The First Slice

- 3D village space
- hero movement with arrow keys
- a visible interaction square instead of visible tile grid lines
- one or two recognizable buildings
- a clean HUD
- event messages when the opening conditions are met

## Development Rule

Do not fall back into prototype visuals.

The order is:

1. 3D world readability
2. character and building identity
3. interaction and progression
4. animation polish
5. content expansion
