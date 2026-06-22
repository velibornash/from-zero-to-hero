package com.game.resources;

public enum ResourceType {
    FOOD("Hrana"),
    WOOD("Drvo"),
    GOLD("Zlato"),
    STONE("Kamen");

    private final String displayName;

    ResourceType(String displayName) {
        this.displayName = displayName;
    }

    public String getDisplayName() {
        return displayName;
    }
}
