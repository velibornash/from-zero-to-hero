# From Zero to Hero

Working document for the game prototype and development plan.

## Direction

We are keeping the project on a free toolchain only:

- Godot 4
- GDScript
- Blender for 3D content
- Krita or GIMP for 2D art
- Audacity for audio cleanup
- Free assets only when needed

This repo already contains a Godot prototype with assets, so the practical move is to continue from there instead of splitting into another stack.

## Current Status

### Done

- Basic Godot project is present in `godot-game/`
- Main scene exists and loads a world scene
- Hero, world generation, building slots, and HUD scaffolding are already in place
- Asset packs are imported and available

### In Progress

- Replacing the current placeholder presentation with a cleaner, more modern visual direction
- Reducing UI clutter and moving toward a more cinematic game HUD
- Defining a consistent art direction for buildings, characters, terrain, and effects

### Remaining

- Make the world presentation feel deliberate, not prototype-like
- Replace temporary UI controls with a proper game interface
- Add stronger camera framing and better motion/animation polish
- Build the first story-driven gameplay slice around the opening events
- Add save/load and progression tracking

## Visual Target

The target is not a generic editor UI or a debug-looking sandbox.

The game should feel like:

- a modern city survival game
- with strong atmosphere
- readable but restrained HUD
- believable animation and presentation
- clear scene composition with a cinematic feel

## Practical Development Plan

1. Lock the game direction to one playable slice.
2. Clean the scene hierarchy and separate gameplay from UI.
3. Replace temporary HUD pieces with a minimal production-style layout.
4. Push lighting, materials, camera, and animation before adding more systems.
5. Add the first story event chain and tie it to visible gameplay changes.

## What Is In The Repo

- `godot-game/` - active Godot prototype
- `old/java/` - earlier Java/LibGDX prototype
- `old/prototypes/` - older experimental branches

## How To Run The Current Project

1. Open the `godot-game/` folder in Godot 4.
2. Make sure the main scene is `res://scenes/main.tscn`.
3. Press Play in the editor.

If you want to run from the command line, use the Godot editor executable with the project folder:

```bash
godot --path godot-game
```

Depending on your local installation, the executable may be named `godot4` instead of `godot`.

## What You Should See Right Now

- A 3D world scene with terrain, trees, flowers, rocks, and water
- A hero character
- Building slots / construction scaffolding
- A top-bar HUD with resource counters
- A build panel toggled by the `B` key

## Tracking Format

Use this file as the living checklist:

- update `Done` when a subsystem is complete
- move items from `Remaining` into `In Progress` before editing
- keep the visual target stable so the implementation does not drift back into prototype UI

