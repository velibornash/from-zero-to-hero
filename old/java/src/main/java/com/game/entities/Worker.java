package com.game.entities;

import com.game.resources.ResourceType;

public class Worker extends Entity {
    public enum State {
        IDLE,
        MOVING_TO_RESOURCE,
        GATHERING,
        MOVING_TO_BASE,
        MOVING_TO_BUILD,
        MOVING_COMMANDED,
        BUILDING
    }

    private static final float BASE_SPEED = 60f;
    private static final float BASE_GATHER_TIME = 2f;
    private static final float CARRY_CAPACITY = 10f;
    private static final float BASE_X = 25f * 32f;
    private static final float BASE_Y = 25f * 32f;

    private State state;
    private ResourceType carryingType;
    private float carryAmount;
    private float taskTimer;
    private float targetX;
    private float targetY;
    private ResourceNode targetNode;
    private boolean commanded;

    public Worker(float x, float y) {
        super(x, y, 20f);
        this.state = State.IDLE;
        this.carryAmount = 0;
        this.carryingType = null;
        this.commanded = false;
    }

    public State getState() { return state; }
    public void setState(State state) { this.state = state; }
    public ResourceType getCarryingType() { return carryingType; }
    public float getCarryAmount() { return carryAmount; }
    public float getTargetX() { return targetX; }
    public float getTargetY() { return targetY; }
    public boolean isCommanded() { return commanded; }

    public void setTarget(float x, float y) {
        this.targetX = x;
        this.targetY = y;
    }

    public void setTargetNode(ResourceNode node) {
        this.targetNode = node;
        if (node != null) {
            setTarget(node.getX(), node.getY());
        }
    }

    public ResourceNode getTargetNode() { return targetNode; }

    public void commandMove(float worldX, float worldY) {
        state = State.MOVING_COMMANDED;
        setTarget(worldX, worldY);
        this.commanded = true;
    }

    public void update(float dt, float moraleSpeedModifier, boolean heroicSurge) {
        if (!alive) return;

        float speed = BASE_SPEED * moraleSpeedModifier;
        if (heroicSurge) speed *= 1.5f;

        switch (state) {
            case IDLE -> {}
            case MOVING_TO_RESOURCE -> updateMoveToResource(dt, speed);
            case GATHERING -> updateGathering(dt);
            case MOVING_TO_BASE -> updateMoveToBase(dt, speed);
            case MOVING_TO_BUILD -> updateMoveToBuild(dt, speed);
            case MOVING_COMMANDED -> updateMoveCommanded(dt, speed);
            case BUILDING -> updateBuilding(dt);
        }
    }

    private void updateMoveToResource(float dt, float speed) {
        if (targetNode == null || !targetNode.isAlive()) {
            state = State.IDLE;
            commanded = false;
            return;
        }
        setTarget(targetNode.getX(), targetNode.getY());
        if (moveToward(targetX, targetY, speed * dt)) {
            state = State.GATHERING;
            taskTimer = 0;
        }
    }

    private void updateGathering(float dt) {
        if (targetNode == null || !targetNode.isAlive()) {
            state = State.IDLE;
            commanded = false;
            return;
        }
        taskTimer += dt;
        if (taskTimer >= BASE_GATHER_TIME) {
            float gathered = Math.min(CARRY_CAPACITY, targetNode.getAmount());
            targetNode.harvest(gathered);
            carryingType = targetNode.getType();
            carryAmount = gathered;
            state = State.MOVING_TO_BASE;
            setTarget(BASE_X, BASE_Y);
        }
    }

    private void updateMoveToBase(float dt, float speed) {
        setTarget(BASE_X, BASE_Y);
        if (moveToward(BASE_X, BASE_Y, speed * dt)) {
            depositResources();
            state = State.IDLE;
            commanded = false;
        }
    }

    private void updateMoveToBuild(float dt, float speed) {
        if (moveToward(targetX, targetY, speed * dt)) {
            state = State.BUILDING;
            taskTimer = 0;
        }
    }

    private void updateMoveCommanded(float dt, float speed) {
        if (moveToward(targetX, targetY, speed * dt)) {
            state = State.IDLE;
            commanded = false;
        }
    }

    private void updateBuilding(float dt) {
        taskTimer += dt;
        if (taskTimer >= 3f) {
            state = State.IDLE;
            commanded = false;
        }
    }

    private boolean moveToward(float tx, float ty, float speed) {
        float dx = tx - x;
        float dy = ty - y;
        float dist = (float) Math.sqrt(dx * dx + dy * dy);

        if (dist < 2f) {
            x = tx;
            y = ty;
            return true;
        }

        float step = Math.min(speed, dist);
        x += (dx / dist) * step;
        y += (dy / dist) * step;
        return false;
    }

    private void depositResources() {
        carryAmount = 0;
        carryingType = null;
    }
}
