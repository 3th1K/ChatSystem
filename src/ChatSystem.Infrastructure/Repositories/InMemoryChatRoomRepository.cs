using ChatSystem.Application.Interfaces;
using ChatSystem.Domain.Models;

namespace ChatSystem.Infrastructure.Repositories;

public class InMemoryChatRoomRepository : IChatRoomRepository
{
    private readonly Dictionary<string, ChatRoom> _rooms = new();

    public ChatRoom? GetRoom(string name) =>
        _rooms.TryGetValue(name, out var room) ? room : null;

    public void AddRoom(ChatRoom room) => _rooms[room.Name] = room;

    public void RemoveRoom(string name) => _rooms.Remove(name);

    public IEnumerable<ChatRoom> GetAllRooms() => _rooms.Values;
}