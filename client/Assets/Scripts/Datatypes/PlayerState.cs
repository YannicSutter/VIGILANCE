using System.Numerics;

public class PlayerState
{
    public int PlayerId { get; set; }
    public float Radius { get; set; } = 0.5f;
    public Vector2 Position { get; set; }
    public int Health { get; set; } = 5;
    public float MovementSpeed { get; set; } = 5;
    public float QCooldown { get; set; } = 5;
    public float CurrentQCooldown {get; set; } = 0;


    // CONSTRUCTOR
    public PlayerState(int playerId, Vector2 position)
    {
        PlayerId = playerId;
        Position = position;
    }

    public void ReduceHealth()
    {
        Health--;
    }
}
