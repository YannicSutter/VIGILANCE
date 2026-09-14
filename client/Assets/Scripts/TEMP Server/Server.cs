using System;

public class Server
{
    // SINGLETON
    private static Server _instance;
    public static Server Instance => _instance ??= new Server();


    public GameState Tick(InputFrame input, float deltaTime)
    {
        Console.WriteLine("Server Tick");
        
        return null;
    }
}
