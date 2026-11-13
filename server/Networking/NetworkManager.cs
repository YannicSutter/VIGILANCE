using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Linq;

public class NetworkManager
{
    private readonly NetManager server;
    private readonly EventBasedNetListener listener;
    private readonly Simulation simulation;

    private readonly Dictionary<NetPeer, int> peerToPlayerId = new();
    private readonly Dictionary<int, GameMode> peerToMode = new();
    private readonly Dictionary<int, PeerState> peerToState = new();
    private readonly HashSet<int> availablePlayerIds = new() { 0, 1 };

    private InputFrame player1Input = new InputFrame();
    private InputFrame player2Input = new InputFrame();

    public NetworkManager(int port, Simulation simulation)
    {
        this.simulation = simulation;
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(port);

        listener.ConnectionRequestEvent += request =>
        {
            string key;
            byte modeValue;

            bool hasKey = request.Data.TryGetString(out key);
            bool hasMode = request.Data.TryGetByte(out modeValue);

            if (!hasKey || !hasMode)
            {
                request.Reject();
                return;
            }

            if (key != "VigilanceKey")
            {
                request.Reject();
                return;
            }

            GameMode mode = (GameMode)modeValue;
            if (!Enum.IsDefined(typeof(GameMode), mode))
            {
                request.Reject();
                return;
            }

            var peer = request.Accept();
            peerToMode[peer.Id] = mode;
        };

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

            GameMode mode = GameMode.SinglePlayer;
            if (peerToMode.ContainsKey(peer.Id))
            {
                mode = peerToMode[peer.Id];
            }

            Console.WriteLine($"Player {assignedId} connected ({mode}): {peer}");

            if (mode == GameMode.SinglePlayer)
            {
                // No second human to wait for, so the pop comes right away
                peerToState[peer.Id] = PeerState.WaitingForAccept;
                SendMatchFound(peer);
            }
            else
            {
                // Multiplayer: the matchmaker will send MatchFound later
                peerToState[peer.Id] = PeerState.InQueue;
            }
        };

        listener.PeerDisconnectedEvent += (peer, reason) =>
        {
            peerToMode.Remove(peer.Id);
            peerToState.Remove(peer.Id);

            if (peerToPlayerId.TryGetValue(peer, out int playerId))
            {
                Console.WriteLine($"Player {playerId} disconnected: {reason.Reason}");
                peerToPlayerId.Remove(peer);
                availablePlayerIds.Add(playerId);
            }
        };

        listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {
            HandleMessage(peer, reader);
            reader.Recycle();
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

    private void HandleMessage(NetPeer peer, NetPacketReader reader)
    {
        if (!peerToPlayerId.TryGetValue(peer, out int playerId))
        {
            return;
        }

        if (!peerToState.TryGetValue(peer.Id, out PeerState peerState))
        {
            return;
        }

        byte typeValue;
        if (!reader.TryGetByte(out typeValue))
        {
            return;
        }

        MessageType type = (MessageType)typeValue;

        if (type == MessageType.AcceptMatch)
        {
            // Only valid while this peer is waiting for a pop
            if (peerState != PeerState.WaitingForAccept)
            {
                return;
            }

            peerToState[peer.Id] = PeerState.InMatch;
            SendMatchCreated(peer);
        }
        else if (type == MessageType.Input)
        {
            // Only valid while this peer is in a match
            if (peerState != PeerState.InMatch)
            {
                return;
            }

            InputFrame input = reader.GetInputFrame();
            ApplyInput(playerId, input);
        }
    }

    private void ApplyInput(int playerId, InputFrame input)
    {
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
    }

    private void SendMatchFound(NetPeer peer)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)MessageType.MatchFound);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine($"Sent MatchFound to {peer}");
    }

    private void SendMatchCreated(NetPeer peer)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)MessageType.MatchCreated);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine($"Sent MatchCreated to {peer}");
    }

    private void BroadcastState(GameState state)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)MessageType.State);
        writer.Put(state);

        foreach (var peer in peerToPlayerId.Keys)
        {
            PeerState peerState;
            if (peerToState.TryGetValue(peer.Id, out peerState) && peerState == PeerState.InMatch)
            {
                peer.Send(writer, DeliveryMethod.Unreliable);
            }
        }
    }
}