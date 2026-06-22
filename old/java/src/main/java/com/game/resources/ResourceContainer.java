package com.game.resources;

import java.util.HashMap;
import java.util.Map;

public class ResourceContainer {
    private final Map<ResourceType, Float> resources;

    public ResourceContainer() {
        this.resources = new HashMap<>();
        for (ResourceType type : ResourceType.values()) {
            resources.put(type, 0f);
        }
    }

    public ResourceContainer(Map<ResourceType, Float> initial) {
        this();
        for (Map.Entry<ResourceType, Float> entry : initial.entrySet()) {
            resources.put(entry.getKey(), entry.getValue());
        }
    }

    public float get(ResourceType type) {
        return resources.get(type);
    }

    public void add(ResourceType type, float amount) {
        resources.merge(type, amount, Float::sum);
    }

    public boolean spend(ResourceType type, float amount) {
        if (resources.get(type) >= amount) {
            resources.merge(type, -amount, Float::sum);
            return true;
        }
        return false;
    }

    public boolean has(ResourceType type, float amount) {
        return resources.get(type) >= amount;
    }

    public boolean hasAll(Map<ResourceType, Float> costs) {
        return costs.entrySet().stream()
                .allMatch(e -> has(e.getKey(), e.getValue()));
    }

    public boolean spendAll(Map<ResourceType, Float> costs) {
        if (!hasAll(costs)) return false;
        costs.forEach(this::spend);
        return true;
    }

    public Map<ResourceType, Float> getAll() {
        return new HashMap<>(resources);
    }
}
