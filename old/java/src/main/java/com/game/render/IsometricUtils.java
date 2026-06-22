package com.game.render;

import com.game.world.WorldGrid;

public class IsometricUtils {
    public static final float ISO_TILE_W = 64;
    public static final float ISO_TILE_H = 32;
    public static final float HALF_W = ISO_TILE_W / 2f;
    public static final float HALF_H = ISO_TILE_H / 2f;

    public static float gridToIsoX(float tx, float ty) {
        return (tx - ty) * HALF_W;
    }

    public static float gridToIsoY(float tx, float ty) {
        return (tx + ty) * HALF_H;
    }

    public static float worldToIsoX(float wx, float wy) {
        return gridToIsoX(wx / WorldGrid.TILE_SIZE, wy / WorldGrid.TILE_SIZE);
    }

    public static float worldToIsoY(float wx, float wy) {
        return gridToIsoY(wx / WorldGrid.TILE_SIZE, wy / WorldGrid.TILE_SIZE);
    }

    public static float isoToGridX(float isoX, float isoY) {
        return (isoX / HALF_W + isoY / HALF_H) / 2f;
    }

    public static float isoToGridY(float isoX, float isoY) {
        return (isoY / HALF_H - isoX / HALF_W) / 2f;
    }
}
