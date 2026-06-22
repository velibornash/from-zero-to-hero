// Game Over ekran
import { useEffect, useRef } from 'react';
import { gsap } from 'gsap';

export const GameOver = ({ reason, onRestart }) => {
  const ref = useRef(null);

  useEffect(() => {
    if (ref.current) {
      gsap.fromTo(ref.current, { opacity: 0 }, { opacity: 1, duration: 1, ease: 'power2.out' });
    }
  }, []);

  return (
    <div ref={ref} style={{
      position: 'absolute', inset: 0, zIndex: 999,
      background: 'rgba(0,0,0,0.9)',
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
      backdropFilter: 'blur(8px)',
    }}>
      <div style={{ fontSize: 60, marginBottom: 20 }}>⚔️</div>
      <h1 style={{ color: '#f87171', fontSize: 32, fontWeight: 900, marginBottom: 10, letterSpacing: 2 }}>
        GRAD JE PAO
      </h1>
      <p style={{ color: '#888', fontSize: 16, marginBottom: 40 }}>{reason}</p>
      <button
        onClick={onRestart}
        style={{
          padding: '14px 40px',
          background: 'linear-gradient(135deg, #D4A017, #a07010)',
          border: 'none', borderRadius: 12,
          color: '#0a0800', fontWeight: 700, fontSize: 16,
          cursor: 'pointer', letterSpacing: 1,
        }}
      >
        Pokušaj ponovo
      </button>
    </div>
  );
};
