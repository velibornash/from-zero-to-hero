package com.game.world;

import com.game.buildings.Building;
import com.game.entities.Entity;
import com.game.entities.ResourceNode;

public class Tile {
    private final int x;
    private final int y;
    private TerrainType terrain;
    private Building building;
    private ResourceNode resourceNode;
    private Entity occupant;

    public Tile(int x, int y, TerrainType terrain) {
        this.x = x;
        this.y = y;
        this.terrain = terrain;
    }

    public int getX() { return x; }
    public int getY() { return y; }
    public TerrainType getTerrain() { return terrain; }
    public void setTerrain(TerrainType terrain) { this.terrain = terrain; }

    public Building getBuilding() { return building; }
    public void setBuilding(Building building) { this.building = building; }
    public boolean hasBuilding() { return building != null; }

    public ResourceNode getResourceNode() { return resourceNode; }
    public void setResourceNode(ResourceNode node) { this.resourceNode = node; }
    public boolean hasResourceNode() { return resourceNode != null && resourceNode.isAlive(); }

    public Entity getOccupant() { return occupant; }
    public void setOccupant(Entity occupant) { this.occupant = occupant; }
    public boolean isOccupied() { return occupant != null; }

    public boolean isWalkable() {
        return terrain.isWalkable() && !hasBuilding();
    }

    public boolean isBuildable() {
        return terrain == TerrainType.GRASS && !hasBuilding() && !hasResourceNode();
    }
}
