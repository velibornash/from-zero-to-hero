package com.game.events;

import com.game.core.GameEngine;
import com.game.resources.ResourceType;

import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Set;

public class EventRegistry {
    private final Map<String, GameEvent> events;

    public EventRegistry() {
        this.events = new LinkedHashMap<>();
    }

    public void register(GameEvent event) {
        events.put(event.getId(), event);
    }

    public GameEvent get(String id) {
        return events.get(id);
    }

    public void unlock(String id) {
        GameEvent event = events.get(id);
        if (event != null) {
            event.setUnlocked(true);
        }
    }

    public Set<String> getEventIds() {
        return events.keySet();
    }

    public void registerEvents(EventDispatcher dispatcher) {
        // 1. "Prvi oganj sela" - starts unlocked, triggers immediately
        register(new GameEvent("prvi_oganj", "Prvi oganj sela",
                "Na rubu šume, mala grupa ljudi podiže prvo naselje. Kažu da je tu nekad \"nešto staro\" spaljeno do temelja. Noću se čuju šapati iz zemlje.",
                Event.EventType.STORY)
                .setUnlocked(true)
                .addCondition(engine -> true)
                .addEffect(engine -> {
                    engine.getGameState().unlockBuilding("basic_hut");
                    engine.getGameState().unlockBuilding("woodcutting");
                    engine.getGameState().addResource(ResourceType.WOOD, 30);
                    engine.getGameState().addResource(ResourceType.FOOD, 40);
                    engine.getGameState().addMorale(10);
                }));

        // 2. "Vukovi sa Rtnja" - triggers after 30s and event 1 completed
        register(new GameEvent("vukovi_rtanj", "Vukovi sa Rtnja",
                "Vukovi napadaju selo, ali nisu obični — kreću se organizovano, kao da ih neko vodi. Stariji kažu: \"To nije čopor… to je znak.\"",
                Event.EventType.COMBAT)
                .addCondition(engine ->
                        engine.getGameState().getTotalTime() > 30f &&
                        engine.getGameState().isEventCompleted("prvi_oganj"))
                .addEffect(engine -> {
                    engine.getGameState().unlockBuilding("watchtower");
                    engine.getWorld().addEnemyWave("wolves", 5);
                }));

        // 3. "Senka Kosančića Ivana" - triggers after combat
        register(new GameEvent("senka_kosancica", "Senka Kosančića Ivana",
                "U snu jednog radnika pojavljuje se lik ratnika koji traži da se \"ne zaboravi čast\". Ujutru, radnici rade brže, ali sa čudnim osećajem težine.",
                Event.EventType.STORY)
                .addCondition(engine ->
                        engine.getGameState().getTotalTime() > 60f &&
                        engine.getGameState().isEventCompleted("vukovi_rtanj"))
                .addEffect(engine -> {
                    engine.getGameState().addMorale(15);
                    engine.getGameState().unlockBuilding("monastery");
                }));

        // 4. "Zmaj sa planine" - mid-game boss
        register(new GameEvent("zmaj_planina", "Zmaj sa planine (Legenda iznad sela)",
                "Planina iznad sela počinje da \"diše dim\". Ljudi kažu da je to zaspali zmaj koji čuva staru moć.",
                Event.EventType.MYTHICAL)
                .addCondition(engine ->
                        engine.getGameState().getTotalTime() > 120f &&
                        engine.getGameState().isEventCompleted("senka_kosancica"))
                .addEffect(engine -> {
                    engine.getWorld().addEnemyWave("boss", 1);
                    engine.getGameState().addResource(ResourceType.GOLD, 100);
                    engine.getGameState().addResource(ResourceType.STONE, 50);
                    engine.getGameState().unlockBuilding("mythic_slot");
                }));

        // 5. "Dolazak Kraljevića Marka" - save system when base is low
        register(new GameEvent("kraljevic_marko", "Dolazak Kraljevića Marka",
                "Kad je selo na ivici propasti, konj udara o zemlju u daljini. Kraljević Marko dolazi — sila koja se pojavljuje kad je najteže.",
                Event.EventType.MYTHICAL)
                .addCondition(engine ->
                        engine.getGameState().isEventCompleted("zmaj_planina") &&
                        engine.getGameState().getBaseHp() < 30f)
                .addEffect(engine -> {
                    engine.getGameState().healBase(50);
                    engine.getGameState().addMorale(30);
                    // AoE damage to enemies near center
                    engine.getWorld().getEnemies().forEach(e -> {
                        float dx = e.getX() - 25 * 32;
                        float dy = e.getY() - 25 * 32;
                        float dist = (float) Math.sqrt(dx * dx + dy * dy);
                        if (dist < 150) {
                            e.takeDamage(50);
                        }
                    });
                }));
    }
}
