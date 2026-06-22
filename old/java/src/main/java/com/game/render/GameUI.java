package com.game.render;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.badlogic.gdx.scenes.scene2d.Stage;
import com.badlogic.gdx.scenes.scene2d.ui.Container;
import com.badlogic.gdx.scenes.scene2d.ui.Label;
import com.badlogic.gdx.scenes.scene2d.ui.Label.LabelStyle;
import com.badlogic.gdx.scenes.scene2d.ui.ScrollPane;
import com.badlogic.gdx.scenes.scene2d.ui.Skin;
import com.badlogic.gdx.scenes.scene2d.ui.Table;
import com.badlogic.gdx.scenes.scene2d.utils.Drawable;
import com.badlogic.gdx.utils.viewport.FitViewport;
import com.game.buildings.BuildingType;
import com.game.core.GameEngine;
import com.game.core.GameState;
import com.game.entities.Hero;
import com.game.world.WorldGrid;

import java.util.List;

public class GameUI {
    private final Stage stage;
    private final Skin skin;
    private final GameEngine engine;
    private final LabelStyle whiteStyle;
    private final LabelStyle goldStyle;
    private final LabelStyle titleStyle;

    private Table hudTable;
    private Label goldLabel, foodLabel, woodLabel;
    private Label moraleLabel, popLabel, baseHpLabel, heroHpLabel;
    private Label buildHintLabel;

    private Table buildPanel;
    private Table tileInfoPanel;
    private Label tileInfoLabel;
    private Table notificationTable;
    private Label notificationLabel;
    private float notificationTimer;

    public GameUI(GameEngine engine) {
        this.engine = engine;
        this.stage = new Stage(new FitViewport(1280, 720));
        this.skin = new Skin(
            Gdx.files.internal("assets/skins/plain-james/plain-james-ui.json"),
            new TextureAtlas(Gdx.files.internal("assets/skins/plain-james/plain-james-ui.atlas"))
        );

        BitmapFont font = skin.getFont("font");
        BitmapFont titleFont = skin.getFont("title");
        whiteStyle = new LabelStyle(font, Color.WHITE);
        goldStyle = new LabelStyle(font, new Color(1f, 0.8f, 0.2f, 1));
        titleStyle = new LabelStyle(titleFont, new Color(1f, 0.9f, 0.4f, 1));

        buildHUD();
        buildBuildPanel();
        buildTileInfo();
        buildNotification();
    }

    private Drawable darkBg() {
        return skin.newDrawable("white", new Color(0, 0, 0, 0.7f));
    }

    private void buildHUD() {
        hudTable = new Table();
        hudTable.setFillParent(true);
        hudTable.top().left();
        hudTable.pad(12);
        hudTable.setBackground(darkBg());

        goldLabel = new Label("Zlato: 0", goldStyle);
        foodLabel = new Label("Hrana: 0", whiteStyle);
        woodLabel = new Label("Drvo: 0", whiteStyle);

        hudTable.add(goldLabel).padRight(24);
        hudTable.add(foodLabel).padRight(24);
        hudTable.add(woodLabel);
        hudTable.row().padTop(4);

        moraleLabel = new Label("Moral: 50/100", whiteStyle);
        popLabel = new Label("Radnici: 0", whiteStyle);
        baseHpLabel = new Label("Baza HP: 100", whiteStyle);

        hudTable.add(moraleLabel).padRight(24);
        hudTable.add(popLabel).padRight(24);
        hudTable.add(baseHpLabel);
        hudTable.row().padTop(4);

        heroHpLabel = new Label("Heroj HP: 100/100", whiteStyle);
        hudTable.add(heroHpLabel).colspan(3).left();

        stage.addActor(hudTable);
    }

    private void buildBuildPanel() {
        buildPanel = new Table();
        buildPanel.setFillParent(true);
        buildPanel.top().right();
        buildPanel.pad(12);
        buildPanel.setBackground(darkBg());
        buildPanel.setVisible(false);

        buildHintLabel = new Label("Pritisni E da gradite", whiteStyle);
        Label title = new Label("GRADNJA (B - izlaz)", titleStyle);

        buildPanel.add(title).padBottom(8);
        buildPanel.row();
        stage.addActor(buildPanel);
    }

    private void rebuildBuildList() {
        buildPanel.clearChildren();
        Label title = new Label("GRADNJA (B - izlaz)", titleStyle);
        buildPanel.add(title).padBottom(8);
        buildPanel.row();

        List<BuildingType> types = engine.getBuildingTypes();
        GameState state = engine.getGameState();
        int selected = state.getSelectedBuildingIndex();

        Table list = new Table();
        for (int i = 0; i < types.size(); i++) {
            BuildingType bt = types.get(i);
            if (!state.isBuildingUnlocked(bt.getId())) continue;

            boolean isSelected = (i == selected);
            Color textColor = isSelected ? new Color(1, 0.9f, 0.3f, 1) : Color.WHITE;
            LabelStyle itemStyle = new LabelStyle(skin.getFont("font"), textColor);

            Table item = new Table();
            if (isSelected) {
                Drawable sel = skin.newDrawable("round-white", new Color(0.2f, 0.35f, 0.6f, 0.5f));
                item.setBackground(sel);
            }
            item.pad(6);

            Label nameLabel = new Label((i + 1) + ": " + bt.getName(), itemStyle);
            Label costLabel = new Label(formatCost(bt), new LabelStyle(skin.getFont("font"), Color.LIGHT_GRAY));

            item.add(nameLabel).left();
            item.row();
            item.add(costLabel).left();
            list.add(item).width(280).padBottom(4);
            list.row();

            if (isSelected) {
                Label descLabel = new Label(bt.getDescription(), new LabelStyle(skin.getFont("font"), new Color(0.8f, 0.8f, 0.8f, 1)));
                descLabel.setWrap(true);
                list.add(descLabel).width(280).padBottom(8);
                list.row();
            }
        }

        ScrollPane scroll = new ScrollPane(list, skin);
        buildPanel.add(scroll).width(300).maxHeight(500);
        buildPanel.row().padTop(8);
        buildPanel.add(buildHintLabel);
    }

    private String formatCost(BuildingType bt) {
        StringBuilder sb = new StringBuilder();
        bt.getBuildCost().forEach((res, amt) -> {
            if (sb.length() > 0) sb.append("  ");
            sb.append(res.getDisplayName()).append(": ").append(amt.intValue());
        });
        return sb.toString();
    }

    private void buildTileInfo() {
        tileInfoPanel = new Table();
        tileInfoPanel.setFillParent(true);
        tileInfoPanel.top().right();
        tileInfoPanel.pad(12);
        tileInfoPanel.padTop(80);
        tileInfoPanel.setBackground(darkBg());
        tileInfoPanel.setVisible(false);

        tileInfoLabel = new Label("", whiteStyle);
        tileInfoLabel.setWrap(true);
        tileInfoPanel.add(tileInfoLabel).width(220);

        stage.addActor(tileInfoPanel);
    }

    private void buildNotification() {
        notificationTable = new Table();
        notificationTable.setFillParent(true);
        notificationTable.center().top().padTop(120);

        notificationLabel = new Label("", new LabelStyle(skin.getFont("title"), new Color(1f, 0.9f, 0.2f, 1)));
        notificationTable.add(notificationLabel);
        notificationTable.setVisible(false);

        stage.addActor(notificationTable);
    }

    public void update() {
        Hero hero = engine.getHero();
        GameState state = engine.getGameState();
        WorldGrid world = engine.getWorld();

        goldLabel.setText("Zlato: " + (int) hero.getGold());
        foodLabel.setText("Hrana: " + (int) hero.getFood());
        woodLabel.setText("Drvo: " + (int) hero.getWood());

        float morale = state.getMorale();
        Color moraleColor = morale < 30 ? Color.RED : morale < 60 ? Color.ORANGE : Color.GREEN;
        moraleLabel.setText("Moral: " + (int) morale + "/100");
        moraleLabel.setColor(moraleColor);

        popLabel.setText("Radnici: " + world.getWorkers().size());
        baseHpLabel.setText("Baza HP: " + (int) state.getBaseHp());
        heroHpLabel.setText("Heroj HP: " + (int) hero.getHp() + "/100");

        boolean buildMode = state.isBuildMode();
        buildPanel.setVisible(buildMode);
        if (buildMode) {
            rebuildBuildList();
        }

        int sx = state.getSelectedTileX();
        int sy = state.getSelectedTileY();
        if (sx >= 0 && sy >= 0 && !buildMode) {
            tileInfoPanel.setVisible(true);
            tileInfoLabel.setText(engine.getTileInfo(sx, sy));
        } else {
            tileInfoPanel.setVisible(false);
        }

        String msg = state.getNotificationMessage();
        if (msg != null) {
            notificationLabel.setText(msg);
            notificationTable.setVisible(true);
            notificationTimer = 3f;
        } else if (notificationTimer > 0) {
            notificationTimer -= Gdx.graphics.getDeltaTime();
            if (notificationTimer <= 0) {
                notificationTable.setVisible(false);
            }
        }
    }

    public void render() {
        stage.act(Gdx.graphics.getDeltaTime());
        stage.draw();
    }

    public void resize(int width, int height) {
        stage.getViewport().update(width, height, true);
    }

    public void dispose() {
        stage.dispose();
        skin.dispose();
    }

    public Stage getStage() {
        return stage;
    }
}
