package com.game.events;

import java.util.LinkedList;
import java.util.Queue;

public class EventQueue {
    private final Queue<Event> queue;

    public EventQueue() {
        this.queue = new LinkedList<>();
    }

    public void add(Event event) {
        queue.add(event);
    }

    public void add(String eventId) {
        queue.add(new Event(eventId));
    }

    public Event poll() {
        return queue.poll();
    }

    public boolean isEmpty() {
        return queue.isEmpty();
    }

    public int size() {
        return queue.size();
    }

    public void process(EventDispatcher dispatcher) {
        while (!queue.isEmpty()) {
            Event event = queue.poll();
            dispatcher.dispatch(event);
        }
    }
}
