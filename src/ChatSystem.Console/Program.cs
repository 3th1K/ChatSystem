using ChatSystem.Application.Interfaces;
using ChatSystem.Infrastructure.Repositories;
using ChatSystem.Infrastructure.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

if (args.Contains("--server"))
{
    Console.WriteLine("Starting chat server...");
    await StartServerAsync();
}
else if (args.Contains("--client"))
{
    Console.WriteLine("Starting chat client...");
    await StartClientAsync();
}
else
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  chat.exe --server       Start the chat server.");
    Console.WriteLine("  chat.exe --client       Start the chat client.");
}

async Task StartServerAsync()
{
    Console.WriteLine("Initializing Chat Server...");
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<IChatRoomRepository, InMemoryChatRoomRepository>();

    var app = builder.Build();
    app.MapHub<ChatHub>("/chat");

    Console.WriteLine("Server is running on http://localhost:5000");
    await app.RunAsync();
}
async Task StartClientAsync()
{
    Console.WriteLine("Initializing Chat Client...");
    // SignalR Hub Connection
    HubConnection connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/chat")
        .WithAutomaticReconnect()
        .Build();

    string currentRoom = string.Empty;
    string userName = string.Empty;
    bool isRunning = true;

    RegisterHubHandlers(connection);

    try
    {
        await connection.StartAsync();
        Console.WriteLine("Connected to Chat Server. Type 'help' for a list of commands.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect to the server: {ex.Message}");
        return;
    }

    // Command Processing Loop
    while (isRunning)
    {
        Console.Write("> ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) continue;

        var commandParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = commandParts[0].ToLower();
        var arguments = commandParts.Skip(1).ToArray();

        try
        {
            await ProcessCommand(command, arguments, connection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // Registers SignalR Hub event handlers
    void RegisterHubHandlers(HubConnection hubConnection)
    {
        hubConnection.On<string>("RoomCreated", roomName =>
        {
            Console.WriteLine($"Room '{roomName}' has been created.");
        });

        hubConnection.On<string>("UserJoined", userName =>
        {
            Console.WriteLine($"# {userName} has joined the chat.");
        });

        hubConnection.On<string, string>("ReceiveMessage", (sender, message) =>
        {
            Console.WriteLine($"{sender}> {message}");
        });

        hubConnection.On<string>("UserLeft", userName =>
        {
            Console.WriteLine($"# {userName} has left the chat.");
        });
    }

    // Processes commands entered by the user
    async Task ProcessCommand(string command, string[] args, HubConnection hubConnection)
    {
        switch (command)
        {
            case "help":
                ShowHelp();
                break;

            case "create":
                if (args.Length < 2) throw new ArgumentException("Usage: create <roomName> <userName>");
                var createRoomName = args[0];
                userName = args[1];
                await hubConnection.InvokeAsync("CreateRoom", createRoomName, userName);
                currentRoom = createRoomName;
                Console.WriteLine($"# You created and joined room '{currentRoom}' as '{userName}'.");
                break;

            case "join":
                if (args.Length < 2) throw new ArgumentException("Usage: join <roomName> <userName>");
                var joinRoomName = args[0];
                userName = args[1];
                await hubConnection.InvokeAsync("JoinRoom", joinRoomName, userName);
                currentRoom = joinRoomName;
                Console.WriteLine($"# You joined room '{currentRoom}' as '{userName}'.");
                break;

            case "send":
                if (string.IsNullOrWhiteSpace(currentRoom)) throw new InvalidOperationException("You must join a room first.");
                if (args.Length < 1) throw new ArgumentException("Usage: send <message>");
                var message = string.Join(' ', args);
                await hubConnection.InvokeAsync("SendMessage", currentRoom, userName, message);
                break;

            case "leave":
                if (string.IsNullOrWhiteSpace(currentRoom)) throw new InvalidOperationException("You are not in a room.");
                await hubConnection.InvokeAsync("LeaveRoom", currentRoom, userName);
                Console.WriteLine($"# You left room '{currentRoom}'.");
                currentRoom = string.Empty;
                break;

            case "exit":
                if (!string.IsNullOrWhiteSpace(currentRoom))
                {
                    await hubConnection.InvokeAsync("LeaveRoom", currentRoom, userName);
                    Console.WriteLine($"# You left room '{currentRoom}'.");
                }
                isRunning = false;
                await hubConnection.StopAsync();
                Console.WriteLine("Disconnected from server. Exiting...");
                break;

            default:
                Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                break;
        }
    }

    // Displays available commands
    void ShowHelp()
    {
        Console.WriteLine("Available Commands:");
        Console.WriteLine("  create <roomName> <userName>: Create a room and join it as the specified user.");
        Console.WriteLine("  join <roomName> <userName>  : Join an existing room as the specified user.");
        Console.WriteLine("  send <message>             : Send a message to the current room.");
        Console.WriteLine("  leave                      : Leave the current room.");
        Console.WriteLine("  exit                       : Disconnect and exit the application.");
    }
}