package com.game.ai;

import com.game.entities.Enemy;
import com.game.world.WorldGrid;

public class BasicAI {

    public static void updateEnemy(Enemy enemy, WorldGrid world, float dt) {
        if (!enemy.isAlive()) return;

        float centerX = 25 * WorldGrid.TILE_SIZE;
        float centerY = 25 * WorldGrid.TILE_SIZE;

        float dx = centerX - enemy.getX();
        float dy = centerY - enemy.getY();
        float dist = (float) Math.sqrt(dx * dx + dy * dy);

        if (dist > 10f) {
            float speed = 30f * dt;
            enemy.setX(enemy.getX() + (dx / dist) * speed);
            enemy.setY(enemy.getY() + (dy / dist) * speed);
        }
    }
}
