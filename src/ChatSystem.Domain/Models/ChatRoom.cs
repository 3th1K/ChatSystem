namespace ChatSystem.Domain.Models;

public class ChatRoom
{
    public string Name { get; }
    private readonly HashSet<string> _users = new();

    public ChatRoom(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void AddUser(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be empty.", nameof(userName));

        _users.Add(userName);
    }

    public void RemoveUser(string userName)
    {
        _users.Remove(userName);
    }

    public IEnumerable<string> Users => _users;
}