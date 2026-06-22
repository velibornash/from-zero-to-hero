// PixiJS renderer za game grid — izometrijski pogled
import * as PIXI from 'pixi.js';
import { gsap } from 'gsap';
import { BUILDINGS } from '../data/buildings.js';

const COLS = 5;
const ROWS = 4;
const TILE_W = 120;
const TILE_H = 70;

// Izometrijska konverzija
function isoPos(col, row) {
  return {
    x: (col - row) * (TILE_W / 2),
    y: (col + row) * (TILE_H / 2),
  };
}

export class GridRenderer {
  constructor(app, onSlotClick) {
    this.app = app;
    this.onSlotClick = onSlotClick;
    this.container = new PIXI.Container();
    this.slotGraphics = [];
    this.buildingSprites = [];

    // Centriramo container
    this.container.x = app.screen.width / 2;
    this.container.y = app.screen.height * 0.25;

    app.stage.addChild(this.container);
    this.buildGrid();
  }

  buildGrid() {
    for (let row = 0; row < ROWS; row++) {
      for (let col = 0; col < COLS; col++) {
        const idx = row * COLS + col;
        const pos = isoPos(col, row);

        const tile = new PIXI.Graphics();
        this.drawTile(tile, false);
        tile.x = pos.x;
        tile.y = pos.y;
        tile.interactive = true;
        tile.cursor = 'pointer';
        tile.on('pointerover', () => this.hoverTile(tile, idx));
        tile.on('pointerout', () => this.unhoverTile(tile, idx));
        tile.on('pointertap', () => this.onSlotClick(idx));

        this.container.addChild(tile);
        this.slotGraphics[idx] = tile;

        // Building container za svaki slot
        const bContainer = new PIXI.Container();
        bContainer.x = pos.x;
        bContainer.y = pos.y;
        this.container.addChild(bContainer);
        this.buildingSprites[idx] = bContainer;
      }
    }
  }

  drawTile(g, hovered) {
    g.clear();
    const w = TILE_W / 2;
    const h = TILE_H / 2;

    // Sjena
    g.beginFill(0x000000, 0.15);
    g.drawPolygon([0, h + 4, w, h / 2 + 4, 0, 0 + 4, -w, h / 2 + 4]);
    g.endFill();

    // Dno tile-a (leva i desna strana)
    g.beginFill(hovered ? 0x4a3820 : 0x2a1f10);
    g.drawPolygon([0, h, w, h / 2, w, h / 2 + 8, 0, h + 8]);
    g.endFill();
    g.beginFill(hovered ? 0x3a2818 : 0x1f1508);
    g.drawPolygon([0, h, -w, h / 2, -w, h / 2 + 8, 0, h + 8]);
    g.endFill();

    // Gornja površina
    const topColor = hovered ? 0x7a5c2e : 0x5a4020;
    g.beginFill(topColor, 1);
    g.drawPolygon([0, 0, w, h / 2, 0, h, -w, h / 2]);
    g.endFill();

    // Border
    g.lineStyle(1, hovered ? 0xd4a017 : 0x8B6914, hovered ? 0.9 : 0.4);
    g.drawPolygon([0, 0, w, h / 2, 0, h, -w, h / 2]);
  }

  hoverTile(tile, idx) {
    this.drawTile(tile, true);
    gsap.to(tile, { pixi: { y: tile.y - 3 }, duration: 0.15, ease: 'power2.out' });
  }

  unhoverTile(tile, idx) {
    const { col, row } = this.idxToColRow(idx);
    const pos = isoPos(col, row);
    this.drawTile(tile, false);
    gsap.to(tile, { pixi: { y: pos.y }, duration: 0.15, ease: 'power2.in' });
  }

  idxToColRow(idx) {
    return { col: idx % COLS, row: Math.floor(idx / COLS) };
  }

  // Ažurira vizual jednog slota na osnovu stanja
  updateSlot(slot) {
    const bCont = this.buildingSprites[slot.id];
    if (!bCont) return;
    bCont.removeChildren();

    if (!slot.building) return;

    const bDef = BUILDINGS[slot.building];
    if (!bDef) return;

    if (slot.constructing) {
      this.drawConstructing(bCont, slot);
    } else {
      this.drawBuilding(bCont, bDef);
    }
  }

  drawConstructing(cont, slot) {
    const pct = slot.buildProgress / slot.buildDuration;
    const g = new PIXI.Graphics();

    // Scaffolding izgled
    g.lineStyle(2, 0xD4A017, 0.6);
    g.drawRect(-20, -40, 40, 40);
    g.lineStyle(1, 0xD4A017, 0.3);
    g.moveTo(-20, -20); g.lineTo(20, -20);
    g.moveTo(0, -40); g.lineTo(0, 0);

    // Progress bar
    g.beginFill(0x1a1a1a);
    g.drawRoundedRect(-25, -55, 50, 8, 4);
    g.endFill();
    g.beginFill(0xD4A017);
    g.drawRoundedRect(-25, -55, 50 * pct, 8, 4);
    g.endFill();

    cont.addChild(g);

    // Animacija pulsiranja
    gsap.to(g, { pixi: { alpha: 0.6 }, duration: 0.8, yoyo: true, repeat: -1, ease: 'sine.inOut' });
  }

  drawBuilding(cont, bDef) {
    const g = new PIXI.Graphics();
    const c = bDef.color;

    // Telo zgrade — izometrijski blok
    const w = 35;
    const h = 20;
    const bh = 30; // visina zgrade

    // Leva strana
    g.beginFill(this.darken(c, 0.5));
    g.drawPolygon([0, 0, w, h / 2, w, h / 2 - bh, 0, -bh]);
    g.endFill();

    // Desna strana
    g.beginFill(this.darken(c, 0.7));
    g.drawPolygon([0, 0, -w, h / 2, -w, h / 2 - bh, 0, -bh]);
    g.endFill();

    // Krov / gornja površina
    g.beginFill(c);
    g.drawPolygon([0, -bh, w, h / 2 - bh, 0, h - bh, -w, h / 2 - bh]);
    g.endFill();

    // Border
    g.lineStyle(1, 0xffffff, 0.15);
    g.drawPolygon([0, -bh, w, h / 2 - bh, 0, h - bh, -w, h / 2 - bh]);

    cont.addChild(g);

    // Ikona zgrade kao tekst
    const icon = new PIXI.Text(bDef.icon, {
      fontSize: 18,
      align: 'center',
    });
    icon.anchor.set(0.5, 1);
    icon.y = -bh - 5;
    cont.addChild(icon);

    // Spawn animacija
    cont.alpha = 0;
    cont.scale.set(0.5);
    gsap.to(cont, {
      pixi: { alpha: 1, scaleX: 1, scaleY: 1 },
      duration: 0.4,
      ease: 'back.out(1.7)',
    });
  }

  darken(color, factor) {
    const r = Math.floor(((color >> 16) & 0xff) * factor);
    const g = Math.floor(((color >> 8) & 0xff) * factor);
    const b = Math.floor((color & 0xff) * factor);
    return (r << 16) | (g << 8) | b;
  }

  // Pozovi kad se state promeni
  updateAll(grid) {
    grid.forEach(slot => this.updateSlot(slot));
  }

  resize(w, h) {
    this.container.x = w / 2;
    this.container.y = h * 0.25;
  }
}
