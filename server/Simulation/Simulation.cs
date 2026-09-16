using System.Numerics;

public class Simulation
{
    // VARIABLES
    private static Vector2 player1SpawnPosition = new Vector2(-4, 0);
    private static Vector2 player2SpawnPosition = new Vector2(4, 0);
    private int nextProjectileId = 0;
    private Tuple<float, float> arenaPlayer1 = new Tuple<float, float>(-7, 4.5f);
    private Tuple<float, float> arenaPlayer2 = new Tuple<float, float>(7, 4.5f);

    private GameState gameState = new GameState
    (
        new PlayerState(0, player1SpawnPosition),
        new PlayerState(1, player2SpawnPosition)
    );

    // SIMULATION METHODS
    public GameState Tick(InputFrame player1Input, InputFrame player2Input, float deltaTime)
    {
        // INPUT MOVING
        if (player1Input.hasMoveTarget)
            MoveTowards(gameState.Player1, player1Input.targetPosition, deltaTime);
        if (player2Input.hasMoveTarget)
            MoveTowards(gameState.Player2, player2Input.targetPosition, deltaTime);

        // COOLDOWNS
        gameState.Player1.CurrentQCooldown = Math.Clamp(gameState.Player1.CurrentQCooldown - deltaTime, 0, gameState.Player1.QCooldown);
        gameState.Player2.CurrentQCooldown = Math.Clamp(gameState.Player2.CurrentQCooldown - deltaTime, 0, gameState.Player2.QCooldown);

        // INPUT SHOOTING
        if (player1Input.isShooting && gameState.Player1.CurrentQCooldown <= 0)
        {
            gameState.projectilesQ.Add(CreateProjectile(0, gameState.Player1.Position, Vector2.Normalize(player1Input.lookingDirection)));
            gameState.Player1.CurrentQCooldown = gameState.Player1.QCooldown;
        }
        if (player2Input.isShooting && gameState.Player2.CurrentQCooldown <= 0)
        {
            gameState.projectilesQ.Add(CreateProjectile(1, gameState.Player2.Position, Vector2.Normalize(player2Input.lookingDirection)));
            gameState.Player2.CurrentQCooldown = gameState.Player2.QCooldown;
        }

        // PROJECTILE MOVING
        foreach (var projectileQ in gameState.projectilesQ)
        {
            projectileQ.Move(deltaTime);
        }

        // PROJECTILE REMOVING
        gameState.projectilesQ.RemoveAll(p => p.HasExceededRange());

        // COLLISIONS
        CheckCollisions();
        IsGameOver();

        return gameState;
    }

    private ProjectileState CreateProjectile(int playerId, Vector2 startPosition, Vector2 direction)
    {
        var projectile = new ProjectileState(playerId, startPosition, direction);
        projectile.ProjectileId = nextProjectileId++;
        return projectile;
    }

    private void MoveTowards(PlayerState player, Vector2 target, float deltaTime)
    {
        Vector2 clampedTarget;

        if (player.PlayerId == 0)
            clampedTarget = new Vector2(
                Math.Clamp(target.X, arenaPlayer1.Item1, -1),
                Math.Clamp(target.Y, -arenaPlayer1.Item2, arenaPlayer1.Item2));
        else
            clampedTarget = new Vector2(
                Math.Clamp(target.X, 1, arenaPlayer2.Item1),
                Math.Clamp(target.Y, -arenaPlayer2.Item2, arenaPlayer2.Item2));

        Vector2 toTarget = clampedTarget - player.Position;
        float distance = toTarget.Length();

        if (distance > 0.01f)
        {
            Vector2 direction = toTarget / distance;
            float moveDistance = player.MovementSpeed * deltaTime;

            player.Position = moveDistance >= distance
                ? clampedTarget
                : player.Position + direction * moveDistance;
        }
    }

    // ABILITY COLLISIONS
    private void CheckCollisions()
    {
        List<ProjectileState> toRemove = new List<ProjectileState>();

        foreach (var projectile in gameState.projectilesQ)
        {
            if (projectile.PlayerId == 0)
            {
                if (Vector2.Distance(gameState.Player2.Position, projectile.Position) <= gameState.Player2.Radius + projectile.Radius)
                {
                    gameState.Player2.ReduceHealth();
                    toRemove.Add(projectile);
                }
            }
            else if (projectile.PlayerId == 1)
            {
                if (Vector2.Distance(gameState.Player1.Position, projectile.Position) <= gameState.Player1.Radius + projectile.Radius)
                {
                    gameState.Player1.ReduceHealth();
                    toRemove.Add(projectile);
                }
            }
        }

        foreach (var projectile in toRemove)
        {
            gameState.projectilesQ.Remove(projectile);
        }
    }

    // PLAYER HEALTH
    private void IsGameOver()
    {
        if (gameState.Player1.Health <= 0 || gameState.Player2.Health <= 0)
            gameState.IsGameOver = true;
    }

    public int GetNextProjectileId()
    {
        return nextProjectileId++;
    }
}