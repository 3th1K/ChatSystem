using ChatSystem.Application.Interfaces;
using ChatSystem.Domain.Models;

namespace ChatSystem.Application.UseCases;

public class SendMessageUseCase
{
    private readonly IChatRoomRepository _repository;

    public SendMessageUseCase(IChatRoomRepository repository)
    {
        _repository = repository;
    }

    public ChatRoom Execute(string roomName, string userName, string message)
    {
        var room = _repository.GetRoom(roomName);
        if (room == null)
            throw new Exception($"Room '{roomName}' does not exist.");

        if (!room.Users.Contains(userName))
            throw new Exception($"User '{userName}' is not part of the room '{roomName}'.");

        // Business logic for message handling could be added here (e.g., logging)
        return room;
    }
}