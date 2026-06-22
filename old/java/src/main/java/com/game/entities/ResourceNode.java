package com.game.entities;

import com.game.resources.ResourceType;

public class ResourceNode extends Entity {
    private final ResourceType type;
    private float amount;
    private final float maxAmount;

    public ResourceNode(float x, float y, ResourceType type, float amount) {
        super(x, y, 50f);
        this.type = type;
        this.amount = amount;
        this.maxAmount = amount;
    }

    public ResourceType getType() { return type; }
    public float getAmount() { return amount; }
    public float getMaxAmount() { return maxAmount; }

    public void harvest(float amount) {
        this.amount = Math.max(0, this.amount - amount);
        if (this.amount <= 0) {
            this.alive = false;
        }
    }

    @Override
    protected void onDeath() {
        amount = 0;
    }
}
