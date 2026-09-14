using System.Numerics;
using System.Collections.Generic;

public class GameState
{
    // PLAYER 1
    private Vector2 player1Position;
    private Vector2 player1targetPosition;

    // PLAYER 2    
    private Vector2 player2Position;
    private Vector2 player2targetPosition;

    // PROJECTILES
    private List<Projectile> projectiles;

    public GameState()
    {
        player1Position = Vector2.Zero;
        player2Position = Vector2.Zero;
        projectiles = new List<Projectile>();
    }
}
