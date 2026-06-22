package com.game.entities;

public abstract class Entity {
    private static int nextId = 1;

    protected final int id;
    protected float x;
    protected float y;
    protected float hp;
    protected float maxHp;
    protected boolean alive;

    public Entity(float x, float y, float maxHp) {
        this.id = nextId++;
        this.x = x;
        this.y = y;
        this.maxHp = maxHp;
        this.hp = maxHp;
        this.alive = true;
    }

    public int getId() { return id; }
    public float getX() { return x; }
    public float getY() { return y; }
    public void setX(float x) { this.x = x; }
    public void setY(float y) { this.y = y; }
    public void setPosition(float x, float y) { this.x = x; this.y = y; }
    public float getHp() { return hp; }
    public float getMaxHp() { return maxHp; }

    public void takeDamage(float amount) {
        hp -= amount;
        if (hp <= 0) {
            hp = 0;
            alive = false;
            onDeath();
        }
    }

    public boolean isAlive() { return alive; }

    protected void onDeath() {}
}
