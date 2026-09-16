using LiteNetLib;

int port = 9050;
int tickRate = 30;

Simulation simulation = new Simulation();
NetworkManager networkManager = new NetworkManager(port, simulation);

Console.WriteLine($"Server listening on port {port}, tick rate {tickRate} Hz...");

GameLoop gameLoop = new GameLoop(tickRate);
gameLoop.Run(
    onPoll: () => networkManager.Poll(),
    onTick: (deltaTime) => networkManager.Tick(deltaTime)
);
