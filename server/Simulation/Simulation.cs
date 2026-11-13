using System.Numerics;

public class Simulation
{
    // CONSTANTS
    private const float MatchDurationSeconds = 60f;

    // VARIABLES
    private static Vector2 player1SpawnPosition = new Vector2(-4, 0);
    private static Vector2 player2SpawnPosition = new Vector2(4, 0);
    private int nextProjectileId = 0;
    private Tuple<float, float> arenaPlayer1 = new Tuple<float, float>(-7, 4.5f);
    private Tuple<float, float> arenaPlayer2 = new Tuple<float, float>(7, 4.5f);
    private Vector2[] targetPositions = new Vector2[2];
    private bool[] hasMoveTargets = new bool[2];

    private GameState gameState = new GameState
    (
        new PlayerState(0, player1SpawnPosition),
        new PlayerState(1, player2SpawnPosition)
    );

    // CONSTRUCTOR
    public Simulation()
    {
        gameState.TimeRemaining = MatchDurationSeconds;
    }

    // APPLY ONE INPUT (once per frame from the queue)
    public void ApplyInput(int slot, InputFrame input)
    {
        if (slot != 0 && slot != 1)
        {
            return;
        }

        PlayerState player = GetPlayer(slot);

        // MOVE TARGET
        if (input.hasMoveTarget)
        {
            targetPositions[slot] = input.targetPosition;
            hasMoveTargets[slot] = true;
        }

        // SHOOTING
        bool hasDirection = input.lookingDirection != Vector2.Zero;

        if (input.isShooting && hasDirection && player.CurrentQCooldown <= 0)
        {
            Vector2 direction = Vector2.Normalize(input.lookingDirection);
            gameState.ProjectilesQ.Add(CreateProjectile(slot, player.Position, direction));
            player.CurrentQCooldown = player.QCooldown;
        }
    }

    // STEP THE WORLD (once per tick)
    public GameState StepWorld(float deltaTime)
    {
        // PLAYER MOVING
        if (hasMoveTargets[0])
        {
            MoveTowards(gameState.Player1, targetPositions[0], deltaTime);
        }
        if (hasMoveTargets[1])
        {
            MoveTowards(gameState.Player2, targetPositions[1], deltaTime);
        }

        // COOLDOWNS
        gameState.Player1.CurrentQCooldown = Math.Clamp(gameState.Player1.CurrentQCooldown - deltaTime, 0, gameState.Player1.QCooldown);
        gameState.Player2.CurrentQCooldown = Math.Clamp(gameState.Player2.CurrentQCooldown - deltaTime, 0, gameState.Player2.QCooldown);

        // PROJECTILE MOVING
        foreach (var projectileQ in gameState.ProjectilesQ)
        {
            projectileQ.Move(deltaTime);
        }

        // PROJECTILE REMOVING
        gameState.ProjectilesQ.RemoveAll(p => p.HasExceededRange());

        // COLLISIONS
        CheckCollisions();

        // MATCH TIMER
        UpdateTimer(deltaTime);

        return gameState;
    }

    private void UpdateTimer(float deltaTime)
    {
        gameState.TimeRemaining = Math.Max(0, gameState.TimeRemaining - deltaTime);

        if (gameState.TimeRemaining <= 0)
        {
            gameState.IsGameOver = true;
        }
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

        foreach (var projectile in gameState.ProjectilesQ)
        {
            if (projectile.PlayerId == 0)
            {
                if (Vector2.Distance(gameState.Player2.Position, projectile.Position) <= gameState.Player2.Radius + projectile.Radius)
                {
                    gameState.Player1.IncreaseScore();
                    toRemove.Add(projectile);
                }
            }
            else if (projectile.PlayerId == 1)
            {
                if (Vector2.Distance(gameState.Player1.Position, projectile.Position) <= gameState.Player1.Radius + projectile.Radius)
                {
                    gameState.Player2.IncreaseScore();
                    toRemove.Add(projectile);
                }
            }
        }

        foreach (var projectile in toRemove)
        {
            gameState.ProjectilesQ.Remove(projectile);
        }
    }

    private PlayerState GetPlayer(int slot)
    {
        if (slot == 0)
        {
            return gameState.Player1;
        }

        return gameState.Player2;
    }
}