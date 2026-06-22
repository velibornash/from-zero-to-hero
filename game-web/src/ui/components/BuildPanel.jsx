// Panel za odabir i gradnju zgrada
import { useState, useEffect, useRef } from 'react';
import { gsap } from 'gsap';
import { BUILDINGS } from '../../data/buildings.js';

const BuildingCard = ({ building, canAfford, selected, onSelect }) => {
  const cardRef = useRef(null);

  const handleClick = () => {
    if (!canAfford) return;
    onSelect(building.id);
    if (cardRef.current) {
      gsap.fromTo(cardRef.current,
        { scale: 0.95 },
        { scale: 1, duration: 0.2, ease: 'back.out(2)' }
      );
    }
  };

  return (
    <div
      ref={cardRef}
      onClick={handleClick}
      style={{
        background: selected
          ? 'rgba(212,160,23,0.2)'
          : canAfford ? 'rgba(255,255,255,0.05)' : 'rgba(0,0,0,0.3)',
        border: `1px solid ${selected ? '#D4A017' : canAfford ? '#ffffff22' : '#ffffff0a'}`,
        borderRadius: 10, padding: '10px 12px',
        cursor: canAfford ? 'pointer' : 'not-allowed',
        opacity: canAfford ? 1 : 0.5,
        transition: 'all 0.2s',
        display: 'flex', flexDirection: 'column', gap: 4,
        minWidth: 120,
      }}
    >
      <div style={{ fontSize: 24, textAlign: 'center' }}>{building.icon}</div>
      <div style={{ color: selected ? '#D4A017' : '#fff', fontWeight: 600, fontSize: 12, textAlign: 'center' }}>
        {building.name}
      </div>
      <div style={{ display: 'flex', justifyContent: 'center', gap: 6, flexWrap: 'wrap' }}>
        {building.cost.wood > 0 && (
          <span style={{ color: '#8B6914', fontSize: 10 }}>🪵 {building.cost.wood}</span>
        )}
        {building.cost.gold > 0 && (
          <span style={{ color: '#FFD700', fontSize: 10 }}>💰 {building.cost.gold}</span>
        )}
      </div>
      {selected && (
        <div style={{ color: '#D4A01788', fontSize: 10, textAlign: 'center' }}>
          Klikni na slot
        </div>
      )}
    </div>
  );
};

export const BuildPanel = ({ state, onBuildSelect, selectedBuilding, onClose }) => {
  const panelRef = useRef(null);
  const { resources, unlockedBuildings } = state;

  useEffect(() => {
    if (panelRef.current) {
      gsap.fromTo(panelRef.current,
        { y: 100, opacity: 0 },
        { y: 0, opacity: 1, duration: 0.35, ease: 'power3.out' }
      );
    }
  }, []);

  const canAfford = (building) => {
    return resources.wood >= (building.cost.wood || 0) &&
           resources.gold >= (building.cost.gold || 0);
  };

  const availableBuildings = unlockedBuildings.map(id => BUILDINGS[id]).filter(Boolean);

  return (
    <div
      ref={panelRef}
      style={{
        position: 'absolute', bottom: 0, left: 0, right: 0,
        background: 'linear-gradient(to top, rgba(10,8,5,0.97) 60%, rgba(10,8,5,0.8))',
        borderTop: '1px solid #D4A01733',
        padding: '16px', zIndex: 200,
        backdropFilter: 'blur(12px)',
      }}
    >
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
        <div style={{ color: '#D4A017', fontWeight: 700, fontSize: 14, letterSpacing: 1, textTransform: 'uppercase' }}>
          🏗️ Gradnja
        </div>
        <button
          onClick={onClose}
          style={{
            background: 'rgba(255,255,255,0.1)', border: '1px solid #ffffff22',
            color: '#fff', borderRadius: 6, padding: '4px 10px',
            cursor: 'pointer', fontSize: 12,
          }}
        >
          Zatvori
        </button>
      </div>

      {/* Kartice zgrada */}
      <div style={{ display: 'flex', gap: 8, overflowX: 'auto', paddingBottom: 4 }}>
        {availableBuildings.map(b => (
          <BuildingCard
            key={b.id}
            building={b}
            canAfford={canAfford(b)}
            selected={selectedBuilding === b.id}
            onSelect={onBuildSelect}
          />
        ))}
      </div>
    </div>
  );
};
