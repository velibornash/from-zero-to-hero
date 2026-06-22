package com.game.systems;

import com.game.core.GameState;
import com.game.entities.Enemy;
import com.game.entities.Worker;
import com.game.events.Event;
import com.game.world.WorldGrid;

import java.util.List;
import java.util.Random;
import java.util.function.Consumer;

public class CombatSystem implements Consumer<Event> {
    private static final float WAVE_INTERVAL = 45f;
    private static final float ENEMY_CHECK_RADIUS = 200f;

    private final WorldGrid world;
    private final GameState gameState;
    private final Random random;
    private float waveTimer;
    private int waveCount;

    public CombatSystem(WorldGrid world, GameState gameState) {
        this.world = world;
        this.gameState = gameState;
        this.random = new Random();
        this.waveTimer = 60f;
        this.waveCount = 0;
    }

    public void update(float dt) {
        float moraleCombat = gameState.getMoraleCombatModifier();
        boolean heroic = gameState.isHeroicSurgeActive();

        for (Enemy enemy : world.getEnemies()) {
            if (!enemy.isAlive()) continue;

            Worker nearestWorker = findNearestWorker(enemy);
            if (nearestWorker != null) {
                float dx = nearestWorker.getX() - enemy.getX();
                float dy = nearestWorker.getY() - enemy.getY();
                if (dx * dx + dy * dy < ENEMY_CHECK_RADIUS * ENEMY_CHECK_RADIUS) {
                    enemy.setTarget(nearestWorker);
                } else {
                    enemy.setTarget(null);
                }
            }

            enemy.update(dt);

            float cx = 25 * 32;
            float cy = 25 * 32;
            if (enemy.getX() >= cx - 32 && enemy.getX() <= cx + 32 &&
                enemy.getY() >= cy - 32 && enemy.getY() <= cy + 32) {
                float dmg = enemy.getDamage();
                if (heroic) dmg *= 0.3f;
                gameState.damageBase(dmg * dt);
                if (heroic) {
                    enemy.takeDamage(20 * dt);
                } else {
                    enemy.takeDamage(10);
                }
            }
        }

        for (Worker worker : world.getWorkers()) {
            if (!worker.isAlive()) continue;
            for (Enemy enemy : world.getEnemies()) {
                if (!enemy.isAlive()) continue;
                float dx = worker.getX() - enemy.getX();
                float dy = worker.getY() - enemy.getY();
                if (dx * dx + dy * dy < 400) {
                    float dmg = 3 * moraleCombat * dt;
                    if (heroic) dmg *= 2f;
                    enemy.takeDamage(dmg);
                }
            }
        }

        for (var building : world.getBuildings()) {
            if (!building.isAlive()) continue;
            if (!"watchtower".equals(building.getType().getId())) continue;
            float bx = building.getTileX() * 32 + 16;
            float by = building.getTileY() * 32 + 16;
            for (Enemy enemy : world.getEnemies()) {
                if (!enemy.isAlive()) continue;
                float dx = enemy.getX() - bx;
                float dy = enemy.getY() - by;
                if (dx * dx + dy * dy < 150 * 150) {
                    enemy.takeDamage(5 * dt);
                }
            }
        }

        waveTimer -= dt;
        if (waveTimer <= 0 && world.getEnemies().isEmpty()) {
            waveCount++;
            waveTimer = Math.max(20, WAVE_INTERVAL - waveCount * 2);
            if (waveCount > 0 && waveCount % 2 == 0) {
                world.addEnemyWave("soldiers", 2 + waveCount / 2);
            }
        }

        world.clearDeadEntities();
    }

    private Worker findNearestWorker(Enemy enemy) {
        List<Worker> workers = world.getWorkers();
        if (workers.isEmpty()) return null;

        Worker nearest = null;
        float minDist = Float.MAX_VALUE;

        for (Worker w : workers) {
            if (!w.isAlive()) continue;
            float dx = w.getX() - enemy.getX();
            float dy = w.getY() - enemy.getY();
            float dist = dx * dx + dy * dy;
            if (dist < minDist) {
                minDist = dist;
                nearest = w;
            }
        }
        return nearest;
    }

    @Override
    public void accept(Event event) {
        if ("vukovi_rtanj".equals(event.getId())) {
            world.addEnemyWave("wolves", 5);
        }
    }
}
