using LiteNetLib;
using LiteNetLib.Utils;

public class NetworkManager
{
    private readonly NetManager server;
    private readonly EventBasedNetListener listener;
    private readonly Simulation simulation;

    private readonly Dictionary<NetPeer, int> peerToPlayerId = new();
    private readonly HashSet<int> availablePlayerIds = new() { 0, 1 };

    private InputFrame player1Input = new InputFrame();
    private InputFrame player2Input = new InputFrame();

    public NetworkManager(int port, Simulation simulation)
    {
        this.simulation = simulation;
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(port);

        listener.ConnectionRequestEvent += request => request.AcceptIfKey("VigilanceKey");

        listener.PeerConnectedEvent += peer =>
        {
            if (availablePlayerIds.Count == 0)
            {
                Console.WriteLine($"Rejected connection, server full: {peer}");
                peer.Disconnect();
                return;
            }

            int assignedId = availablePlayerIds.Min();
            availablePlayerIds.Remove(assignedId);
            peerToPlayerId[peer] = assignedId;
            Console.WriteLine($"Player {assignedId} connected: {peer}");
        };

        listener.PeerDisconnectedEvent += (peer, reason) =>
        {
            if (peerToPlayerId.TryGetValue(peer, out int playerId))
            {
                Console.WriteLine($"Player {playerId} disconnected: {reason.Reason}");
                peerToPlayerId.Remove(peer);
                availablePlayerIds.Add(playerId);
            }
        };

        listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {
            if (!peerToPlayerId.TryGetValue(peer, out int playerId))
            {
                reader.Recycle();
                return;
            }

            InputFrame input = reader.GetInputFrame();
            reader.Recycle();

            if (playerId == 0)
            {
                player1Input.targetPosition = input.targetPosition;
                player1Input.hasMoveTarget = input.hasMoveTarget;
                player1Input.lookingDirection = input.lookingDirection;
                player1Input.isShooting = player1Input.isShooting || input.isShooting;
            }
            else
            {
                player2Input.targetPosition = input.targetPosition;
                player2Input.hasMoveTarget = input.hasMoveTarget;
                player2Input.lookingDirection = input.lookingDirection;
                player2Input.isShooting = player2Input.isShooting || input.isShooting;
            }
        };
    }

    public void Poll()
    {
        server.PollEvents();
    }

    public void Tick(float deltaTime)
    {
        GameState state = simulation.Tick(player1Input, player2Input, deltaTime);
        BroadcastState(state);

            player1Input.isShooting = false;
            player2Input.isShooting = false;
    }

    private void BroadcastState(GameState state)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(state);

        foreach (var peer in peerToPlayerId.Keys)
        {
            peer.Send(writer, DeliveryMethod.Unreliable);
        }
    }
}