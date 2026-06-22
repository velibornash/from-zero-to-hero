package com.game.systems;

import com.game.buildings.Building;
import com.game.core.GameState;
import com.game.events.Event;
import com.game.resources.ResourceType;
import com.game.world.WorldGrid;

import java.util.function.Consumer;

public class ResourceSystem implements Consumer<Event> {
    private static final float WORKER_FOOD_CONSUMPTION = 1f;
    private static final float TICK_INTERVAL = 1f;

    private final WorldGrid world;
    private final GameState gameState;
    private float tickTimer;

    public ResourceSystem(WorldGrid world, GameState gameState) {
        this.world = world;
        this.gameState = gameState;
        this.tickTimer = 0;
    }

    public void update(float dt) {
        tickTimer += dt;
        while (tickTimer >= TICK_INTERVAL) {
            tickTimer -= TICK_INTERVAL;
            tick();
        }
    }

    private void tick() {
        int workerCount = world.getWorkers().size();
        float foodCost = workerCount * WORKER_FOOD_CONSUMPTION;
        float foodAvailable = gameState.getResources().get(ResourceType.FOOD);

        if (foodAvailable >= foodCost) {
            gameState.spendResource(ResourceType.FOOD, foodCost);
        } else {
            float shortage = foodCost - foodAvailable;
            gameState.spendResource(ResourceType.FOOD, foodAvailable);
            gameState.addMorale(-shortage * 2);
        }

        for (Building building : world.getBuildings()) {
            if (!building.isAlive()) continue;
            String bid = building.getType().getId();
            if ("kafana".equals(bid)) {
                gameState.addMorale(1f);
            } else if ("monastery".equals(bid)) {
                gameState.addMorale(2f);
            }
            building.getProduction().forEach((resourceType, amount) -> {
                float production = amount;
                if (gameState.getMorale() > 70) {
                    production *= 1.2f;
                } else if (gameState.getMorale() < 20) {
                    production *= 0.5f;
                }
                gameState.addResource(resourceType, production);
            });
        }
    }

    @Override
    public void accept(Event event) {
        // Handle resource-related events if needed
    }
}
