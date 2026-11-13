using LiteNetLib;
using LiteNetLib.Utils;

public class NetworkManager
{
    private readonly NetManager server;
    private readonly EventBasedNetListener listener;
    private readonly MatchManager matchManager = new MatchManager();

    private readonly Dictionary<int, GameMode> peerToMode = new();
    private readonly Dictionary<int, PeerState> peerToState = new();

    public NetworkManager(int port)
    {
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
            GameMode mode = GameMode.SinglePlayer;
            if (peerToMode.ContainsKey(peer.Id))
            {
                mode = peerToMode[peer.Id];
            }

            Console.WriteLine($"Peer {peer.Id} connected ({mode}): {peer}");

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
            matchManager.RemovePeer(peer);

            Console.WriteLine($"Peer {peer.Id} disconnected: {reason.Reason}");
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
        matchManager.Tick(deltaTime);
    }

    private void HandleMessage(NetPeer peer, NetPacketReader reader)
    {
        PeerState peerState;
        if (!peerToState.TryGetValue(peer.Id, out peerState))
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
            HandleAccept(peer, peerState);
        }
        else if (type == MessageType.Input)
        {
            HandleInput(peer, peerState, reader);
        }
    }

    private void HandleAccept(NetPeer peer, PeerState peerState)
    {
        // Only valid while this peer is waiting for a pop
        if (peerState != PeerState.WaitingForAccept)
        {
            return;
        }

        GameMode mode;
        if (!peerToMode.TryGetValue(peer.Id, out mode))
        {
            return;
        }

        // Multiplayer matches come with the matchmaker
        if (mode != GameMode.SinglePlayer)
        {
            return;
        }

        matchManager.CreateSinglePlayerMatch(peer);
        peerToState[peer.Id] = PeerState.InMatch;
        SendMatchCreated(peer);
    }

    private void HandleInput(NetPeer peer, PeerState peerState, NetPacketReader reader)
    {
        // Only valid while this peer is in a match
        if (peerState != PeerState.InMatch)
        {
            return;
        }

        InputFrame input = reader.GetInputFrame();
        matchManager.RouteInput(peer, input);
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
}