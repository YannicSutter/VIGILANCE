using System.Numerics;

public class PlayerState
{
    public Vector2 Position { get; set; }
    public float MovementSpeed { get; set; }


    // CONSTRUCTOR
    public PlayerState(Vector2 position)
    {
        Position = position;
        MovementSpeed = 5;
    }
}
