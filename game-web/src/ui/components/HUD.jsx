// Gornji HUD — resursi, moral, HP baze
import { useEffect, useRef } from 'react';
import { gsap } from 'gsap';

const ResourceItem = ({ icon, value, label, color, prevValue }) => {
  const valRef = useRef(null);
  const prevRef = useRef(value);

  useEffect(() => {
    if (prevRef.current !== value && valRef.current) {
      const diff = value - prevRef.current;
      if (diff !== 0) {
        gsap.fromTo(valRef.current,
          { color: diff > 0 ? '#4ade80' : '#f87171', scale: 1.3 },
          { color: '#ffffff', scale: 1, duration: 0.4, ease: 'back.out' }
        );
      }
      prevRef.current = value;
    }
  }, [value]);

  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 6,
      background: 'rgba(0,0,0,0.5)',
      border: `1px solid ${color}44`,
      borderRadius: 8, padding: '6px 12px',
      backdropFilter: 'blur(8px)',
    }}>
      <span style={{ fontSize: 18 }}>{icon}</span>
      <div>
        <div ref={valRef} style={{ color: '#fff', fontWeight: 700, fontSize: 16, lineHeight: 1 }}>
          {value}
        </div>
        <div style={{ color: '#888', fontSize: 10, textTransform: 'uppercase', letterSpacing: 1 }}>
          {label}
        </div>
      </div>
    </div>
  );
};

const MoraleBar = ({ value }) => {
  const color = value > 60 ? '#4ade80' : value > 30 ? '#facc15' : '#f87171';
  return (
    <div style={{
      background: 'rgba(0,0,0,0.5)',
      border: `1px solid ${color}44`,
      borderRadius: 8, padding: '6px 12px',
      backdropFilter: 'blur(8px)',
      minWidth: 120,
    }}>
      <div style={{ color: '#888', fontSize: 10, textTransform: 'uppercase', letterSpacing: 1, marginBottom: 4 }}>
        ❤️ Moral
      </div>
      <div style={{ background: '#1a1a1a', borderRadius: 4, height: 8, overflow: 'hidden' }}>
        <div style={{
          width: `${value}%`, height: '100%',
          background: `linear-gradient(90deg, ${color}88, ${color})`,
          borderRadius: 4,
          transition: 'width 0.5s ease, background 0.3s',
        }} />
      </div>
      <div style={{ color, fontSize: 11, marginTop: 2, fontWeight: 600 }}>{value}/100</div>
    </div>
  );
};

export const HUD = ({ state }) => {
  const { resources, population, morale, baseHP, day, era, activeBuffs = [] } = state;

  return (
    <div style={{
      position: 'absolute', top: 0, left: 0, right: 0,
      padding: '12px 16px',
      display: 'flex', alignItems: 'flex-start', gap: 8, flexWrap: 'wrap',
      pointerEvents: 'none', zIndex: 100,
    }}>
      {/* Resursi */}
      <ResourceItem icon="🪵" value={resources.wood} label="Drvo" color="#8B6914" />
      <ResourceItem icon="🍞" value={resources.food} label="Hrana" color="#D4A017" />
      <ResourceItem icon="💰" value={resources.gold} label="Zlato" color="#FFD700" />
      <ResourceItem icon="👥" value={`${population.current}/${population.max}`} label="Pop." color="#60a5fa" />

      {/* Moral */}
      <MoraleBar value={morale} />

      {/* Baza HP */}
      <div style={{
        background: 'rgba(0,0,0,0.5)', border: '1px solid #f8717144',
        borderRadius: 8, padding: '6px 12px', backdropFilter: 'blur(8px)', minWidth: 120,
      }}>
        <div style={{ color: '#888', fontSize: 10, textTransform: 'uppercase', letterSpacing: 1, marginBottom: 4 }}>
          🏰 Baza
        </div>
        <div style={{ background: '#1a1a1a', borderRadius: 4, height: 8, overflow: 'hidden' }}>
          <div style={{
            width: `${baseHP}%`, height: '100%',
            background: 'linear-gradient(90deg, #f8717188, #f87171)',
            borderRadius: 4, transition: 'width 0.5s ease',
          }} />
        </div>
        <div style={{ color: '#f87171', fontSize: 11, marginTop: 2, fontWeight: 600 }}>{baseHP}/100</div>
      </div>

      {/* Dan / Era */}
      <div style={{
        marginLeft: 'auto', background: 'rgba(0,0,0,0.5)',
        border: '1px solid #D4A01744', borderRadius: 8,
        padding: '6px 12px', backdropFilter: 'blur(8px)',
        textAlign: 'right',
      }}>
        <div style={{ color: '#D4A017', fontWeight: 700, fontSize: 14 }}>Dan {day}</div>
        <div style={{ color: '#888', fontSize: 10, textTransform: 'uppercase', letterSpacing: 1 }}>Era {era}</div>
      </div>

      {/* Aktivni buffovi */}
      {activeBuffs.length > 0 && (
        <div style={{ width: '100%', display: 'flex', gap: 6, flexWrap: 'wrap' }}>
          {activeBuffs.map((b, i) => (
            <div key={i} style={{
              background: 'rgba(212,160,23,0.15)', border: '1px solid #D4A01788',
              borderRadius: 6, padding: '3px 8px', fontSize: 11, color: '#D4A017',
            }}>
              ✦ {b.name} ({b.remaining}s)
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
