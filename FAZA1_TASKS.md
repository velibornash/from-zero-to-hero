# FAZA 1 — Make the Game Fun

## TASK 1 — Hero Movement & Feel ✓
- [x] Acceleration/deceleration → `Vector3.MoveTowards` at 25 units/s²
- [x] Camera: fog, mobile touch orbit/zoom
- [x] HP bar as ScreenSpaceOverlay tracking hero
- [x] Animator controllers restored (hero, barbarian, wolf)
- [ ] Next: footstep particles, subtle jump (spacebar), speed curve (ease-in)

## TASK 2 — Resource Gathering (WOOD)
- [x] Fix hero acceleration (was sliding)
- [ ] **TreeController.cs** — placed on each scene tree
  - Detect hero within 3f + E press
  - Hero attack animation + sound
  - Smoke puff (reuse BuildSlot.SmokePuff pattern)
  - Replace tree model → stump (`PT_Pine_Tree_03_green_cut`)
  - Spawn log pile (`PT_Pine_Tree_03_logs`) with pickup script
- [ ] **ResourcePickup.cs** — on log pile
  - Trigger collider
  - Hero walks over → +Wood, floating text, destroy
- [ ] Modify **Setup3DScene.cs** — add component + tag to forest trees
- [ ] Modify **HUDController.cs** — add `AddResource(type, amount)` method

## TASK 3 — Lumber Mill Automation
- [x] Resource buildings (church/flag) spawn NPC worker
- [x] Worker uses Ranger character model from KayKit set
- [x] `WorkerAI` patrols to resource nodes and gathers wood
- [ ] Dedicated Lumber Mill building + woodcutter loop
- [ ] Reuse `NPCPatrol` with `idleOnly=false`

## TASK 4 — Feedback Everywhere
- Floating text for: gold earned, wood chopped, building complete, quest update
- Sound effects for: pickup, build complete, damage taken, enemy death

## TASK 5 — Wolf Attacks on Villagers
- Wolves target NPCs (not just hero)
- NPCs have health, can die
- Hero must defend villagers

## TASK 6 — Day Cycle ✓
- [x] 3m day / 2m night cycle via `DayNightCycle`
- [x] Sun rotation + ambient/fog gradients
- [x] UI clock/indicator at top center
- [x] Brighter day, darker night
- [ ] Morning → gather, Afternoon → build, Evening → defend, Night → rewards
- [ ] Night: reduced visibility, more wolves

## TASK 7 — Hero Attack Upgrade
- Weapon upgrade via Lumber Mill → bigger range, more damage
- Area attack (cleave) when surrounded

## TASK 8 — Villager NPCs
- Rescue/find villagers in the valley
- They auto-assist: collect resources, repair walls, man towers

## TASK 9 — Building UI Polish
- Building slot shows required resources + progress bar
- Cancel building (refund partial)
- Queue multiple buildings

## TASK 10 — Resource Diversity
- Stone quarry: mine rock → stone (for walls, towers)
- Wheat fields: harvest → food (for healing/recruiting)
- Gold mines: rare, rich deposits

## TASK 11 — Wall Building
- Player places wall segments between towers
- Enemies pathfind around walls or break through

## TASK 12 — Wave Defense
- Scripted waves with increasing difficulty
- Between waves: brief calm for repairs
- Boss wolf every 5 waves

## TASK 13 — Tutorial
- [x] Welcome screen on first launch
- [x] Nikola Tesla introduction via `ElderPopup`
- [x] Elder popup on building completion
- [ ] Highlight key objects
- [ ] Context-sensitive hints

## TASK 14 — Save/Load
- Save game state (resources, buildings, day)
- Load on restart

## TASK 15 — Soundtrack & SFX
- Ambient background music
- SFX for all interactions
- Voice lines for key events
