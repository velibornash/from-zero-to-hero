package com.game.systems;

import com.game.buildings.Building;
import com.game.buildings.BuildingType;
import com.game.core.GameState;
import com.game.events.Event;
import com.game.resources.ResourceType;
import com.game.world.WorldGrid;

import java.util.function.Consumer;

public class BuildingSystem implements Consumer<Event> {
    private final WorldGrid world;
    private final GameState gameState;

    public BuildingSystem(WorldGrid world, GameState gameState) {
        this.world = world;
        this.gameState = gameState;
    }

    public void update(float dt) {
        for (Building building : world.getBuildings()) {
            if (!building.isAlive()) continue;
        }
    }

    public boolean canBuild(BuildingType type, int tileX, int tileY) {
        var tile = world.getTile(tileX, tileY);
        if (tile == null || !tile.isBuildable()) return false;
        if (!gameState.getResources().hasAll(type.getBuildCost())) return false;
        if (!gameState.isBuildingUnlocked(type.getId())) return false;
        return true;
    }

    public boolean build(BuildingType type, int tileX, int tileY) {
        if (!canBuild(type, tileX, tileY)) return false;
        if (!gameState.getResources().spendAll(type.getBuildCost())) return false;

        Building building = new Building(type, tileX, tileY);
        world.addBuilding(building);
        return true;
    }

    @Override
    public void accept(Event event) {}
}
