using System.Numerics;

public class ProjectileState
{
    public int ProjectileId { get; set; }
    public int PlayerId { get; set; }
    public float Radius { get; set; } = 0.5f;
    public Vector2 StartPosition { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 EndPosition { get; set; }
    public float Speed { get; set; } = 15;
    public float Range { get; set; } = 20;


    // CONSTRUCTOR
    public ProjectileState(int playerId, Vector2 startPosition, Vector2 direction)
    {
        ProjectileId = Server.Instance.GetNextProjectileId();
        this.PlayerId = playerId;
        this.StartPosition = startPosition;
        this.Position = startPosition;
        EndPosition = Position + direction * Range;
    }

    // METHODS
    public void Move(float deltaTime)
    {
        Vector2 toEnd = EndPosition - Position;
        float distance = toEnd.Length();

        if (distance > 0.01f)
        {
            Vector2 dir = toEnd / distance;
            float moveDistance = Speed * deltaTime;

            Position = moveDistance >= distance ? EndPosition : Position + dir * moveDistance;
        }
    }

    public bool HasExceededRange()
    {
        float distanceTraveled = Vector2.Distance(Position, StartPosition);
        return distanceTraveled >= Range - 0.01f; // small tolerance for float precision
    }
}
