// PixiJS canvas — mountuje se jednom, komunicira sa React via props
import { useEffect, useRef } from 'react';
import * as PIXI from 'pixi.js';
import { GridRenderer } from '../engine/GridRenderer.js';
import { ParticleSystem } from '../engine/ParticleSystem.js';

export const GameCanvas = ({ grid, onSlotClick }) => {
  const mountRef = useRef(null);
  const appRef = useRef(null);
  const rendererRef = useRef(null);
  const particlesRef = useRef(null);

  useEffect(() => {
    // Init PixiJS jednom
    const app = new PIXI.Application({
      resizeTo: mountRef.current,
      backgroundAlpha: 0,
      antialias: true,
      resolution: window.devicePixelRatio || 1,
      autoDensity: true,
    });
    mountRef.current.appendChild(app.view);
    appRef.current = app;

    // Pozadina — tamni gradijent
    const bg = new PIXI.Graphics();
    const updateBg = () => {
      bg.clear();
      bg.beginFill(0x0a0812);
      bg.drawRect(0, 0, app.screen.width, app.screen.height);
      bg.endFill();
      // Suptilni sjaj u centru
      const grd = new PIXI.Graphics();
      grd.beginFill(0x1a1020, 1);
      grd.drawEllipse(app.screen.width / 2, app.screen.height * 0.4, 300, 200);
      grd.endFill();
    };
    updateBg();
    app.stage.addChildAt(bg, 0);

    const gridRenderer = new GridRenderer(app, onSlotClick);
    const particles = new ParticleSystem(app);
    rendererRef.current = gridRenderer;
    particlesRef.current = particles;

    // Resize
    const handleResize = () => {
      gridRenderer.resize(app.screen.width, app.screen.height);
      updateBg();
    };
    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
      app.destroy(true);
    };
  }, []);

  // Ažuriraj grid kad se state promeni
  useEffect(() => {
    if (rendererRef.current) {
      rendererRef.current.updateAll(grid);
    }
  }, [grid]);

  return (
    <div
      ref={mountRef}
      style={{ position: 'absolute', inset: 0, zIndex: 1 }}
    />
  );
};
