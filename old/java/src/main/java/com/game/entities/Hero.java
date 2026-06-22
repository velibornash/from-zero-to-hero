package com.game.entities;

import com.game.world.WorldGrid;

public class Hero extends Entity {
    private static final float SPEED = 120f;
    private static final float WORLD_SIZE = 50 * WorldGrid.TILE_SIZE;

    private float gold;
    private float food;
    private float wood;
    private int tileX;
    private int tileY;
    private float facingX;
    private float facingY;

    public Hero(float x, float y) {
        super(x, y, 100f);
        this.gold = 0;
        this.food = 0;
        this.wood = 0;
        this.facingX = 1;
        this.facingY = 0;
        updateTilePos();
    }

    public void update(float dt) {
        updateTilePos();
    }

    private void updateTilePos() {
        this.tileX = (int)((x + 16) / WorldGrid.TILE_SIZE);
        this.tileY = (int)((y + 16) / WorldGrid.TILE_SIZE);
    }

    public void move(float dx, float dy, float dt) {
        if (dx != 0 || dy != 0) {
            float len = (float)Math.sqrt(dx * dx + dy * dy);
            dx /= len;
            dy /= len;
            facingX = dx;
            facingY = dy;
        }
        x += dx * SPEED * dt;
        y += dy * SPEED * dt;
        x = Math.max(0, Math.min(WORLD_SIZE - 32, x));
        y = Math.max(0, Math.min(WORLD_SIZE - 32, y));
        updateTilePos();
    }

    public void addGold(float amount) { gold += amount; }
    public float getGold() { return gold; }
    public boolean spendGold(float amount) {
        if (gold >= amount) {
            gold -= amount;
            return true;
        }
        return false;
    }

    public void addFood(float amount) { food += amount; }
    public float getFood() { return food; }

    public void addWood(float amount) { wood += amount; }
    public float getWood() { return wood; }

    public int getTileX() { return tileX; }
    public int getTileY() { return tileY; }
    public float getFacingX() { return facingX; }
    public float getFacingY() { return facingY; }
}
