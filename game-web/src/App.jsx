// Glavni App — koordinira sve komponente
import { useState, useEffect, useCallback } from 'react';
import { GameCanvas } from './ui/GameCanvas.jsx';
import { HUD } from './ui/components/HUD.jsx';
import { BuildPanel } from './ui/components/BuildPanel.jsx';
import { EventModal } from './ui/components/EventModal.jsx';
import { Notifications } from './ui/components/Notifications.jsx';
import { GameOver } from './ui/components/GameOver.jsx';
import { useGameState } from './ui/hooks/useGameState.js';
import { gameState } from './engine/GameState.js';

export default function App() {
  const state = useGameState();
  const [showBuildPanel, setShowBuildPanel] = useState(false);
  const [selectedBuilding, setSelectedBuilding] = useState(null);

  // Pokretanje igre
  useEffect(() => {
    gameState.start();
    return () => gameState.stop();
  }, []);

  // Klik na slot — gradi ako je zgrada selektovana
  const handleSlotClick = useCallback((slotId) => {
    if (!selectedBuilding) {
      setShowBuildPanel(true);
      return;
    }
    const result = gameState.buildOnSlot(slotId, selectedBuilding);
    if (result.ok) {
      setSelectedBuilding(null);
      setShowBuildPanel(false);
    }
  }, [selectedBuilding]);

  const handleRestart = () => {
    window.location.reload();
  };

  return (
    <div style={{ position: 'relative', width: '100%', height: '100%', overflow: 'hidden', fontFamily: 'system-ui, sans-serif' }}>

      {/* PixiJS game canvas */}
      <GameCanvas grid={state.grid} onSlotClick={handleSlotClick} />

      {/* HUD */}
      <HUD state={state} />

      {/* Notifikacije */}
      <Notifications
        notifications={state.notifications}
        onRemove={(id) => gameState.removeNotification(id)}
      />

      {/* Story event modal */}
      {state.activeEvent && (
        <EventModal
          event={state.activeEvent}
          onResolve={(choiceId) => gameState.resolveEvent(choiceId)}
        />
      )}

      {/* Build dugme */}
      {!showBuildPanel && !state.activeEvent && !state.gameOver && (
        <button
          onClick={() => setShowBuildPanel(true)}
          style={{
            position: 'absolute', bottom: 20, left: '50%',
            transform: 'translateX(-50%)',
            padding: '12px 28px',
            background: 'linear-gradient(135deg, #D4A017, #a07010)',
            border: 'none', borderRadius: 12,
            color: '#0a0800', fontWeight: 700, fontSize: 15,
            cursor: 'pointer', zIndex: 200,
            boxShadow: '0 4px 20px rgba(212,160,23,0.4)',
            letterSpacing: 0.5,
          }}
        >
          🏗️ Gradi
        </button>
      )}

      {/* Build panel */}
      {showBuildPanel && !state.gameOver && (
        <BuildPanel
          state={state}
          selectedBuilding={selectedBuilding}
          onBuildSelect={(id) => setSelectedBuilding(id === selectedBuilding ? null : id)}
          onClose={() => { setShowBuildPanel(false); setSelectedBuilding(null); }}
        />
      )}

      {/* Hint kad je zgrada selektovana */}
      {selectedBuilding && (
        <div style={{
          position: 'absolute', top: '45%', left: '50%',
          transform: 'translate(-50%, -50%)',
          color: '#D4A017', fontSize: 14, fontWeight: 600,
          textShadow: '0 2px 8px rgba(0,0,0,0.8)',
          pointerEvents: 'none', zIndex: 150,
          background: 'rgba(0,0,0,0.6)', padding: '8px 16px',
          borderRadius: 8, border: '1px solid #D4A01744',
        }}>
          Klikni na slot za gradnju
        </div>
      )}

      {/* Game Over */}
      {state.gameOver && (
        <GameOver reason={state.gameOverReason} onRestart={handleRestart} />
      )}
    </div>
  );
}
