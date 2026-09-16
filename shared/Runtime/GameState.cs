using System.Collections.Generic;

public class GameState
{
    // VARIABLES
    public PlayerState Player1 {get; set; }
    public PlayerState Player2 {get; set; }
    public List<ProjectileState> projectilesQ { get; set; }
    public bool IsGameOver { get; set; } = false;


    // CONSTRUCTOR
    public GameState(PlayerState player1, PlayerState player2)
    {
        this.Player1 = player1;
        this.Player2 = player2;
    
        projectilesQ = new List<ProjectileState>();
    }

}
