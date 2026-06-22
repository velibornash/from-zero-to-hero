package com.game.entities;

public class Enemy extends Entity {
    private static final float SPEED = 30f;

    private final int damage;
    private final float attackCooldown;
    private float attackTimer;
    private Entity target;
    private float targetX;
    private float targetY;

    public Enemy(float x, float y, float hp, int damage) {
        super(x, y, hp);
        this.damage = damage;
        this.attackCooldown = 1f;
        this.attackTimer = 0f;
    }

    public int getDamage() { return damage; }

    public void setTargetPosition(float tx, float ty) {
        this.targetX = tx;
        this.targetY = ty;
    }

    public Entity getTarget() { return target; }
    public void setTarget(Entity target) { this.target = target; }

    public void update(float dt) {
        if (!alive) return;
        attackTimer += dt;

        if (target != null && target.isAlive()) {
            float dx = target.getX() - x;
            float dy = target.getY() - y;
            float dist = (float) Math.sqrt(dx * dx + dy * dy);

            if (dist < 20f) {
                if (attackTimer >= attackCooldown) {
                    target.takeDamage(damage);
                    attackTimer = 0;
                }
            } else {
                float step = Math.min(SPEED * dt, dist);
                x += (dx / dist) * step;
                y += (dy / dist) * step;
            }
        } else {
            float dx = targetX - x;
            float dy = targetY - y;
            float dist = (float) Math.sqrt(dx * dx + dy * dy);

            if (dist > 2f) {
                float step = Math.min(SPEED * dt, dist);
                x += (dx / dist) * step;
                y += (dy / dist) * step;
            }
        }
    }
}
