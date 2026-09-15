using System;
using System.Numerics;

public class Server
{
    // SINGLETON
    private static Server _instance;
    public static Server Instance => _instance ??= new Server();


    // CONSTANTS
    private static Vector2 player1SpawnPosition = new Vector2(-4, 0);
    private static Vector2 player2SpawnPosition = new Vector2(4, 0);
    private int nextProjectileId = 0;


    // INPUTS
    private GameState gameState = new GameState
    (
        new PlayerState(player1SpawnPosition), 
        new PlayerState(player2SpawnPosition)
    );


    // SERVER METHODS
    public GameState Tick(InputFrame input, float deltaTime)
    {
        // INPUT MOVING
        if (input.PlayerId == 0 && input.hasMoveTarget)
            MoveTowards(gameState.Player1, input.targetPosition, deltaTime);
        else if (input.PlayerId == 1 && input.hasMoveTarget)
            MoveTowards(gameState.Player2, input.targetPosition, deltaTime);

        // INPUT SHOOTING
        if (input.PlayerId == 0 && input.isShooting)
            gameState.projectilesQ.Add(new ProjectileState(input.PlayerId, gameState.Player1.Position, Vector2.Normalize(input.lookingDirection)));
        else if (input.PlayerId == 1 && input.isShooting)
            gameState.projectilesQ.Add(new ProjectileState(input.PlayerId, gameState.Player2.Position, Vector2.Normalize(input.lookingDirection)));

        // PROJECTILE MOVING
        foreach (var projectileQ in gameState.projectilesQ)
        {
            projectileQ.Move(deltaTime);
        }

        // PROJECTILE REMOVING
        gameState.projectilesQ.RemoveAll(p => p.HasExceededRange());

        return gameState;
    }

    private void MoveTowards(PlayerState player, Vector2 target, float deltaTime)
{
    Vector2 toTarget = target - player.Position;
    float distance = toTarget.Length();

    if (distance > 0.01f)
    {
        Vector2 direction = toTarget / distance;
        float moveDistance = player.MovementSpeed * deltaTime;

        player.Position = moveDistance >= distance
            ? target
            : player.Position + direction * moveDistance;
    }
}

    public int GetNextProjectileId()
    {
        return nextProjectileId++;
    }
}
