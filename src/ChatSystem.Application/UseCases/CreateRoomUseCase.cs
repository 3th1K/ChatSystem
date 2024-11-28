using ChatSystem.Application.Interfaces;
using ChatSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Application.UseCases;

public class CreateRoomUseCase
{
    private readonly IChatRoomRepository _repository;

    public CreateRoomUseCase(IChatRoomRepository repository)
    {
        _repository = repository;
    }

    public void Execute(string roomName)
    {
        if (_repository.GetRoom(roomName) != null)
            throw new Exception($"Room '{roomName}' already exists.");

        var room = new ChatRoom(roomName);
        _repository.AddRoom(room);
    }
}