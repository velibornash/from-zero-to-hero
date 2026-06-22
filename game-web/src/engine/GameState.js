// Centralni store stanja igre — jedan izvor istine
import { BUILDINGS } from '../data/buildings.js';
import { STORY_EVENTS } from '../data/events.js';

const INITIAL_STATE = {
  resources: { wood: 30, food: 20, gold: 10 },
  population: { current: 3, max: 5 },
  morale: 50,
  baseHP: 100,
  defense: 0,
  soldiers: 0,
  day: 1,
  era: 1,
  tick: 0,

  // Grid: 5x4 = 20 slotova
  grid: Array(20).fill(null).map((_, i) => ({
    id: i,
    building: null,       // tip zgrade
    constructing: false,
    buildProgress: 0,
    buildDuration: 0,
  })),

  // Aktivni eventi koji čekaju odgovor
  activeEvent: null,
  completedEvents: [],

  // Unlocked zgrade
  unlockedBuildings: Object.entries(BUILDINGS)
    .filter(([_, b]) => b.unlocked)
    .map(([id]) => id),

  // Notifikacije
  notifications: [],

  // Game over
  gameOver: false,
  gameOverReason: '',

  // Aktivni buffovi
  activeBuffs: [],
};

class GameStateManager {
  constructor() {
    this.state = { ...INITIAL_STATE, grid: INITIAL_STATE.grid.map(s => ({ ...s })) };
    this.listeners = [];
    this.tickInterval = null;
  }

  subscribe(fn) {
    this.listeners.push(fn);
    return () => { this.listeners = this.listeners.filter(l => l !== fn); };
  }

  notify() {
    const snapshot = this.getSnapshot();
    this.listeners.forEach(fn => fn(snapshot));
  }

  getSnapshot() {
    return JSON.parse(JSON.stringify(this.state));
  }

  start() {
    // Game tick: svakih 1000ms = 1 sekunda igre
    this.tickInterval = setInterval(() => this.tick(), 1000);
    // Inicijalni event
    setTimeout(() => this.triggerEvent('prvi_oganj'), 1500);
  }

  stop() {
    if (this.tickInterval) clearInterval(this.tickInterval);
  }

  tick() {
    if (this.state.gameOver || this.state.activeEvent) return;
    this.state.tick++;

    // Dan se menja svaki 60 ticks
    if (this.state.tick % 60 === 0) {
      this.state.day++;
      this.addNotification(`Dan ${this.state.day}`, 'day');
    }

    // Produkcija zgrada
    this.state.grid.forEach(slot => {
      if (!slot.building || slot.constructing) return;
      const bDef = BUILDINGS[slot.building];
      if (!bDef || !bDef.productionInterval) return;
      if (this.state.tick % bDef.productionInterval === 0) {
        this.applyProduction(bDef.produces);
      }
    });

    // Hrana konzumacija
    if (this.state.tick % 10 === 0) {
      const foodNeeded = this.state.population.current;
      if (this.state.resources.food >= foodNeeded) {
        this.state.resources.food -= foodNeeded;
      } else {
        this.state.morale = Math.max(0, this.state.morale - 5);
        this.addNotification('Nedostaje hrane! Moral opada.', 'warning');
      }
    }

    // Gradnja u toku
    this.state.grid.forEach(slot => {
      if (!slot.constructing) return;
      slot.buildProgress += 1;
      if (slot.buildProgress >= slot.buildDuration) {
        slot.constructing = false;
        this.addNotification(`${BUILDINGS[slot.building].name} izgrađena!`, 'success');
      }
    });

    // Provjera story eventa
    this.checkEventTriggers();

    // Provjera game over
    if (this.state.baseHP <= 0) {
      this.state.gameOver = true;
      this.state.gameOverReason = 'Grad je pao.';
    }
    if (this.state.morale <= 0) {
      this.state.gameOver = true;
      this.state.gameOverReason = 'Narod je izgubio volju za životom.';
    }

    // Buff timeri
    this.state.activeBuffs = this.state.activeBuffs.filter(b => {
      b.remaining -= 1;
      return b.remaining > 0;
    });

    this.notify();
  }

  applyProduction(produces) {
    Object.entries(produces).forEach(([res, amount]) => {
      if (res === 'morale') {
        this.state.morale = Math.min(100, this.state.morale + amount);
      } else if (res === 'defense') {
        this.state.defense += amount;
      } else if (res === 'soldiers') {
        this.state.soldiers += amount;
      } else if (this.state.resources[res] !== undefined) {
        this.state.resources[res] += amount;
      }
    });
  }

  checkEventTriggers() {
    const s = this.state;
    STORY_EVENTS.forEach(ev => {
      if (s.completedEvents.includes(ev.id)) return;
      if (s.activeEvent?.id === ev.id) return;
      const c = ev.triggerCondition;
      let triggered = false;
      if (c.type === 'buildings_count') {
        const built = s.grid.filter(slot => slot.building && !slot.constructing).length;
        triggered = built >= c.value;
      } else if (c.type === 'population') {
        triggered = s.population.current >= c.value;
      } else if (c.type === 'base_hp_below') {
        triggered = s.baseHP / 100 <= c.value;
      }
      if (triggered) this.triggerEvent(ev.id);
    });
  }

  triggerEvent(eventId) {
    const ev = STORY_EVENTS.find(e => e.id === eventId);
    if (!ev) return;
    this.state.activeEvent = ev;
    this.notify();
  }

  resolveEvent(choiceId = null) {
    const ev = this.state.activeEvent;
    if (!ev) return;

    let effects = ev.effects;
    if (ev.choices && choiceId) {
      const choice = ev.choices.find(c => c.id === choiceId);
      if (choice) effects = choice.effects;
    }

    if (effects) {
      if (effects.resources) {
        Object.entries(effects.resources).forEach(([r, v]) => {
          this.state.resources[r] = (this.state.resources[r] || 0) + v;
        });
      }
      if (effects.morale) this.state.morale = Math.min(100, this.state.morale + effects.morale);
      if (effects.unlockBuildings) {
        effects.unlockBuildings.forEach(b => {
          if (!this.state.unlockedBuildings.includes(b)) {
            this.state.unlockedBuildings.push(b);
          }
        });
      }
      if (effects.tempBuff) {
        this.state.activeBuffs.push({
          ...effects.tempBuff,
          remaining: effects.tempBuff.duration,
        });
      }
      if (effects.healBase) {
        this.state.baseHP = Math.min(100, this.state.baseHP + effects.healBase * 100);
      }
    }

    this.state.completedEvents.push(ev.id);
    this.state.activeEvent = null;
    this.notify();
  }

  buildOnSlot(slotId, buildingId) {
    const slot = this.state.grid[slotId];
    if (!slot || slot.building || slot.constructing) return { ok: false, msg: 'Slot nije slobodan.' };

    const bDef = BUILDINGS[buildingId];
    if (!bDef) return { ok: false, msg: 'Nepoznata zgrada.' };
    if (!this.state.unlockedBuildings.includes(buildingId)) return { ok: false, msg: 'Zgrada nije otključana.' };

    // Provjeri resurse
    const cost = bDef.cost;
    for (const [res, amount] of Object.entries(cost)) {
      if ((this.state.resources[res] || 0) < amount) {
        return { ok: false, msg: `Nema dovoljno ${res}!` };
      }
    }

    // Oduzmi resurse
    Object.entries(cost).forEach(([res, amount]) => {
      this.state.resources[res] -= amount;
    });

    slot.building = buildingId;
    slot.constructing = true;
    slot.buildProgress = 0;
    slot.buildDuration = bDef.buildTime;

    this.addNotification(`Gradnja ${bDef.name} počela!`, 'info');
    this.notify();
    return { ok: true };
  }

  addNotification(msg, type = 'info') {
    const id = Date.now() + Math.random();
    this.state.notifications.push({ id, msg, type, ts: Date.now() });
    // Čisti stare (max 5)
    if (this.state.notifications.length > 5) {
      this.state.notifications.shift();
    }
  }

  removeNotification(id) {
    this.state.notifications = this.state.notifications.filter(n => n.id !== id);
    this.notify();
  }
}

export const gameState = new GameStateManager();
