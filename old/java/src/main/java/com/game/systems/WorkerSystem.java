package com.game.systems;

import com.game.core.GameState;
import com.game.entities.ResourceNode;
import com.game.entities.Worker;
import com.game.events.Event;
import com.game.world.WorldGrid;

import java.util.Comparator;
import java.util.List;
import java.util.function.Consumer;

public class WorkerSystem implements Consumer<Event> {
    private final WorldGrid world;
    private final GameState gameState;

    public WorkerSystem(WorldGrid world, GameState gameState) {
        this.world = world;
        this.gameState = gameState;
    }

    public void update(float dt) {
        float moraleMod = gameState.getMoraleSpeedModifier();
        boolean heroic = gameState.isHeroicSurgeActive();

        for (Worker worker : world.getWorkers()) {
            if (!worker.isAlive()) continue;

            if (worker.getState() == Worker.State.IDLE && !worker.isCommanded()) {
                assignTask(worker);
            }

            worker.update(dt, moraleMod, heroic);
        }

        if (gameState.getMorale() > 70 && Math.random() < 0.001f * dt * 60) {
            gameState.activateHeroicSurge();
        }

        world.clearDeadEntities();
    }

    private void assignTask(Worker worker) {
        ResourceNode nearest = findNearestResource(worker);
        if (nearest != null) {
            worker.setTargetNode(nearest);
            worker.setState(Worker.State.MOVING_TO_RESOURCE);
        }
    }

    private ResourceNode findNearestResource(Worker worker) {
        List<ResourceNode> nodes = world.getResourceNodes();
        return nodes.stream()
                .filter(ResourceNode::isAlive)
                .min(Comparator.comparingDouble(n -> {
                    float dx = n.getX() - worker.getX();
                    float dy = n.getY() - worker.getY();
                    return dx * dx + dy * dy;
                }))
                .orElse(null);
    }

    public void commandWorkerTo(int tileX, int tileY) {
        List<Worker> workers = world.getWorkers();
        Worker nearest = null;
        float minDist = Float.MAX_VALUE;
        float wx = tileX * WorldGrid.TILE_SIZE + WorldGrid.TILE_SIZE / 2;
        float wy = tileY * WorldGrid.TILE_SIZE + WorldGrid.TILE_SIZE / 2;

        for (Worker w : workers) {
            if (!w.isAlive()) continue;
            float dx = w.getX() - wx;
            float dy = w.getY() - wy;
            float dist = dx * dx + dy * dy;
            if (dist < minDist) {
                minDist = dist;
                nearest = w;
            }
        }
        if (nearest != null) {
            nearest.commandMove(wx, wy);
        }
    }

    @Override
    public void accept(Event event) {}
}
