// Hook koji subscribuje React na GameState promene
import { useState, useEffect } from 'react';
import { gameState } from '../../engine/GameState.js';

export function useGameState() {
  const [state, setState] = useState(() => gameState.getSnapshot());

  useEffect(() => {
    const unsub = gameState.subscribe(setState);
    return unsub;
  }, []);

  return state;
}
