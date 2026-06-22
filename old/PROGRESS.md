# 🏗️ From Zero to Hero — PROGRESS

## ✅ Completed

### Project Setup
- [x] Maven pom.xml sa LibGDX 1.12.1 i Jackson 2.15.3
- [x] Java 21
- [x] Package structure (core, world, entities, systems, ai, events, buildings, resources, render)
- [x] Projekat se kompajlira (`mvn compile`)

### Core Data Models
- [x] `Entity` — base class (id, position, hp, alive state)
- [x] `Worker` — state machine with commanded move
- [x] `Enemy` — moves toward target, attacks on contact
- [x] `ResourceNode` — depletable resource (FOOD, WOOD, GOLD)
- [x] `Building` / `BuildingType` — definisan tip, cena, proizvodnja
- [x] `Tile` — grid cell (terrain, building, resource, occupant)
- [x] `WorldGrid` — 50×50 tile grid, resource placement, enemy wave spawning
- [x] `ResourceType` — FOOD, WOOD, GOLD, STONE
- [x] `ResourceContainer` — multi-resource inventory
- [x] `TerrainType` — GRASS, DIRT, WATER, FOREST, STONE

### Event System
- [x] `Event` — base event with id, name, description, type
- [x] `GameEvent` — data-driven event with conditions + effects
- [x] `EventQueue` — FIFO queue processed each fixed tick
- [x] `EventDispatcher` — pub/sub dispatch to systems
- [x] `EventRegistry` — holds all events, unlock/trigger logic

### 5 Story Events (GDD)
- [x] **Prvi oganj sela** — unlocks Basic Hut, Woodcutting; +resursi
- [x] **Vukovi sa Rtnja** — prvi enemy wave (vukovi), unlocks Watchtower
- [x] **Senka Kosančića Ivana** — morale boost, unlocks Monastery
- [x] **Zmaj sa planine** — boss wave, otključava mythic slot
- [x] **Dolazak Kraljevića Marka** — save mehanika: heal + AoE

### Systems
- [x] `ResourceSystem` — food consumption, production, morale modifier, kafana/monastery morale
- [x] `WorkerSystem` — auto-assign nearest resource, gather/deposit loop, commanded move
- [x] `BuildingSystem` — construction validation (canBuild/build)
- [x] `CombatSystem` — enemy AI, periodic waves, base damage, watchtower defense, worker combat
- [x] `EventSystem` — proverava uslove svaki tick, lančano otključava

### Game Engine
- [x] `GameEngine` — fixed timestep @ 60 ticks/sec, error capping
- [x] `GameState` — resources, population, morale, base HP, unlocked content, build mode, notification
- [x] Centriranje kamere na grid, WASD/strelica pomeranje

### Input & Interaction
- [x] **Levi klik** — selektuje tile, prikazuje info panel (terrain, building, resources)
- [x] **Desni klik** — pomeri najbližeg workera na tile
- [x] **B** — ulaz/izlaz iz build mode
- [x] **1-8** — izbor tipa zgrade u build mode
- [x] **ESC** — izlaz iz build mode / gašenje igre

### Construction System
- [x] **UI panel** — desna strana, lista dostupnih zgrada sa cenom i proizvodnjom
- [x] **Ghost building** — green/red preview na hover tile-u
- [x] **Potvrda gradnje** — klik na tile u build mode, troši resurse, prikazuje poruku
- [x] 8 definisanih tipova zgrada (Koliba, Drvoseča, Stražara, Manastir, Pekara, Kafana, Kasarna, Mitski Slot)

### Morale System (dublje)
- [x] **Morale → Worker speed** — `getMoraleSpeedModifier()` (0.5–1.0x skaliranje)
- [x] **Morale → Combat** — `getMoraleCombatModifier()` za damage
- [x] **Heroic Surge** — random buff kad morale > 70, traje 10s, daje workerima zlatnu boju
- [x] **Kafana** daje +1 moral/tick, **Manastir** daje +2 moral/tick
- [x] **Morale UI boja** — crvena (<30), žuta (30-60), zelena (>60)

### Rendering (LibGDX)
- [x] Grid tile-ovi sa bojama po terenu
- [x] Grid linije
- [x] Resource nodovi (boja po tipu)
- [x] Zgrade (braon blokovi sa HP barom)
- [x] Worker-i (plavi krugovi, zlatni za Heroic Surge, beli prsten za commanded)
- [x] Neprijatelji (crveni krugovi sa HP barom)
- [x] Ghost building (zeleno/crveno)
- [x] Selektovan tile (žuti highlight)
- [x] HUD: resursi, radnici, moral (color-coded), baza HP, event poruke, build mode UI panel
- [x] Tile info panel (desni gornji ugao)
- [x] Notification sistem (center screen)
- [x] Game Over screen

---

## 🔄 In Progress

- _None — sve za ovu fazu završeno_

---

## ⏭️ Next Steps (prioritized)

### 1. Hero System
- [ ] `Hero` entitet (Kraljević Marko) — dedicated class sa posebnim ponašanjem
- [ ] Hero spawn-uje se kad su uslovi ispunjeni (base HP < 30%)
- [ ] AoE napad, privremeni invincibility buff
- [ ] Hero rendering (drugačiji sprite/boja)

### 2. Save/Load
- [ ] JSON serializacija (Jackson) za GameState i WorldGrid
- [ ] Save na disk, load na start
- [ ] SQLite kasnije za meta-progression

### 3. UI Improvements
- [ ] Event notification popup sa izborima (Zmaj event: ubiti vs smiriti)
- [ ] Mini-map (mali pregled celog grida)
- [ ] Building tooltip na hover
- [ ] Progress bar za worker gathering

### 4. Combat Improvements
- [ ] Pathfinding za neprijatelje (A* ka centru)
- [ ] Tipovi neprijatelja (brzi, tanky, boss)
- [ ] Kolizija između entiteta
- [ ] Worker-i beže od neprijatelja kad su ugroženi

### 5. Audio
- [ ] Background music
- [ ] SFX (build, combat, event, UI click)

### 6. Balancing & Content
- [ ] Više era (Srednji vek, Modernizacija)
- [ ] Više enemy varijanti
- [ ] Era progression sistem sa uslovima za prelazak
- [ ] Tech tree

### 7. Meta Progression
- [ ] Unlock novih zgrada između run-ova
- [ ] Jači start svakog novog run-a
- [ ] Achievement sistem

---

## 🐛 Known Issues
- Workers ne reaguju na promenu terena (mogu hodati kroz FOREST/ne-walkable)
- CombatSystem targetovanje je basic (najbliži worker, ignorisanje zgrada)
- Nema kolizije između entiteta (worker prolazi kroz enemy)
- Ghost building se vidi i van grida (negativne koordinate)

---

## 🧱 Architecture Overview

```
com.game
├── FromZeroToHero.java        # ApplicationListener + input handling
├── DesktopLauncher.java        # Entry point
├── core/
│   ├── GameEngine.java         # Fixed timestep engine, coordinates systems
│   └── GameState.java          # Central state (resources, unlocked, morale, build mode)
├── world/
│   ├── WorldGrid.java          # 50×50 tile grid + entity lists
│   ├── Tile.java               # Single grid cell
│   └── TerrainType.java        # Enum
├── entities/
│   ├── Entity.java             # Base (id, hp, position)
│   ├── Worker.java             # State machine AI + commanded move
│   ├── ResourceNode.java       # Depletable resource
│   └── Enemy.java              # Combat AI
├── buildings/
│   ├── BuildingType.java       # Template (cost, production, hp)
│   └── Building.java           # Instance
├── resources/
│   ├── ResourceType.java       # Enum
│   └── ResourceContainer.java  # Multi-resource map
├── events/
│   ├── Event.java              # Base event
│   ├── GameEvent.java          # Data-driven with conditions/effects
│   ├── EventQueue.java         # FIFO processing
│   ├── EventDispatcher.java    # Pub/sub dispatch
│   └── EventRegistry.java      # Registry + 5 story events
├── systems/
│   ├── ResourceSystem.java     # Production/consumption, morale buildings
│   ├── WorkerSystem.java       # Task assignment, commanded move, Heroic Surge trigger
│   ├── BuildingSystem.java     # Construction validation
│   ├── CombatSystem.java       # Enemy waves, combat, watchtower, morale modifier
│   └── EventSystem.java        # Condition checking + chaining
├── ai/
│   └── BasicAI.java            # Enemy movement helper
└── render/
    └── GameRenderer.java       # Full UI: HUD, build panel, tile info, ghost, notifications
```

---

## 🎮 How to Run

```bash
mvn compile exec:java -Dexec.mainClass="com.game.DesktopLauncher"
# or
mvn package
java -jar target/from-zero-to-hero-1.0-SNAPSHOT.jar
```

**Controls:**
- WASD / Arrow Keys — pomeranje kamere
- B — Build Mode (ulaz/izlaz)
- 1-8 — izbor zgrade u Build Mode
- Levi klik — selektuj tile / postavi zgradu
- Desni klik — pomeri workera
- ESC — izlaz iz build mode / gašenje igre
