using ChatSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Application.Interfaces;

public interface IChatRoomRepository
{
    ChatRoom? GetRoom(string name);

    void AddRoom(ChatRoom room);

    void RemoveRoom(string name);

    IEnumerable<ChatRoom> GetAllRooms();
}