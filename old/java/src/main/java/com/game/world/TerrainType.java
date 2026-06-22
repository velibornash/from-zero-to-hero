package com.game.world;

public enum TerrainType {
    GRASS("Trava", true),
    DIRT("Zemlja", true),
    STONE("Kamen", false),
    WATER("Voda", false),
    FOREST("Suma", false);

    private final String displayName;
    private final boolean walkable;

    TerrainType(String displayName, boolean walkable) {
        this.displayName = displayName;
        this.walkable = walkable;
    }

    public String getDisplayName() { return displayName; }
    public boolean isWalkable() { return walkable; }
}
