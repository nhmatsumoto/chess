# 3D Chess: Qwik + Three.js + .NET

A full-stack 3D chess game.

## Setup Instructions

### Run with Docker (Recommended)
1. Ensure Docker and Docker Compose are installed.
2. From the root directory, run:
   ```bash
   docker-compose up --build
   ```
3. Open http://localhost:5173 to play.

### Local Development (Manual)
- **3D Rendering**: High-performance Three.js board and pieces.
- **Move Validation**: Server-side logic to ensure legal moves.
- **Interactive UI**: Click-to-move piece selection with raycasting.
- **Resumable UI**: Powered by Qwik for instant loading.

## Current State
- [x] Core Board Logic
- [x] Basic Piece Movement
- [x] Multi-platform Styling (Dark Mode)
- [x] Demo mode fallback if backend is offline
