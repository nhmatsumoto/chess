import { useEffect, useState } from 'react';
import { useGameStore } from './store/gameStore';
import { ChessScene } from './components/chess/ChessScene.tsx';
import { ScrollText, RotateCcw, Copy, LogOut, Plus, LogIn } from 'lucide-react';

function App() {
  const { roomId, board, turn, history, status, fetchGame, isLoading, createRoom, setRoomId } = useGameStore();
  const [joinId, setJoinId] = useState('');

  useEffect(() => {
    if (roomId) {
      fetchGame();
      // Polling for updates in multiplayer
      const interval = setInterval(fetchGame, 2000);
      return () => clearInterval(interval);
    }
  }, [roomId, fetchGame]);

  const handleCopyId = () => {
    if (roomId) {
      navigator.clipboard.writeText(roomId);
    }
  };

  if (!roomId) {
    return (
      <div className="flex flex-col items-center justify-center h-screen w-screen bg-slate-900 text-slate-100 font-sans p-4">
        <div className="max-w-md w-full bg-slate-800 rounded-2xl shadow-2xl p-8 border border-white/10 flex flex-col gap-8">
          <div className="text-center">
            <div className="w-16 h-16 bg-blue-500 rounded-2xl flex items-center justify-center font-bold text-3xl text-white shadow-lg mx-auto mb-4">♝</div>
            <h1 className="text-3xl font-bold tracking-tight text-white mb-2">React<span className="text-blue-400">Chess</span></h1>
            <p className="text-slate-400 text-sm">Create a room to play with a friend or join an existing one.</p>
          </div>

          <div className="flex flex-col gap-4">
            <button 
              onClick={() => createRoom()}
              disabled={isLoading}
              className="w-full py-4 bg-blue-600 hover:bg-blue-500 disabled:bg-blue-800 rounded-xl font-bold flex items-center justify-center gap-3 transition-all active:scale-95 shadow-lg shadow-blue-500/20"
            >
              {isLoading ? 'Creating...' : 'Create New Room'}
            </button>
            {isLoading && <p className="text-blue-400 text-xs text-center animate-pulse">Communicating with backend...</p>}

            <div className="relative flex items-center py-4">
              <div className="flex-grow border-t border-slate-700"></div>
              <span className="flex-shrink mx-4 text-slate-500 text-xs uppercase tracking-widest font-bold">OR</span>
              <div className="flex-grow border-t border-slate-700"></div>
            </div>

            <div className="flex flex-col gap-2">
              <input 
                type="text" 
                placeholder="Enter Room ID (e.g. 5a2e1c8d)"
                value={joinId}
                onChange={(e) => setJoinId(e.target.value)}
                className="w-full bg-slate-900 border border-slate-700 rounded-xl px-4 py-4 text-slate-100 placeholder-slate-600 focus:outline-none focus:border-blue-500 transition-colors"
              />
              <button 
                onClick={() => setRoomId(joinId)}
                disabled={!joinId || isLoading}
                className="w-full py-4 bg-slate-700 hover:bg-slate-600 disabled:opacity-50 rounded-xl font-bold flex items-center justify-center gap-3 transition-all active:scale-95 border border-white/5"
              >
                <LogIn size={20} />
                Join Room
              </button>
            </div>
          </div>
        </div>
        <p className="mt-8 text-slate-600 text-xs tracking-widest uppercase">Version 3.0.0 • Room System Enabled</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen w-screen bg-slate-900 text-slate-100 font-sans overflow-hidden">
      {/* Header */}
      <header className="p-4 bg-slate-800 flex justify-between items-center shadow-lg border-b border-slate-700 z-10">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 bg-blue-500 rounded flex items-center justify-center font-bold text-white shadow-inner">♝</div>
          <div className="flex flex-col">
            <h1 className="text-xl font-bold tracking-tight text-white leading-tight">
              React<span className="text-blue-400">Chess</span>
            </h1>
            <div className="flex items-center gap-1.5 group cursor-pointer" onClick={handleCopyId}>
              <span className="text-[10px] text-slate-500 font-mono tracking-tighter uppercase font-bold group-hover:text-blue-400 transition-colors">Room: {roomId}</span>
              <Copy size={10} className="text-slate-600 group-hover:text-blue-400 transition-colors" />
            </div>
          </div>
          {isLoading && <div className="animate-spin h-4 w-4 border-2 border-blue-500 border-t-transparent rounded-full ml-2" />}
        </div>
        
        <div className="flex items-center gap-6">
          <div className="flex flex-col items-end">
             <span className="text-xs text-slate-400 uppercase tracking-widest font-semibold">
               {status === 'Active' ? 'Current Turn' : 'Game Over'}
             </span>
             <span className={`text-lg font-bold ${
               status !== 'Active' ? 'text-red-400' : (turn === 'White' ? 'text-white' : 'text-slate-500')
             }`}>
               {status === 'Active' ? turn : status}
             </span>
          </div>
          <button 
            onClick={() => window.location.reload()} 
            className="p-2.5 rounded bg-slate-700 hover:bg-red-900/40 hover:text-red-400 transition-all active:scale-95 shadow-md flex items-center gap-2 group border border-transparent hover:border-red-900/50"
            title="Leave Room"
          >
            <LogOut size={18} />
          </button>
        </div>
      </header>
      
      {/* Main Content */}
      <main className="flex-1 flex relative">
        <div className="flex-1 relative bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-slate-800 to-slate-950">
          <ChessScene />
        </div>
        
        {/* Sidebar */}
        <aside className="w-80 bg-slate-800/80 backdrop-blur-md p-6 border-l border-white/10 flex flex-col gap-6 shadow-2xl z-10">
          <div className="flex flex-col gap-2">
            <div className="flex items-center gap-2 text-blue-400">
              <ScrollText size={20} />
              <h2 className="text-lg font-bold uppercase tracking-wider text-slate-100">Move History</h2>
            </div>
            <div className="bg-slate-900/50 rounded-xl p-4 h-[300px] overflow-y-auto border border-white/5 scrollbar-thin scrollbar-thumb-slate-700">
              <div className="grid grid-cols-2 gap-x-4 gap-y-2">
                {history.length === 0 ? (
                  <div className="col-span-2 text-slate-500 italic text-sm text-center mt-10">
                    Waiting for moves...
                  </div>
                ) : (
                  history.map((move, i) => (
                    <div key={i} className="flex items-center gap-2 text-sm">
                      <span className="text-slate-600 font-mono w-4 italic">{Math.floor(i/2) + 1}.</span>
                      <span className="bg-slate-800 px-2 py-1 rounded text-slate-200 border border-white/5 w-full">
                        {move}
                      </span>
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>

          <div className="p-4 bg-blue-600/10 rounded-xl border border-blue-500/20">
            <h3 className="text-xs font-bold uppercase text-blue-400 mb-1 leading-none flex items-center gap-2">
              <span className="w-1.5 h-1.5 rounded-full bg-blue-500 animate-pulse" />
              Multiplayer Active
            </h3>
            <p className="text-[10px] text-slate-400 leading-relaxed mt-1">
              Share your Room ID with a friend to play together. Your view and moves are synced in real-time.
            </p>
          </div>
        </aside>
      </main>
      
      {/* Footer */}
      <footer className="px-4 py-2 bg-slate-950/80 border-t border-white/5 text-[10px] text-slate-600 flex justify-between uppercase tracking-widest z-10">
         <span>Session Storage: In-Memory Singleton</span>
         <span>Frontend: React + Zustand + Three.js</span>
         <span>Build: Stable v3.0</span>
      </footer>
    </div>
  );
}

export default App;
