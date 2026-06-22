package com.game.buildings;

import com.game.resources.ResourceType;

import java.util.Collections;
import java.util.Map;

public class BuildingType {
    private final String id;
    private final String name;
    private final String description;
    private final int workerSlots;
    private final Map<ResourceType, Float> buildCost;
    private final Map<ResourceType, Float> production;
    private final float maxHp;

    public BuildingType(String id, String name, String description, int workerSlots,
                        Map<ResourceType, Float> buildCost, Map<ResourceType, Float> production,
                        float maxHp) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.workerSlots = workerSlots;
        this.buildCost = buildCost;
        this.production = production;
        this.maxHp = maxHp;
    }

    public String getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
    public int getWorkerSlots() { return workerSlots; }
    public Map<ResourceType, Float> getBuildCost() { return Collections.unmodifiableMap(buildCost); }
    public Map<ResourceType, Float> getProduction() { return Collections.unmodifiableMap(production); }
    public float getMaxHp() { return maxHp; }
}
