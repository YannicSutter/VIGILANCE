using LiteNetLib;

public class MatchManager
{
    private readonly List<Match> matches = new List<Match>();
    private readonly Dictionary<int, Match> peerIdToMatch = new Dictionary<int, Match>();
    private int nextMatchId = 1;

    public void CreateSinglePlayerMatch(NetPeer peer)
    {
        // Slot 1 stays empty for now (later: the bot)
        Match match = new Match(nextMatchId, peer, null);
        nextMatchId++;

        matches.Add(match);
        peerIdToMatch[peer.Id] = match;

        Console.WriteLine($"Match {match.MatchId} started (singleplayer), peer {peer.Id}");
    }

    public void RouteInput(NetPeer peer, InputFrame input)
    {
        if (!peerIdToMatch.TryGetValue(peer.Id, out Match? match))
        {
            return;
        }

        int slot = match.GetSlot(peer);
        if (slot < 0)
        {
            return;
        }

        match.EnqueueInput(slot, input);
    }

    public void RemovePeer(NetPeer peer)
    {
        if (!peerIdToMatch.TryGetValue(peer.Id, out Match? match))
        {
            return;
        }

        // Singleplayer: the match ends with its only human.
        // TODO multiplayer: the other player has to be told.
        peerIdToMatch.Remove(peer.Id);
        matches.Remove(match);

        Console.WriteLine($"Match {match.MatchId} ended");
    }

    public void Tick(float deltaTime)
    {
        foreach (Match match in matches)
        {
            match.Tick(deltaTime);
        }
    }
}