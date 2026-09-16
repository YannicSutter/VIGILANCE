using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;

public class NetworkManager
{
    private readonly NetManager server;
    private readonly EventBasedNetListener listener;
    private readonly Simulation simulation;

    private readonly Dictionary<NetPeer, int> peerToPlayerId = new();
    private int nextPlayerId = 0;

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
            if (nextPlayerId > 1)
            {
                Console.WriteLine($"Rejected connection, server full: {peer}");
                peer.Disconnect();
                return;
            }

            int assignedId = nextPlayerId++;
            peerToPlayerId[peer] = assignedId;
            Console.WriteLine($"Player {assignedId} connected: {peer}");
        };

        listener.PeerDisconnectedEvent += (peer, reason) =>
        {
            if (peerToPlayerId.TryGetValue(peer, out int playerId))
            {
                Console.WriteLine($"Player {playerId} disconnected: {reason}");
                peerToPlayerId.Remove(peer);
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

            if (playerId == 0) player1Input = input;
            else player2Input = input;
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