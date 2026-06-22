package com.game;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input;
import com.badlogic.gdx.graphics.PerspectiveCamera;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.math.collision.Ray;
import com.game.core.GameEngine;
import com.game.entities.Hero;
import com.game.render.GameRenderer;
import com.game.render.GameUI;

public class FromZeroToHero extends ApplicationAdapter {
    private PerspectiveCamera camera;
    private GameEngine engine;
    private GameRenderer renderer;
    private GameUI gameUI;
    private final Vector3 cameraTarget = new Vector3();
    private final Vector3 heroPos3D = new Vector3();

    @Override
    public void create() {
        camera = new PerspectiveCamera(60, 1280, 720);
        camera.near = 0.1f;
        camera.far = 200f;

        float cx = 25f;
        float cz = 25f;
        camera.position.set(cx + 12, 10, cz + 12);
        camera.lookAt(cx, 0, cz);
        camera.update();

        engine = new GameEngine();
        renderer = new GameRenderer(engine, camera);
        gameUI = new GameUI(engine);
    }

    @Override
    public void render() {
        float delta = Gdx.graphics.getDeltaTime();
        engine.update(delta);
        handleInput(delta);
        doMoveHero(delta);
        engine.checkHeroCollection();

        Hero hero = engine.getHero();
        heroPos3D.set(hero.getX() / 32f, 0, hero.getY() / 32f);

        cameraTarget.set(heroPos3D.x + 10, 8, heroPos3D.z + 10);
        camera.position.lerp(cameraTarget, 0.05f);
        camera.lookAt(heroPos3D.x, 0.3f, heroPos3D.z);
        camera.update();

        renderer.render();

        gameUI.update();
        gameUI.render();
    }

    private void doMoveHero(float delta) {
        Hero hero = engine.getHero();
        float mx = 0, my = 0;
        if (Gdx.input.isKeyPressed(Input.Keys.A) || Gdx.input.isKeyPressed(Input.Keys.LEFT)) mx -= 1;
        if (Gdx.input.isKeyPressed(Input.Keys.D) || Gdx.input.isKeyPressed(Input.Keys.RIGHT)) mx += 1;
        if (Gdx.input.isKeyPressed(Input.Keys.W) || Gdx.input.isKeyPressed(Input.Keys.UP)) my += 1;
        if (Gdx.input.isKeyPressed(Input.Keys.S) || Gdx.input.isKeyPressed(Input.Keys.DOWN)) my -= 1;
        if (mx != 0 || my != 0) {
            hero.move(mx, my, delta);
        }
    }

    private void handleInput(float delta) {
        if (Gdx.input.isKeyJustPressed(Input.Keys.ESCAPE)) {
            if (engine.getGameState().isBuildMode()) {
                engine.getGameState().setBuildMode(false);
            } else {
                Gdx.app.exit();
            }
        }

        if (Gdx.input.isKeyJustPressed(Input.Keys.B)) {
            boolean was = engine.getGameState().isBuildMode();
            engine.getGameState().setBuildMode(!was);
        }

        if (engine.getGameState().isBuildMode()) {
            int[] numKeys = {Input.Keys.NUM_1, Input.Keys.NUM_2, Input.Keys.NUM_3, Input.Keys.NUM_4,
                             Input.Keys.NUM_5, Input.Keys.NUM_6, Input.Keys.NUM_7, Input.Keys.NUM_8};
            for (int i = 0; i < numKeys.length; i++) {
                if (Gdx.input.isKeyJustPressed(numKeys[i])) {
                    engine.getGameState().setSelectedBuildingIndex(
                        i < engine.getBuildingTypes().size() ? i : -1);
                }
            }

            if (Gdx.input.isKeyJustPressed(Input.Keys.E)) {
                engine.buildOnHeroTile();
            }
        }

        if (Gdx.input.isButtonJustPressed(Input.Buttons.LEFT)) {
            handleLeftClick();
        }
        if (Gdx.input.isButtonJustPressed(Input.Buttons.RIGHT)) {
            handleRightClick();
        }
    }

    private int[] screenToTile() {
        Ray ray = camera.getPickRay(Gdx.input.getX(), Gdx.input.getY());
        float t = -ray.origin.y / ray.direction.y;
        if (t < 0) return new int[]{-1, -1};
        float wx = ray.origin.x + t * ray.direction.x;
        float wz = ray.origin.z + t * ray.direction.z;
        int tx = (int) wx;
        int ty = (int) wz;
        return new int[]{tx, ty};
    }

    private void handleLeftClick() {
        int[] tile = screenToTile();
        int tx = tile[0], ty = tile[1];
        if (tx < 0 || tx >= 50 || ty < 0 || ty >= 50) return;
        engine.getGameState().setSelectedTile(tx, ty);
    }

    private void handleRightClick() {
        int[] tile = screenToTile();
        int tx = tile[0], ty = tile[1];
        if (tx < 0 || tx >= 50 || ty < 0 || ty >= 50) return;
        engine.commandWorker(tx, ty);
    }

    @Override
    public void resize(int width, int height) {
        camera.viewportWidth = width;
        camera.viewportHeight = height;
        camera.update();
        gameUI.resize(width, height);
    }

    @Override
    public void dispose() {
        renderer.dispose();
        gameUI.dispose();
    }
}
