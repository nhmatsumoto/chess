import { component$, useVisibleTask$, useStore, $ } from '@builder.io/qwik';
import { ChessScene } from '../components/chess/scene';

export default component$((props: any) => {
  const gameState = useStore({
    board: [] as any[],
    turn: 'White',
    history: [] as string[]
  });

  const fetchGameState = $(async () => {
    try {
      const apiUrl = (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000/api';
      const resp = await fetch(`${apiUrl}/chess`);
      const data = await resp.json();
      gameState.board = data.board;
      gameState.turn = data.turn;
      gameState.history = data.history;
    } catch (e) {
      console.warn('Backend not reachable, using demo state');
      gameState.board = [
        { type: 'Pawn', color: 'White', pos: 'e2' },
        { type: 'Pawn', color: 'Black', pos: 'e7' },
        { type: 'King', color: 'White', pos: 'e1' },
        { type: 'King', color: 'Black', pos: 'e8' }
      ];
    }
  });

  const handleMove = $(async (from: string, to: string) => {
    console.log(`Moving from ${from} to ${to}`);
    try {
      const apiUrl = (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000/api';
      const resp = await fetch(`${apiUrl}/chess/move`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ from, to })
      });
      if (resp.ok) await fetchGameState();
    } catch (e) {
      // Demo move if backend fails
      const pieceIdx = gameState.board.findIndex(p => p.pos === from);
      if (pieceIdx !== -1) {
        const newBoard = [...gameState.board];
        newBoard[pieceIdx] = { ...newBoard[pieceIdx], pos: to };
        gameState.board = newBoard;
        gameState.history = [...gameState.history, `${from} to ${to}`];
        gameState.turn = gameState.turn === 'White' ? 'Black' : 'White';
      }
    }
  });

  useVisibleTask$(async () => {
    await fetchGameState();
  });

  return (
    <div class="h-screen w-full flex flex-col bg-slate-900 text-white font-sans overflow-hidden">
      <header class="p-4 bg-slate-800 flex justify-between items-center shadow-lg border-b border-slate-700">
        <div class="flex items-center gap-3">
          <div class="w-8 h-8 bg-blue-500 rounded flex items-center justify-center font-bold">♝</div>
          <h1 class="text-2xl font-bold tracking-tight text-white">Qwik <span class="text-blue-400">Chess</span></h1>
        </div>
        <div class="flex items-center gap-6">
          <div class="flex flex-col items-end">
             <span class="text-xs text-slate-400 uppercase tracking-widest">Current Turn</span>
             <span class={`text-lg font-bold ${gameState.turn === 'White' ? 'text-white' : 'text-slate-500'}`}>{gameState.turn}</span>
          </div>
          <button onClick$={fetchGameState} class="p-2 rounded bg-slate-700 hover:bg-slate-600 transition-colors">
            Reset
          </button>
        </div>
      </header>
      
      <main class="flex-1 flex relative">
        <div class="flex-1 relative">
          <ChessScene board={gameState.board} onMove$={handleMove} />
        </div>
        
        <aside class="w-80 bg-slate-800/50 backdrop-blur-md p-6 border-l border-white/10 flex flex-col gap-6">
          <div>
            <h2 class="text-sm font-bold text-slate-400 uppercase tracking-widest mb-4">Move History</h2>
            <div class="h-64 overflow-y-auto space-y-2 pr-2 custom-scrollbar">
              {gameState.history.length === 0 ? (
                <p class="text-slate-500 italic text-sm">No moves yet...</p>
              ) : (
                gameState.history.map((move, i) => (
                  <div key={i} class="p-3 bg-slate-700/50 border border-white/5 rounded-lg text-sm flex justify-between items-center hover:bg-slate-700 transition-all">
                    <span class="text-slate-500">#{i + 1}</span>
                    <span class="font-mono font-medium text-blue-300">{move}</span>
                  </div>
                ))
              )}
            </div>
          </div>
          
          <div class="mt-auto p-4 bg-gradient-to-br from-blue-600/20 to-purple-600/20 rounded-xl border border-blue-500/20 space-y-2">
            <h3 class="text-xs font-bold text-blue-400 uppercase">How to play</h3>
            <p class="text-xs text-slate-300 leading-relaxed">
              1. Click a piece to select it.<br/>
              2. Click a target square to move.<br/>
              3. Backend validates rules & logic.
            </p>
          </div>
        </aside>
      </main>
    </div>
  );
});
