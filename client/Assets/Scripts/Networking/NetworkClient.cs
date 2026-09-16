using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }

    private NetManager client;
    private EventBasedNetListener listener;
    private NetPeer serverPeer;

    public GameState LatestState { get; private set; }
    public bool IsConnected => serverPeer != null && serverPeer.ConnectionState == ConnectionState.Connected;

    private string serverAddress = "83.228.210.108"; 
    private int serverPort = 9050;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
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
        };

        listener.PeerDisconnectedEvent += (peer, reason) =>
        {
            Debug.Log($"Disconnected from server: {reason}");
            Debug.Log($"Disconnected from server: {reason.Reason}");
            serverPeer = null;
        };

        client.Connect(serverAddress, serverPort, "VigilanceKey");
    }

    private void Update()
    {
        client.PollEvents();
    }

    public void SendInput(InputFrame input)
    {
        if (!IsConnected) return;

        NetDataWriter writer = new NetDataWriter();
        writer.Put(input);

        DeliveryMethod method = input.isShooting ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
        serverPeer.Send(writer, method);
    }

    private void OnDestroy()
    {
        client?.Stop();
    }
}