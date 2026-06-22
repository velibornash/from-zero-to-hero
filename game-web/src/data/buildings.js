// Definicija svih zgrada — tip, cena, produkcija, vreme gradnje
export const BUILDINGS = {
  koliba: {
    id: 'koliba', name: 'Koliba', icon: '🏚️', color: 0x8B6914,
    cost: { wood: 10, gold: 0 }, buildTime: 3,
    produces: { food: 1 }, productionInterval: 5,
    description: 'Osnovno sklonište. +1 populacija.', unlocked: true,
  },
  drvoseca: {
    id: 'drvoseca', name: 'Drvoseča', icon: '🪵', color: 0x5C4A1E,
    cost: { wood: 15, gold: 5 }, buildTime: 5,
    produces: { wood: 2 }, productionInterval: 4,
    description: 'Seče drvo automatski.', unlocked: true,
  },
  pekara: {
    id: 'pekara', name: 'Pekara', icon: '🍞', color: 0xD4A017,
    cost: { wood: 20, gold: 10 }, buildTime: 6,
    produces: { food: 3 }, productionInterval: 5,
    description: 'Hrani stanovništvo efikasnije.', unlocked: false,
  },
  strazara: {
    id: 'strazara', name: 'Stražara', icon: '⚔️', color: 0x8B0000,
    cost: { wood: 30, gold: 20 }, buildTime: 8,
    produces: { defense: 5 }, productionInterval: 0,
    description: 'Brani grad. +5 odbrana.', unlocked: false,
  },
  kafana: {
    id: 'kafana', name: 'Kafana Čardak', icon: '🍺', color: 0xA0522D,
    cost: { wood: 25, gold: 15 }, buildTime: 7,
    produces: { morale: 1 }, productionInterval: 3,
    description: 'Podiže moral svakih nekoliko sekundi.', unlocked: false,
  },
  manastir: {
    id: 'manastir', name: 'Manastir', icon: '⛪', color: 0xF5F5DC,
    cost: { wood: 40, gold: 30 }, buildTime: 10,
    produces: { morale: 2, stability: 1 }, productionInterval: 5,
    description: 'Daje stabilnost i moral.', unlocked: false,
  },
  kasarna: {
    id: 'kasarna', name: 'Kasarna', icon: '🛡️', color: 0x4A4A6A,
    cost: { wood: 50, gold: 40 }, buildTime: 12,
    produces: { soldiers: 1 }, productionInterval: 10,
    description: 'Trenira vojnike.', unlocked: false,
  },
  mitski_slot: {
    id: 'mitski_slot', name: 'Mitski Slot', icon: '🐉', color: 0x8A2BE2,
    cost: { wood: 100, gold: 100 }, buildTime: 20,
    produces: { morale: 5, defense: 10 }, productionInterval: 8,
    description: 'Mistična građevina — otključava legendarne moći.', unlocked: false,
  },
};
