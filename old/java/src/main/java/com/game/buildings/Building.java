package com.game.buildings;

import com.game.resources.ResourceType;

import java.util.Map;

public class Building {
    private final BuildingType type;
    private final int tileX;
    private final int tileY;
    private float hp;
    private boolean destroyed;

    public Building(BuildingType type, int tileX, int tileY) {
        this.type = type;
        this.tileX = tileX;
        this.tileY = tileY;
        this.hp = type.getMaxHp();
        this.destroyed = false;
    }

    public BuildingType getType() { return type; }
    public int getTileX() { return tileX; }
    public int getTileY() { return tileY; }
    public float getHp() { return hp; }

    public void takeDamage(float amount) {
        hp -= amount;
        if (hp <= 0) {
            hp = 0;
            destroyed = true;
        }
    }

    public boolean isDestroyed() { return destroyed; }
    public boolean isAlive() { return !destroyed; }

    public Map<ResourceType, Float> getProduction() { return type.getProduction(); }
}
