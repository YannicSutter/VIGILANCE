using System.Numerics;
using LiteNetLib.Utils;

public static class Serialization
{
    // Vector2
    public static void Put(this NetDataWriter writer, Vector2 vector)
    {
        writer.Put(vector.X);
        writer.Put(vector.Y);
    }

    public static Vector2 GetVector2(this NetDataReader reader)
    {
        float x = reader.GetFloat();
        float y = reader.GetFloat();
        return new Vector2(x, y);
    }

    // InputFrame (client -> server)
    public static void Put(this NetDataWriter writer, InputFrame input)
    {
        writer.Put(input.targetPosition);
        writer.Put(input.hasMoveTarget);
        writer.Put(input.lookingDirection);
        writer.Put(input.isShooting);
    }

    public static InputFrame GetInputFrame(this NetDataReader reader)
    {
        return new InputFrame
        {
            targetPosition = reader.GetVector2(),
            hasMoveTarget = reader.GetBool(),
            lookingDirection = reader.GetVector2(),
            isShooting = reader.GetBool()
        };
    }

    // GameState (server -> client)
    public static void Put(this NetDataWriter writer, GameState state)
    {
        writer.Put(state.Player1);
        writer.Put(state.Player2);
        writer.Put(state.IsGameOver);
        writer.Put(state.projectilesQ.Count);
        foreach (var projectile in state.projectilesQ)
            writer.Put(projectile);
    }

    public static GameState GetGameState(this NetDataReader reader)
    {
        var player1 = reader.GetPlayerState();
        var player2 = reader.GetPlayerState();
        bool isGameOver = reader.GetBool();

        var state = new GameState(player1, player2) { IsGameOver = isGameOver };

        int projectileCount = reader.GetInt();
        for (int i = 0; i < projectileCount; i++)
            state.projectilesQ.Add(reader.GetProjectileState());

        return state;
    }

    // PlayerState
    public static void Put(this NetDataWriter writer, PlayerState player)
    {
        writer.Put(player.PlayerId);
        writer.Put(player.Position);
        writer.Put(player.Health);
        writer.Put(player.CurrentQCooldown);
    }

    public static PlayerState GetPlayerState(this NetDataReader reader)
    {
        int playerId = reader.GetInt();
        var position = reader.GetVector2();
        var player = new PlayerState(playerId, position);
        player.Health = reader.GetInt();
        player.CurrentQCooldown = reader.GetFloat();
        return player;
    }

    // ProjectileState (only what the client needs to render)
    public static void Put(this NetDataWriter writer, ProjectileState projectile)
    {
        writer.Put(projectile.ProjectileId);
        writer.Put(projectile.Position);
    }

    public static ProjectileState GetProjectileState(this NetDataReader reader)
    {
        int id = reader.GetInt();
        var position = reader.GetVector2();
        var projectile = new ProjectileState(0, position, Vector2.Zero);
        projectile.ProjectileId = id;
        projectile.Position = position;
        return projectile;
    }
}