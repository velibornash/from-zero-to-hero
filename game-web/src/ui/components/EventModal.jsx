// Popup za story evente — sa animacijom i izborima
import { useEffect, useRef } from 'react';
import { gsap } from 'gsap';

export const EventModal = ({ event, onResolve }) => {
  const overlayRef = useRef(null);
  const cardRef = useRef(null);

  useEffect(() => {
    if (overlayRef.current) {
      gsap.fromTo(overlayRef.current, { opacity: 0 }, { opacity: 1, duration: 0.3 });
    }
    if (cardRef.current) {
      gsap.fromTo(cardRef.current,
        { scale: 0.85, y: 40, opacity: 0 },
        { scale: 1, y: 0, opacity: 1, duration: 0.4, ease: 'back.out(1.5)' }
      );
    }
  }, []);

  const handleChoice = (choiceId) => {
    if (cardRef.current) {
      gsap.to(cardRef.current, {
        scale: 0.9, opacity: 0, y: -20,
        duration: 0.25, ease: 'power2.in',
        onComplete: () => onResolve(choiceId),
      });
    } else {
      onResolve(choiceId);
    }
  };

  return (
    <div ref={overlayRef} style={{
      position: 'absolute', inset: 0, zIndex: 500,
      background: 'rgba(0,0,0,0.75)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      backdropFilter: 'blur(4px)',
      padding: 20,
    }}>
      <div ref={cardRef} style={{
        background: 'linear-gradient(135deg, #0f0d08 0%, #1a1508 100%)',
        border: '1px solid #D4A01766',
        borderRadius: 16, padding: '28px 24px',
        maxWidth: 420, width: '100%',
        boxShadow: '0 0 60px rgba(212,160,23,0.15), 0 20px 60px rgba(0,0,0,0.8)',
      }}>
        {/* Era badge */}
        <div style={{
          display: 'inline-block',
          background: 'rgba(212,160,23,0.15)',
          border: '1px solid #D4A01755',
          borderRadius: 20, padding: '3px 10px',
          color: '#D4A017', fontSize: 11,
          textTransform: 'uppercase', letterSpacing: 1,
          marginBottom: 16,
        }}>
          Era {event.era} · Priča
        </div>

        {/* Ikona i naslov */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
          <div style={{ fontSize: 40 }}>{event.icon}</div>
          <h2 style={{ color: '#fff', fontSize: 20, fontWeight: 700, lineHeight: 1.2 }}>
            {event.name}
          </h2>
        </div>

        {/* Opis */}
        <p style={{ color: '#ccc', fontSize: 14, lineHeight: 1.7, marginBottom: 12 }}>
          {event.description}
        </p>

        {/* Flavor tekst */}
        {event.flavor && (
          <div style={{
            borderLeft: '2px solid #D4A01766',
            paddingLeft: 12, marginBottom: 20,
            color: '#D4A017', fontSize: 13, fontStyle: 'italic',
          }}>
            {event.flavor}
          </div>
        )}

        {/* Efekti (ako nema izbora) */}
        {!event.choices && event.effects && (
          <div style={{
            background: 'rgba(212,160,23,0.08)',
            border: '1px solid #D4A01722',
            borderRadius: 8, padding: '10px 14px',
            marginBottom: 20,
          }}>
            {event.effects.resources && Object.entries(event.effects.resources).map(([r, v]) => (
              <div key={r} style={{ color: '#D4A017', fontSize: 12, marginBottom: 2 }}>
                +{v} {r === 'wood' ? '🪵 Drvo' : r === 'food' ? '🍞 Hrana' : '💰 Zlato'}
              </div>
            ))}
            {event.effects.morale && (
              <div style={{ color: '#4ade80', fontSize: 12 }}>+{event.effects.morale} ❤️ Moral</div>
            )}
            {event.effects.unlockBuildings && (
              <div style={{ color: '#60a5fa', fontSize: 12 }}>
                🔓 Otključano: {event.effects.unlockBuildings.join(', ')}
              </div>
            )}
          </div>
        )}

        {/* Dugme ili izbori */}
        {!event.choices ? (
          <button
            onClick={() => handleChoice(null)}
            style={{
              width: '100%', padding: '12px',
              background: 'linear-gradient(135deg, #D4A017, #a07010)',
              border: 'none', borderRadius: 10,
              color: '#0a0800', fontWeight: 700, fontSize: 15,
              cursor: 'pointer', letterSpacing: 0.5,
            }}
          >
            Nastavi →
          </button>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            {event.choices.map(choice => (
              <button
                key={choice.id}
                onClick={() => handleChoice(choice.id)}
                style={{
                  padding: '12px 16px',
                  background: 'rgba(255,255,255,0.05)',
                  border: '1px solid #ffffff22',
                  borderRadius: 10, color: '#fff',
                  cursor: 'pointer', textAlign: 'left',
                  transition: 'all 0.2s',
                }}
                onMouseEnter={e => e.target.style.background = 'rgba(212,160,23,0.15)'}
                onMouseLeave={e => e.target.style.background = 'rgba(255,255,255,0.05)'}
              >
                <div style={{ fontWeight: 600, marginBottom: 3 }}>
                  {choice.icon} {choice.label}
                </div>
                <div style={{ color: '#888', fontSize: 12 }}>{choice.description}</div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
