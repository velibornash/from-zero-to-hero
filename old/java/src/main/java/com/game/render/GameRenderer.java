package com.game.render;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.PerspectiveCamera;
import com.badlogic.gdx.graphics.g3d.Environment;
import com.badlogic.gdx.graphics.g3d.Model;
import com.badlogic.gdx.graphics.g3d.ModelBatch;
import com.badlogic.gdx.graphics.g3d.ModelInstance;
import com.badlogic.gdx.graphics.g3d.attributes.ColorAttribute;
import com.badlogic.gdx.graphics.g3d.environment.DirectionalLight;
import com.badlogic.gdx.graphics.g3d.utils.ModelBuilder;
import com.badlogic.gdx.math.Matrix4;
import com.badlogic.gdx.math.Vector3;
import com.game.buildings.Building;
import com.game.core.GameEngine;
import com.game.core.GameState;
import com.game.entities.Enemy;
import com.game.entities.Hero;
import com.game.entities.ResourceNode;
import com.game.entities.Worker;
import com.game.world.TerrainType;
import com.game.world.Tile;
import com.game.world.WorldGrid;

import java.util.ArrayList;
import java.util.List;

public class GameRenderer {
    private static final Color[] TERRAIN_COLORS = {
        new Color(0.35f, 0.60f, 0.18f, 1), // GRASS
        new Color(0.55f, 0.35f, 0.15f, 1), // DIRT
        new Color(0.12f, 0.35f, 0.65f, 1), // WATER
        new Color(0.18f, 0.40f, 0.10f, 1), // FOREST
        new Color(0.45f, 0.45f, 0.45f, 1), // STONE
    };

    private static final Color BUILDING_WALL = new Color(0.50f, 0.30f, 0.10f, 1);
    private static final Color BUILDING_ROOF = new Color(0.65f, 0.18f, 0.12f, 1);
    private static final Color HERO_TUNIC = new Color(0.12f, 0.28f, 0.58f, 1);
    private static final Color HERO_SKIN = new Color(0.95f, 0.78f, 0.60f, 1);
    private static final Color HERO_HAT = new Color(0.75f, 0.12f, 0.08f, 1);
    private static final Color HERO_METAL = new Color(0.72f, 0.72f, 0.80f, 1);
    private static final Color HERO_SHIELD = new Color(0.65f, 0.12f, 0.12f, 1);
    private static final Color ENEMY_BODY = new Color(0.40f, 0.08f, 0.16f, 1);
    private static final Color ENEMY_SKIN = new Color(0.30f, 0.18f, 0.10f, 1);
    private static final Color WORKER_TUNIC = new Color(0.20f, 0.40f, 0.20f, 1);
    private static final Color WORKER_SKIN = new Color(0.90f, 0.75f, 0.55f, 1);
    private static final Color RESOURCE_GOLD = new Color(0.90f, 0.70f, 0.10f, 1);
    private static final Color RESOURCE_FOOD = new Color(0.85f, 0.55f, 0.10f, 1);
    private static final Color RESOURCE_WOOD = new Color(0.35f, 0.22f, 0.08f, 1);

    private static final Vector3 LIGHT_DIR = new Vector3(-0.5f, -0.8f, -0.3f).nor();
    private static final Color LIGHT_COLOR = new Color(0.9f, 0.85f, 0.75f, 1);
    private static final Color AMBIENT_COLOR = new Color(0.35f, 0.40f, 0.50f, 1);

    private final GameEngine engine;
    private final PerspectiveCamera camera;
    private final ModelBatch modelBatch;
    private final Environment environment;
    private final MaterialCache mats;

    private final Model[] sharedModels;
    private final List<ModelInstance> tileInstances;
    private final List<ModelInstance> dynamicInstances;

    public GameRenderer(GameEngine engine, PerspectiveCamera camera) {
        this.engine = engine;
        this.camera = camera;
        this.modelBatch = new ModelBatch();
        this.mats = new MaterialCache();
        this.dynamicInstances = new ArrayList<>();

        environment = new Environment();
        environment.set(new ColorAttribute(ColorAttribute.AmbientLight, AMBIENT_COLOR));
        environment.add(new DirectionalLight().set(LIGHT_COLOR, LIGHT_DIR));

        sharedModels = new Model[]{
            ModelFactory.tileModel(),
            ModelFactory.buildingWallsModel(),
            ModelFactory.buildingRoofModel(),
            ModelFactory.heroBodyModel(),
            ModelFactory.heroHeadModel(),
            ModelFactory.heroHatModel(),
            ModelFactory.swordModel(),
            ModelFactory.shieldModel(),
            ModelFactory.resourceModel(),
            ModelFactory.enemyBodyModel(),
            ModelFactory.enemyHeadModel(),
            ModelFactory.workerBodyModel(),
            ModelFactory.workerHeadModel(),
        };

        tileInstances = precreateTileInstances();
    }

    private List<ModelInstance> precreateTileInstances() {
        WorldGrid world = engine.getWorld();
        int gw = world.getWidth();
        int gh = world.getHeight();
        List<ModelInstance> list = new ArrayList<>(gw * gh);
        for (int tx = 0; tx < gw; tx++) {
            for (int ty = 0; ty < gh; ty++) {
                Tile tile = world.getTile(tx, ty);
                ModelInstance mi = new ModelInstance(sharedModels[0], tx, 0, ty);
                mi.materials.get(0).set(ColorAttribute.createDiffuse(terrainColor(tile)));
                list.add(mi);
            }
        }
        return list;
    }

    private static Color terrainColor(Tile tile) {
        if (tile == null) return TERRAIN_COLORS[0];
        return switch (tile.getTerrain()) {
            case GRASS -> TERRAIN_COLORS[0];
            case DIRT -> TERRAIN_COLORS[1];
            case WATER -> TERRAIN_COLORS[2];
            case FOREST -> TERRAIN_COLORS[3];
            case STONE -> TERRAIN_COLORS[4];
        };
    }

    public void render() {
        Gdx.gl.glEnable(GL20.GL_DEPTH_TEST);
        Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT | GL20.GL_DEPTH_BUFFER_BIT);
        Gdx.gl.glClearColor(0.55f, 0.65f, 0.90f, 1);

        camera.update();
        buildDynamicInstances();

        modelBatch.begin(camera);
        modelBatch.render(tileInstances, environment);
        modelBatch.render(dynamicInstances, environment);
        modelBatch.end();
    }

    private void buildDynamicInstances() {
        dynamicInstances.clear();
        WorldGrid world = engine.getWorld();

        for (ResourceNode node : world.getResourceNodes()) {
            if (!node.isAlive()) continue;
            float rx = node.getX() / 32f;
            float rz = node.getY() / 32f;
            var mi = new ModelInstance(sharedModels[8], rx, 0.15f, rz);
            Color c = switch (node.getType()) {
                case GOLD -> RESOURCE_GOLD;
                case FOOD -> RESOURCE_FOOD;
                default -> RESOURCE_WOOD;
            };
            mi.materials.get(0).set(ColorAttribute.createDiffuse(c));
            dynamicInstances.add(mi);
        }

        for (Building building : world.getBuildings()) {
            if (!building.isAlive()) continue;
            int bx = building.getTileX();
            int bz = building.getTileY();
            var walls = new ModelInstance(sharedModels[1], bx, 0.5f, bz);
            walls.materials.get(0).set(ColorAttribute.createDiffuse(BUILDING_WALL));
            dynamicInstances.add(walls);
            var roof = new ModelInstance(sharedModels[2], bx, 1.3f, bz);
            roof.materials.get(0).set(ColorAttribute.createDiffuse(BUILDING_ROOF));
            dynamicInstances.add(roof);
        }

        for (Worker worker : world.getWorkers()) {
            if (!worker.isAlive()) continue;
            addWorkerInstance(worker);
        }

        for (Enemy enemy : world.getEnemies()) {
            if (!enemy.isAlive()) continue;
            addEnemyInstance(enemy);
        }

        Hero hero = engine.getHero();
        if (hero != null && hero.isAlive()) {
            addHeroInstance(hero);
        }
    }

    private void addHeroInstance(Hero hero) {
        float hx = hero.getX() / 32f;
        float hz = hero.getY() / 32f;
        float fx = hero.getFacingX();
        float fy = hero.getFacingY();

        var body = new ModelInstance(sharedModels[3], hx, 0.45f, hz);
        body.materials.get(0).set(ColorAttribute.createDiffuse(HERO_TUNIC));
        dynamicInstances.add(body);

        var head = new ModelInstance(sharedModels[4], hx, 1.02f, hz);
        head.materials.get(0).set(ColorAttribute.createDiffuse(HERO_SKIN));
        dynamicInstances.add(head);

        var hat = new ModelInstance(sharedModels[5], hx, 1.25f, hz);
        hat.materials.get(0).set(ColorAttribute.createDiffuse(HERO_HAT));
        dynamicInstances.add(hat);

        float angle = (float) Math.atan2(fy, fx);
        float cos = (float) Math.cos(angle);
        float sin = (float) Math.sin(angle);

        var sword = new ModelInstance(sharedModels[6]);
        sword.transform.setToTranslation(hx + cos * 0.32f, 0.35f, hz + sin * 0.32f);
        sword.materials.get(0).set(ColorAttribute.createDiffuse(HERO_METAL));
        dynamicInstances.add(sword);

        var shield = new ModelInstance(sharedModels[7]);
        shield.transform.setToTranslation(hx - cos * 0.32f, 0.45f, hz - sin * 0.32f);
        shield.materials.get(0).set(ColorAttribute.createDiffuse(HERO_SHIELD));
        dynamicInstances.add(shield);
    }

    private void addEnemyInstance(Enemy enemy) {
        float ex = enemy.getX() / 32f;
        float ez = enemy.getY() / 32f;

        var body = new ModelInstance(sharedModels[9], ex, 0.35f, ez);
        body.materials.get(0).set(ColorAttribute.createDiffuse(ENEMY_BODY));
        dynamicInstances.add(body);

        var head = new ModelInstance(sharedModels[10], ex, 0.75f, ez);
        head.materials.get(0).set(ColorAttribute.createDiffuse(ENEMY_SKIN));
        dynamicInstances.add(head);
    }

    private void addWorkerInstance(Worker worker) {
        float wx = worker.getX() / 32f;
        float wz = worker.getY() / 32f;

        var body = new ModelInstance(sharedModels[11], wx, 0.35f, wz);
        body.materials.get(0).set(ColorAttribute.createDiffuse(WORKER_TUNIC));
        dynamicInstances.add(body);

        var head = new ModelInstance(sharedModels[12], wx, 0.75f, wz);
        head.materials.get(0).set(ColorAttribute.createDiffuse(WORKER_SKIN));
        dynamicInstances.add(head);
    }

    public void resize(int width, int height) {
        camera.viewportWidth = width;
        camera.viewportHeight = height;
        camera.update();
    }

    public void dispose() {
        modelBatch.dispose();
        for (Model m : sharedModels) m.dispose();
    }

    private static class MaterialCache {
        // placeholder for future material caching
    }
}
