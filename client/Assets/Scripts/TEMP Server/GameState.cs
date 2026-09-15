using System.Collections.Generic;

public class GameState
{
    // PLAYERS
    public PlayerState Player1 {get; set; }
    public PlayerState Player2 {get; set; }


    // PROJECTILES
    public List<ProjectileState> projectilesQ { get; set; }


    // CONSTRUCTOR
    public GameState(PlayerState player1, PlayerState player2)
    {
        this.Player1 = player1;
        this.Player2 = player2;
    
        projectilesQ = new List<ProjectileState>();
    }

}
