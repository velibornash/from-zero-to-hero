package com.game.events;

public class Event {
    private final String id;
    private final String name;
    private final String description;
    private final EventType type;

    public Event(String id, String name, String description, EventType type) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
    }

    public Event(String id) {
        this(id, "", "", EventType.STORY);
    }

    public String getId() { return id; }
    public String getName() { return name; }
    public String getDescription() { return description; }
    public EventType getType() { return type; }

    public enum EventType {
        STORY,
        COMBAT,
        ECONOMIC,
        MYTHICAL,
        SYSTEM
    }
}
