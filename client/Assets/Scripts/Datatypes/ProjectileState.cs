using System.Numerics;

public class ProjectileState
{
    public int projectileId { get; set; }
    public int playerId { get; set; }
    public Vector2 startPosition { get; set; }
    public Vector2 position { get; set; }
    public Vector2 endPosition { get; set; }
    public float speed { get; set; } = 10;
    public float range { get; set; } = 10;


    // CONSTRUCTOR
    public ProjectileState(int playerId, Vector2 startPosition, Vector2 direction)
    {
        projectileId = Server.Instance.GetNextProjectileId();
        this.playerId = playerId;
        this.startPosition = startPosition;
        this.position = startPosition;
        endPosition = position + direction * range;
    }

    // METHODS
    public void Move(float deltaTime)
    {
        Vector2 toEnd = endPosition - position;
        float distance = toEnd.Length();

        if (distance > 0.01f)
        {
            Vector2 dir = toEnd / distance;
            float moveDistance = speed * deltaTime;

            position = moveDistance >= distance ? endPosition : position + dir * moveDistance;
        }
    }

    public bool HasExceededRange()
    {
        float distanceTraveled = Vector2.Distance(position, startPosition);
        return distanceTraveled >= range - 0.01f; // small tolerance for float precision
    }
}
