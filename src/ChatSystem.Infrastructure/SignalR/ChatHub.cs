using ChatSystem.Application.Interfaces;
using ChatSystem.Application.UseCases;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatSystem.Infrastructure.SignalR;

public enum ChatRoomResult
{
    JoinRoomSuccess,
    JoinRoomFailed
}

public class ChatHub : Hub
{
    private readonly IChatRoomRepository _repository;

    public ChatHub(IChatRoomRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateRoom(string roomName, string userName)
    {
        // Use the CreateRoomUseCase to handle the logic
        var createUseCase = new CreateRoomUseCase(_repository);
        createUseCase.Execute(roomName);

        // Add the creator as the first user
        var joinUseCase = new JoinRoomUseCase(_repository);
        joinUseCase.Execute(roomName, userName);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Caller.SendAsync("RoomCreated", roomName);
        await Clients.Caller.SendAsync("UserJoined", userName);
    }

    public async Task JoinRoom(string roomName, string userName)
    {
        var joinUseCase = new JoinRoomUseCase(_repository);
        joinUseCase.Execute(roomName, userName);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserJoined", userName);
    }

    public async Task SendMessage(string roomName, string userName, string message)
    {
        var sendMessageUseCase = new SendMessageUseCase(_repository);
        var room = sendMessageUseCase.Execute(roomName, userName, message);

        // Broadcast the message to the group
        await Clients.Group(roomName).SendAsync("ReceiveMessage", userName, message);
    }

    public async Task LeaveRoom(string roomName, string userName)
    {
        var room = _repository.GetRoom(roomName);
        if (room == null)
            throw new Exception($"Room '{roomName}' does not exist.");

        room.RemoveUser(userName);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserLeft", userName);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Handle logic for disconnected users (optional cleanup)
        await base.OnDisconnectedAsync(exception);
    }
}