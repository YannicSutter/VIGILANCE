using System;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }

    public event Action Connected;
    public event Action<DisconnectReason> Disconnected;

    private NetManager client;
    private EventBasedNetListener listener;
    private NetPeer serverPeer;

    public GameState LatestState { get; private set; }
    public bool IsConnected => serverPeer != null && serverPeer.ConnectionState == ConnectionState.Connected;

    public int Ping
    {
        get
        {
            if (serverPeer == null)
            {
                return -1;
            }

            return serverPeer.Ping;
        }
    }

    private string serverAddress = "83.228.210.108";
    private int serverPort = 9050;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();

        listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {
            LatestState = reader.GetGameState();
            reader.Recycle();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Debug.Log($"Connected to server: {peer}");
            serverPeer = peer;

            if (Connected != null)
            {
                Connected.Invoke();
            }
        };

        listener.PeerDisconnectedEvent += (peer, reason) =>
        {
            Debug.Log($"Disconnected from server: {reason.Reason}");
            serverPeer = null;
            LatestState = null;

            if (Disconnected != null)
            {
                Disconnected.Invoke(reason.Reason);
            }
        };
    }

    private void Update()
    {
        client.PollEvents();
    }

    public void Connect(GameMode mode)
    {
        if (client == null || IsConnected)
        {
            return;
        }

        NetDataWriter connectionData = new NetDataWriter();
        connectionData.Put("VigilanceKey");
        connectionData.Put((byte)mode);

        client.Connect(serverAddress, serverPort, connectionData);
    }

    public void Disconnect()
    {
        if (client != null)
        {
            client.DisconnectAll();
        }

        serverPeer = null;
        LatestState = null;
    }

    public void SendInput(InputFrame input)
    {
        if (!IsConnected)
        {
            return;
        }

        NetDataWriter writer = new NetDataWriter();
        writer.Put(input);

        DeliveryMethod method;
        if (input.isShooting)
        {
            method = DeliveryMethod.ReliableOrdered;
        }
        else
        {
            method = DeliveryMethod.Unreliable;
        }

        serverPeer.Send(writer, method);
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.Stop();
        }
    }
}