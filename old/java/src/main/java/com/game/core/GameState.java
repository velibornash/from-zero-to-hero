package com.game.core;

import com.game.resources.ResourceContainer;
import com.game.resources.ResourceType;

import java.util.HashSet;
import java.util.Set;

public class GameState {
    private final ResourceContainer resources;
    private int population;
    private int maxPopulation;
    private float morale;
    private float baseHp;
    private float maxBaseHp;
    private float totalTime;
    private final Set<String> unlockedBuildings;
    private final Set<String> completedEvents;
    private final Set<String> unlockedEvents;
    private int currentEra;

    private boolean buildMode;
    private int selectedBuildingIndex;
    private int hoverTileX;
    private int hoverTileY;
    private boolean hoverValid;
    private int selectedTileX;
    private int selectedTileY;

    private float heroicSurgeTimer;
    private static final float HEROIC_SURGE_DURATION = 10f;

    private String notificationMessage;
    private float notificationTimer;

    public GameState() {
        this.resources = new ResourceContainer();
        resources.add(ResourceType.WOOD, 50);
        resources.add(ResourceType.FOOD, 30);
        resources.add(ResourceType.GOLD, 10);
        this.population = 0;
        this.maxPopulation = 10;
        this.morale = 50f;
        this.baseHp = 100f;
        this.maxBaseHp = 100f;
        this.totalTime = 0f;
        this.unlockedBuildings = new HashSet<>();
        this.completedEvents = new HashSet<>();
        this.unlockedEvents = new HashSet<>();
        this.currentEra = 0;
        this.buildMode = false;
        this.selectedBuildingIndex = -1;
        this.hoverTileX = -1;
        this.hoverTileY = -1;
        this.selectedTileX = -1;
        this.selectedTileY = -1;
        this.heroicSurgeTimer = 0;
    }

    public ResourceContainer getResources() { return resources; }
    public int getPopulation() { return population; }
    public int getMaxPopulation() { return maxPopulation; }
    public float getMorale() { return morale; }
    public float getBaseHp() { return baseHp; }
    public float getMaxBaseHp() { return maxBaseHp; }
    public float getTotalTime() { return totalTime; }
    public int getCurrentEra() { return currentEra; }

    public void addResource(ResourceType type, float amount) { resources.add(type, amount); }
    public boolean spendResource(ResourceType type, float amount) { return resources.spend(type, amount); }

    public void addPopulation(int amount) { population += amount; }
    public void setMaxPopulation(int max) { this.maxPopulation = max; }

    public void setMorale(float value) { morale = Math.min(100, Math.max(0, value)); }
    public void addMorale(float amount) { morale = Math.min(100, Math.max(0, morale + amount)); }

    public float getMoraleSpeedModifier() {
        return 0.5f + (morale / 100f) * 0.5f;
    }

    public float getMoraleCombatModifier() {
        return 0.5f + (morale / 100f) * 0.5f;
    }

    public void damageBase(float amount) { baseHp = Math.max(0, baseHp - amount); }
    public void healBase(float amount) { baseHp = Math.min(maxBaseHp, baseHp + amount); }

    public boolean isBaseDestroyed() { return baseHp <= 0; }

    public void update(float dt) {
        totalTime += dt;
        if (heroicSurgeTimer > 0) {
            heroicSurgeTimer -= dt;
        }
        if (notificationTimer > 0) {
            notificationTimer -= dt;
            if (notificationTimer <= 0) {
                notificationMessage = null;
            }
        }
    }

    public void unlockBuilding(String id) { unlockedBuildings.add(id); }
    public boolean isBuildingUnlocked(String id) { return unlockedBuildings.contains(id); }

    public void completeEvent(String id) { completedEvents.add(id); }
    public boolean isEventCompleted(String id) { return completedEvents.contains(id); }

    public void unlockEvent(String id) { unlockedEvents.add(id); }
    public boolean isEventUnlocked(String id) { return unlockedEvents.contains(id); }

    public Set<String> getCompletedEvents() { return completedEvents; }
    public Set<String> getUnlockedBuildings() { return unlockedBuildings; }

    public boolean isBuildMode() { return buildMode; }
    public void setBuildMode(boolean b) {
        this.buildMode = b;
        if (!b) selectedBuildingIndex = -1;
    }
    public int getSelectedBuildingIndex() { return selectedBuildingIndex; }
    public void setSelectedBuildingIndex(int i) { this.selectedBuildingIndex = i; }

    public int getHoverTileX() { return hoverTileX; }
    public int getHoverTileY() { return hoverTileY; }
    public void setHoverTile(int x, int y) { this.hoverTileX = x; this.hoverTileY = y; }
    public boolean isHoverValid() { return hoverValid; }
    public void setHoverValid(boolean v) { this.hoverValid = v; }

    public int getSelectedTileX() { return selectedTileX; }
    public int getSelectedTileY() { return selectedTileY; }
    public void setSelectedTile(int x, int y) { this.selectedTileX = x; this.selectedTileY = y; }

    public void activateHeroicSurge() {
        heroicSurgeTimer = HEROIC_SURGE_DURATION;
        addMorale(20);
        showNotification("HEROIC SURGE: Radnici su nadahnuti!");
    }

    public boolean isHeroicSurgeActive() { return heroicSurgeTimer > 0; }
    public float getHeroicSurgeTimer() { return heroicSurgeTimer; }

    public String getNotificationMessage() { return notificationMessage; }
    public void showNotification(String msg) {
        this.notificationMessage = msg;
        this.notificationTimer = 4f;
    }
}
