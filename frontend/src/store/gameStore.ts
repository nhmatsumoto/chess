import { create } from 'zustand';

interface GameState {
  roomId: string | null;
  board: any[];
  turn: string;
  history: string[];
  status: string;
  legalMoves: string[];
  isLoading: boolean;
  apiUrl: string;

  setRoomId: (id: string) => void;
  createRoom: () => Promise<string>;
  fetchGame: () => Promise<void>;
  fetchLegalMoves: (pos: string) => Promise<void>;
  makeMove: (from: string, to: string) => Promise<void>;
}

export const useGameStore = create<GameState>((set, get) => ({
  roomId: null,
  board: [],
  turn: 'White',
  history: [],
  status: 'Active',
  legalMoves: [],
  isLoading: false,
  apiUrl: (import.meta as any).env?.VITE_API_URL || 'http://localhost:5000/api',

  setRoomId: (id: string) => set({ roomId: id }),

  createRoom: async () => {
    set({ isLoading: true });
    try {
      const resp = await fetch(`${get().apiUrl}/chess/rooms`, { method: 'POST' });
      const data = await resp.json();
      set({ roomId: data.roomId, isLoading: false });
      return data.roomId;
    } catch (e) {
      console.error('Failed to create room', e);
      alert('Failed to connect to backend. Make sure the backend is running at ' + get().apiUrl);
      set({ isLoading: false });
      return '';
    }
  },

  fetchGame: async () => {
    const { roomId, apiUrl } = get();
    if (!roomId) return;
    
    set({ isLoading: true });
    try {
      const resp = await fetch(`${apiUrl}/chess/rooms/${roomId}`);
      if (resp.ok) {
        const data = await resp.json();
        set({
          board: data.board,
          turn: data.turn,
          history: data.history,
          status: data.status,
          legalMoves: [],
        });
      }
    } catch (e) {
      console.error('Failed to fetch game', e);
    } finally {
      set({ isLoading: false });
    }
  },

  fetchLegalMoves: async (pos: string) => {
    const { roomId, apiUrl } = get();
    if (!roomId) return;

    try {
      const resp = await fetch(`${apiUrl}/chess/rooms/${roomId}/moves?pos=${pos}`);
      if (resp.ok) {
        const moves = await resp.json();
        set({ legalMoves: moves });
      }
    } catch (e) {
      set({ legalMoves: [] });
    }
  },

  makeMove: async (from: string, to: string) => {
    const { roomId, apiUrl } = get();
    if (!roomId) return;

    set({ isLoading: true });
    try {
      const resp = await fetch(`${apiUrl}/chess/rooms/${roomId}/move`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ from, to })
      });
      if (resp.ok) {
        await get().fetchGame();
      }
    } catch (e) {
      console.error('Move failed', e);
    } finally {
      set({ isLoading: false });
    }
  }
}));
