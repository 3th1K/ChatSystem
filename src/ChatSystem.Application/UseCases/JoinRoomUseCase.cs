using ChatSystem.Application.Interfaces;

namespace ChatSystem.Application.UseCases;

public class JoinRoomUseCase
{
    private readonly IChatRoomRepository _repository;

    public JoinRoomUseCase(IChatRoomRepository repository)
    {
        _repository = repository;
    }

    public void Execute(string roomName, string userName)
    {
        var room = _repository.GetRoom(roomName);
        if (room == null)
            throw new Exception($"Room '{roomName}' does not exist.");

        room.AddUser(userName);
    }
}