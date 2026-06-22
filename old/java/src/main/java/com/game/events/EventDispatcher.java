package com.game.events;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Consumer;

public class EventDispatcher {
    private final Map<String, List<Consumer<Event>>> listeners;

    public EventDispatcher() {
        this.listeners = new HashMap<>();
    }

    public void register(String eventId, Consumer<Event> listener) {
        listeners.computeIfAbsent(eventId, k -> new ArrayList<>()).add(listener);
    }

    public void registerGlobal(Consumer<Event> listener) {
        register("*", listener);
    }

    public void dispatch(Event event) {
        List<Consumer<Event>> specific = listeners.get(event.getId());
        if (specific != null) {
            specific.forEach(l -> l.accept(event));
        }
        List<Consumer<Event>> global = listeners.get("*");
        if (global != null) {
            global.forEach(l -> l.accept(event));
        }
    }
}
