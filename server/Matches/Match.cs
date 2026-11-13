using LiteNetLib;
using LiteNetLib.Utils;

public class Match
{
    public int MatchId { get; private set; }
    private Simulation simulation  = new Simulation();
    private Queue<InputFrame> actionQueuePlayer1 = new Queue<InputFrame>();
    private Queue<InputFrame> actionQueuePlayer2 = new Queue<InputFrame>();
    private NetPeer?[] peers = new NetPeer?[2];
    const int MaxQueuedFrames = 100;
    private bool hasLoggedOverflowPlayer1 = false;
    private bool hasLoggedOverflowPlayer2 = false;
    private bool isFinished = false;

    public Match(int matchId, NetPeer? player1, NetPeer? player2)
    {
        this.MatchId = matchId;
        peers[0] = player1;
        peers[1] = player2;
    }

    // Which slot does this peer have? Returns 0 or 1, or -1 if the peer is not in this match
    public int GetSlot(NetPeer peer)
    {
        if (peer == null)
        {
            return -1;
        }
        else if (peer == peers[0])
        {
            return 0;
        }
        else if (peer == peers[1])
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    // Puts a frame from the given slot into that slot's queue
    public void EnqueueInput(int slot, InputFrame input)
    {
        if (slot == 0)
        {
            if (actionQueuePlayer1.Count < MaxQueuedFrames)
            {
                actionQueuePlayer1.Enqueue(input);
            }
            else if (!hasLoggedOverflowPlayer1)
            {
                hasLoggedOverflowPlayer1 = true;
                Console.WriteLine($"Match {MatchId}: input queue of slot 0 is full, refusing frames");
            }
        }
        else if (slot == 1)
        {
            if (actionQueuePlayer2.Count < MaxQueuedFrames)
            {
                actionQueuePlayer2.Enqueue(input);
            }
            else if (!hasLoggedOverflowPlayer2)
            {
                hasLoggedOverflowPlayer2 = true;
                Console.WriteLine($"Match {MatchId}: input queue of slot 1 is full, refusing frames");
            }
        }
    }

    // One server tick: apply the queued frames, step the world, send the state
    public void Tick(float deltaTime)
    {
        if (isFinished)
        {
            return;
        }

        // 1. Apply every queued frame, in the order it arrived
        DrainQueue(actionQueuePlayer1, 0);
        DrainQueue(actionQueuePlayer2, 1);

        // 2. Advance the world once
        GameState state = simulation.StepWorld(deltaTime);

        // 3. Send the result to the human players
        SendState(state);

        if (state.IsGameOver)
        {
            isFinished = true;
        }
    }

    private void DrainQueue(Queue<InputFrame> queue, int slot)
    {
        while (queue.Count > 0)
        {
            InputFrame frame = queue.Dequeue();
            simulation.ApplyInput(slot, frame);
        }
    }

    private void SendState(GameState state)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)MessageType.State);
        writer.Put(state);

        for (int slot = 0; slot < 2; slot++)
        {
            NetPeer? peer = peers[slot];

            if (peer != null)
            {
                peer.Send(writer, DeliveryMethod.Unreliable);
            }
        }
    }
}