import { component$, useVisibleTask$, useStore, $ } from '@builder.io/qwik';
import { ChessScene } from '../components/chess/scene';

export default component$((props: any) => {
  const gameState = useStore({
    board: [] as any[],
    turn: 'White',
    history: [] as string[],
    status: 'Active',
    legalMoves: [] as string[]
  });

  const fetchGameState = $(async () => {
    try {
      const apiUrl = (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000/api';
      const resp = await fetch(`${apiUrl}/chess`);
      const data = await resp.json();
      gameState.board = data.board;
      gameState.turn = data.turn;
      gameState.history = data.history;
      gameState.status = data.status || 'Active';
      gameState.legalMoves = [];
    } catch (e) {
      console.warn('Backend not reachable');
    }
  });

  const handleSelect = $(async (pos: string) => {
    try {
      const apiUrl = (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000/api';
      const resp = await fetch(`${apiUrl}/chess/moves?pos=${pos}`);
      if (resp.ok) {
        gameState.legalMoves = await resp.json();
      }
    } catch (e) {
      gameState.legalMoves = [];
    }
  });

  const handleMove = $(async (from: string, to: string) => {
    try {
      const apiUrl = (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000/api';
      const resp = await fetch(`${apiUrl}/chess/move`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ from, to })
      });
      if (resp.ok) {
        await fetchGameState();
      }
    } catch (e) {
      console.error('Move failed', e);
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
             <span class="text-xs text-slate-400 uppercase tracking-widest">{gameState.status === 'Active' ? 'Current Turn' : 'Status'}</span>
             <span class={`text-lg font-bold ${gameState.status !== 'Active' ? 'text-red-400' : gameState.turn === 'White' ? 'text-white' : 'text-slate-500'}`}>
               {gameState.status === 'Active' ? gameState.turn : gameState.status}
             </span>
          </div>
          <button onClick$={() => window.location.reload()} class="p-2 rounded bg-slate-700 hover:bg-slate-600 transition-colors">
            Reset
          </button>
        </div>
      </header>
      
      <main class="flex-1 flex relative">
        <div class="flex-1 relative">
          <ChessScene 
            board={gameState.board} 
            legalMoves={gameState.legalMoves}
            onMove$={handleMove} 
            onSelect$={handleSelect}
          />
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
