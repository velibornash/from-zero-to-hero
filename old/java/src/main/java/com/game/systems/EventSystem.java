package com.game.systems;

import com.game.core.GameEngine;
import com.game.core.GameState;
import com.game.events.Event;
import com.game.events.EventDispatcher;
import com.game.events.EventQueue;
import com.game.events.EventRegistry;
import com.game.events.GameEvent;
import com.game.world.WorldGrid;

import java.util.function.Consumer;

public class EventSystem implements Consumer<Event> {
    private final WorldGrid world;
    private final GameState gameState;
    private final EventRegistry registry;
    private final EventQueue eventQueue;
    private GameEngine engine;
    private float checkTimer;

    public EventSystem(WorldGrid world, GameState gameState, EventRegistry registry, EventQueue eventQueue) {
        this.world = world;
        this.gameState = gameState;
        this.registry = registry;
        this.eventQueue = eventQueue;
        this.checkTimer = 0;
    }

    public void setEngine(GameEngine engine) {
        this.engine = engine;
    }

    public void update(float dt) {
        checkTimer += dt;
        if (checkTimer >= 1f) {
            checkTimer = 0;
            checkEvents();
        }
    }

    private void checkEvents() {
        if (engine == null) return;

        for (String eventId : registry.getEventIds()) {
            GameEvent event = registry.get(eventId);
            if (event == null || event.isTriggered() || !event.isUnlocked()) continue;

            if (event.checkConditions(engine)) {
                event.execute(engine);
                gameState.completeEvent(eventId);
                eventQueue.add(new Event(eventId, event.getName(), event.getDescription(), event.getType()));

                String nextEvent = getNextEvent(eventId);
                if (nextEvent != null) {
                    registry.unlock(nextEvent);
                }
            }
        }
    }

    private String getNextEvent(String currentId) {
        return switch (currentId) {
            case "prvi_oganj" -> "vukovi_rtanj";
            case "vukovi_rtanj" -> "senka_kosancica";
            case "senka_kosancica" -> "zmaj_planina";
            case "zmaj_planina" -> "kraljevic_marko";
            default -> null;
        };
    }

    @Override
    public void accept(Event event) {}
}
