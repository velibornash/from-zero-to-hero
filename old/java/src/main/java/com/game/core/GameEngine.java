package com.game.core;

import com.game.buildings.Building;
import com.game.buildings.BuildingType;
import com.game.entities.Hero;
import com.game.entities.ResourceNode;
import com.game.entities.Worker;
import com.game.events.*;
import com.game.resources.ResourceType;
import com.game.systems.*;
import com.game.world.Tile;
import com.game.world.WorldGrid;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class GameEngine {
    private static final float FIXED_DELTA = 1f / 60f;

    private final WorldGrid world;
    private final GameState gameState;
    private final Hero hero;
    private final EventQueue eventQueue;
    private final EventDispatcher eventDispatcher;
    private final EventRegistry eventRegistry;
    private final ResourceSystem resourceSystem;
    private final WorkerSystem workerSystem;
    private final BuildingSystem buildingSystem;
    private final CombatSystem combatSystem;
    private final EventSystem eventSystem;
    private final List<BuildingType> buildingTypes;
    private float accumulator;
    private float totalTime;
    private boolean running;
    private String activeEventMessage;
    private float eventMessageTimer;

    public GameEngine() {
        this.world = new WorldGrid(50, 50);
        this.gameState = new GameState();
        this.hero = new Hero(25 * 32, 25 * 32);
        this.eventQueue = new EventQueue();
        this.eventDispatcher = new EventDispatcher();
        this.eventRegistry = new EventRegistry();

        this.resourceSystem = new ResourceSystem(world, gameState);
        this.workerSystem = new WorkerSystem(world, gameState);
        this.buildingSystem = new BuildingSystem(world, gameState);
        this.combatSystem = new CombatSystem(world, gameState);
        this.eventSystem = new EventSystem(world, gameState, eventRegistry, eventQueue);

        this.buildingTypes = new ArrayList<>();
        initBuildingTypes();

        eventSystem.setEngine(this);

        eventDispatcher.registerGlobal(event -> {
            activeEventMessage = event.getName() + ": " + event.getDescription();
            eventMessageTimer = 8f;
        });

        eventRegistry.registerEvents(eventDispatcher);

        world.placeInitialResources();

        gameState.addPopulation(2);

        Worker w1 = new Worker(24 * 32, 25 * 32);
        Worker w2 = new Worker(26 * 32, 25 * 32);
        world.addWorker(w1);
        world.addWorker(w2);

        this.accumulator = 0;
        this.totalTime = 0;
        this.running = true;
    }

    private void initBuildingTypes() {
        buildingTypes.add(new BuildingType("basic_hut", "Koliba", "U staroj Srbiji, koliba je bila prvo sklonište pastira. Ovdje seljaci nalaze mir i dom. +2 max populacija.",
                2, Map.of(ResourceType.WOOD, 15f), Map.of(), 30f));
        buildingTypes.add(new BuildingType("woodcutting", "Drvoseca", "Po ugledu na šume Fruške gore, drvoseče donose drvo za gradnju. +5 drva/tick.",
                1, Map.of(ResourceType.WOOD, 10f), Map.of(ResourceType.WOOD, 5f), 40f));
        buildingTypes.add(new BuildingType("watchtower", "Stražara", "Kao kule na Smederevskom gradu, štiti narod od najezda. Stetuje neprijateljima u blizini.",
                1, Map.of(ResourceType.WOOD, 25f, ResourceType.STONE, 10f), Map.of(), 60f));
        buildingTypes.add(new BuildingType("monastery", "Manastir", "Poput Hilandara i Studenice, mjesto duhovne snage i pismenosti. +2 moral/tick.",
                2, Map.of(ResourceType.WOOD, 20f, ResourceType.STONE, 15f, ResourceType.GOLD, 5f), Map.of(), 50f));
        buildingTypes.add(new BuildingType("pekara", "Pekara Kirćanski", "Hleb iz ove pekare hranio je Karađorđeve ustanike. +5 hrane/tick.",
                2, Map.of(ResourceType.WOOD, 20f), Map.of(ResourceType.FOOD, 5f), 50f));
        buildingTypes.add(new BuildingType("kafana", "Kafana Čardak", "Na čardaku se pila rakija i kovale bune. Srce srpskog okupljanja i moral.",
                1, Map.of(ResourceType.WOOD, 15f, ResourceType.GOLD, 5f), Map.of(), 40f));
        buildingTypes.add(new BuildingType("kasarna", "Kasarna Topčider", "Vojska koja je branila Beograd u 19. veku. Ovdje se treniraju novi vojnici.",
                3, Map.of(ResourceType.WOOD, 30f, ResourceType.GOLD, 15f, ResourceType.STONE, 10f), Map.of(), 80f));
        buildingTypes.add(new BuildingType("mythic_slot", "Mitski Portal", "Na mestu gde je Marko Kraljević probio planinu, sila mita otključava tajne moći.",
                1, Map.of(ResourceType.GOLD, 50f, ResourceType.STONE, 30f, ResourceType.WOOD, 40f), Map.of(), 100f));
    }

    public void update(float delta) {
        if (!running) return;
        if (delta > 0.1f) delta = 0.1f;

        accumulator += delta;
        totalTime += delta;
        gameState.update(delta);

        if (eventMessageTimer > 0) {
            eventMessageTimer -= delta;
            if (eventMessageTimer <= 0) {
                activeEventMessage = null;
            }
        }

        if (gameState.isBaseDestroyed()) {
            running = false;
            gameState.showNotification("GAME OVER - Baza je unistena!");
            return;
        }

        while (accumulator >= FIXED_DELTA) {
            fixedUpdate(FIXED_DELTA);
            accumulator -= FIXED_DELTA;
        }
    }

    private void fixedUpdate(float dt) {
        eventQueue.process(eventDispatcher);
        resourceSystem.update(dt);
        workerSystem.update(dt);
        buildingSystem.update(dt);
        combatSystem.update(dt);
        eventSystem.update(dt);
    }

    public void checkHeroCollection() {
        int tx = hero.getTileX();
        int ty = hero.getTileY();
        Tile tile = world.getTile(tx, ty);
        if (tile == null) return;
        ResourceNode node = tile.getResourceNode();
        if (node != null && node.isAlive()) {
            float amount = Math.min(node.getAmount(), 5f);
            node.harvest(amount);
            switch (node.getType()) {
                case GOLD -> hero.addGold(amount);
                case FOOD -> hero.addFood(amount);
                case WOOD -> hero.addWood(amount);
            }
            if (!node.isAlive()) {
                tile.setResourceNode(null);
            }
        }
    }

    public void buildOnHeroTile() {
        int tx = hero.getTileX();
        int ty = hero.getTileY();
        int idx = gameState.getSelectedBuildingIndex();
        if (idx < 0 || idx >= buildingTypes.size()) return;
        BuildingType type = buildingTypes.get(idx);
        if (!gameState.isBuildingUnlocked(type.getId())) {
            gameState.showNotification("Zgrada nije otkljucana!");
            return;
        }
        Tile tile = world.getTile(tx, ty);
        if (tile == null || tile.hasBuilding()) {
            gameState.showNotification("Ovo mesto je zauzeto!");
            return;
        }
        if (!world.isBuildingSlot(tx, ty)) {
            gameState.showNotification("Ovo nije gradjevinski slot!");
            return;
        }
        float totalCost = 0;
        for (Float cost : type.getBuildCost().values()) {
            totalCost += cost;
        }
        if (!hero.spendGold(totalCost)) {
            gameState.showNotification("Nemate dovoljno zlata! Potrebno: " + (int)totalCost);
            return;
        }
        Building building = new Building(type, tx, ty);
        world.addBuilding(building);
        gameState.showNotification("Izgradjeno: " + type.getName());
    }

    public void tryBuild(int tileX, int tileY) {
        int idx = gameState.getSelectedBuildingIndex();
        if (idx < 0 || idx >= buildingTypes.size()) return;
        BuildingType type = buildingTypes.get(idx);
        if (buildingSystem.build(type, tileX, tileY)) {
            gameState.showNotification("Izgradjeno: " + type.getName());
        } else {
            gameState.showNotification("Ne moze ovde! (nedovoljno resursa ili zauzeto)");
        }
    }

    public void commandWorker(int tileX, int tileY) {
        workerSystem.commandWorkerTo(tileX, tileY);
    }

    public Hero getHero() { return hero; }

    public String getTileInfo(int tileX, int tileY) {
        var tile = world.getTile(tileX, tileY);
        if (tile == null) return "Van sveta";
        StringBuilder sb = new StringBuilder();
        sb.append("Tile [").append(tileX).append(",").append(tileY).append("]\n");
        sb.append("Terrain: ").append(tile.getTerrain().getDisplayName());
        if (tile.hasBuilding()) {
            sb.append("\nZgrada: ").append(tile.getBuilding().getType().getName());
            sb.append(" HP: ").append((int)tile.getBuilding().getHp());
        }
        if (tile.hasResourceNode()) {
            var node = tile.getResourceNode();
            sb.append("\nResurs: ").append(node.getType().getDisplayName());
            sb.append(" (").append((int)node.getAmount()).append("/").append((int)node.getMaxAmount()).append(")");
        }
        return sb.toString();
    }

    public WorldGrid getWorld() { return world; }
    public GameState getGameState() { return gameState; }
    public EventRegistry getEventRegistry() { return eventRegistry; }
    public EventDispatcher getEventDispatcher() { return eventDispatcher; }
    public BuildingSystem getBuildingSystem() { return buildingSystem; }
    public ResourceSystem getResourceSystem() { return resourceSystem; }
    public List<BuildingType> getBuildingTypes() { return buildingTypes; }
    public float getTotalTime() { return totalTime; }
    public boolean isRunning() { return running; }
    public String getActiveEventMessage() { return activeEventMessage; }
}
