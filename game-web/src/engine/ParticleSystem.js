// Particle efekti — resursi koji lete, eksplozije, magla
import * as PIXI from 'pixi.js';
import { gsap } from 'gsap';

export class ParticleSystem {
  constructor(app) {
    this.app = app;
    this.container = new PIXI.Container();
    app.stage.addChild(this.container);
  }

  // Resurs "leti" od izvora ka HUD-u
  spawnResourceFly(fromX, fromY, toX, toY, icon, color = 0xD4A017) {
    for (let i = 0; i < 3; i++) {
      setTimeout(() => {
        const p = new PIXI.Text(icon, { fontSize: 16 });
        p.anchor.set(0.5);
        p.x = fromX + (Math.random() - 0.5) * 20;
        p.y = fromY + (Math.random() - 0.5) * 20;
        this.container.addChild(p);

        gsap.to(p, {
          x: toX,
          y: toY,
          duration: 0.8 + Math.random() * 0.3,
          ease: 'power2.in',
          onComplete: () => {
            this.container.removeChild(p);
          },
        });
        gsap.to(p, { pixi: { alpha: 0 }, duration: 0.3, delay: 0.6 });
      }, i * 120);
    }
  }

  // Eksplozija partikla (za borbu, event)
  spawnBurst(x, y, color = 0xff4400, count = 12) {
    for (let i = 0; i < count; i++) {
      const p = new PIXI.Graphics();
      p.beginFill(color);
      p.drawCircle(0, 0, 3 + Math.random() * 4);
      p.endFill();
      p.x = x;
      p.y = y;
      this.container.addChild(p);

      const angle = (i / count) * Math.PI * 2;
      const dist = 30 + Math.random() * 50;
      gsap.to(p, {
        x: x + Math.cos(angle) * dist,
        y: y + Math.sin(angle) * dist,
        duration: 0.6 + Math.random() * 0.4,
        ease: 'power2.out',
        onComplete: () => this.container.removeChild(p),
      });
      gsap.to(p, { pixi: { alpha: 0, scaleX: 0.2, scaleY: 0.2 }, duration: 0.5, delay: 0.3 });
    }
  }

  // Zlatni sjaj za Heroic Surge
  spawnHeroicGlow(x, y) {
    for (let i = 0; i < 8; i++) {
      const star = new PIXI.Text('✦', { fontSize: 12 + Math.random() * 10, fill: 0xD4A017 });
      star.anchor.set(0.5);
      star.x = x + (Math.random() - 0.5) * 60;
      star.y = y + (Math.random() - 0.5) * 60;
      this.container.addChild(star);

      gsap.to(star, {
        y: star.y - 40 - Math.random() * 30,
        duration: 1.2,
        ease: 'power1.out',
        onComplete: () => this.container.removeChild(star),
      });
      gsap.to(star, { pixi: { alpha: 0, rotation: Math.PI }, duration: 1.0, delay: 0.2 });
    }
  }
}
