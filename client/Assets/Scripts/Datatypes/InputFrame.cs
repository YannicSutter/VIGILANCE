using System.Numerics;

public struct InputFrame
{
    // TODO: Delete playerId when server connects to client and assigns it automatically
    public int PlayerId;
    public Vector2 targetPosition;
    public bool hasMoveTarget;
    public Vector2 lookingDirection;
    public bool isShooting;
}

