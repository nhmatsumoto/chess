using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ChessBackend.Models;

namespace ChessBackend.Services
{
    public interface IRoomService
    {
        GameRoom CreateRoom();
        GameRoom GetRoom(string id);
        bool DeleteRoom(string id);
        IEnumerable<string> GetAllRoomIds();
    }

    public class RoomService : IRoomService
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new ConcurrentDictionary<string, GameRoom>();

        public GameRoom CreateRoom()
        {
            var id = Guid.NewGuid().ToString().Substring(0, 8);
            var room = new GameRoom(id);
            _rooms[id] = room;
            return room;
        }

        public GameRoom GetRoom(string id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        public bool DeleteRoom(string id)
        {
            return _rooms.TryRemove(id, out _);
        }

        public IEnumerable<string> GetAllRoomIds()
        {
            return _rooms.Keys;
        }
    }
}
