import { create } from 'zustand';
import api from '../services/api';

interface GameState {
  roomId: string | null;
  board: any[];
  turn: string;
  history: string[];
  status: string;
  legalMoves: string[];
  isLoading: boolean;

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

  setRoomId: (id: string) => set({ roomId: id }),

  createRoom: async () => {
    set({ isLoading: true });
    try {
      const data: any = await api.post('/chess/rooms');
      set({ roomId: data.roomId, isLoading: false });
      return data.roomId;
    } catch (e) {
      alert('Failed to connect to backend. Please check your connection.');
      set({ isLoading: false });
      return '';
    }
  },

  fetchGame: async () => {
    const { roomId } = get();
    if (!roomId) return;
    
    set({ isLoading: true });
    try {
      const data: any = await api.get(`/chess/rooms/${roomId}`);
      set({
        board: data.board,
        turn: data.turn,
        history: data.history,
        status: data.status,
        legalMoves: [],
      });
    } catch (e) {
      console.error('Failed to fetch game', e);
    } finally {
      set({ isLoading: false });
    }
  },

  fetchLegalMoves: async (pos: string) => {
    const { roomId } = get();
    if (!roomId) return;

    try {
      const moves: any = await api.get(`/chess/rooms/${roomId}/moves`, {
        params: { pos }
      });
      set({ legalMoves: moves });
    } catch (e) {
      set({ legalMoves: [] });
    }
  },

  makeMove: async (from: string, to: string) => {
    const { roomId } = get();
    if (!roomId) return;

    set({ isLoading: true });
    try {
      await api.post(`/chess/rooms/${roomId}/move`, { from, to });
      await get().fetchGame();
    } catch (e) {
      console.error('Move failed', e);
    } finally {
      set({ isLoading: false });
    }
  }
}));
