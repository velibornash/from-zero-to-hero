package com.game.events;

import com.game.core.GameEngine;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Consumer;
import java.util.function.Predicate;

public class GameEvent {
    private final String id;
    private final String name;
    private final String description;
    private final Event.EventType type;
    private boolean unlocked;
    private boolean triggered;
    private final List<Predicate<GameEngine>> conditions;
    private final List<Consumer<GameEngine>> effects;

    public GameEvent(String id, String name, String description, Event.EventType type) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.unlocked = false;
        this.triggered = false;
        this.conditions = new ArrayList<>();
        this.effects = new ArrayList<>();
    }

    public String getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
    public Event.EventType getType() { return type; }
    public boolean isUnlocked() { return unlocked; }
    public boolean isTriggered() { return triggered; }

    public GameEvent setUnlocked(boolean unlocked) {
        this.unlocked = unlocked;
        return this;
    }

    public GameEvent addCondition(Predicate<GameEngine> condition) {
        this.conditions.add(condition);
        return this;
    }

    public GameEvent addEffect(Consumer<GameEngine> effect) {
        this.effects.add(effect);
        return this;
    }

    public boolean checkConditions(GameEngine engine) {
        return conditions.stream().allMatch(c -> c.test(engine));
    }

    public void execute(GameEngine engine) {
        if (triggered) return;
        triggered = true;
        effects.forEach(e -> e.accept(engine));
    }
}
