using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class GameManager : MonoBehaviour
{
    // SERIALIZED FIELDS
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private InputHandler inputHandler;

    // VARIABLES
    private GameObject player1;
    private GameObject player2;
    private Dictionary<int, GameObject> projectileVisuals = new Dictionary<int, GameObject>();


    // UNITY METHODS
    private void FixedUpdate()
    {
        InputFrame input = inputHandler.ConsumeInput();
        BuildGameState(SendInputToServer(input));
    }


    // BUILD GAMESTATE
    private void BuildGameState(GameState gameState)
    {
        SyncPlayersVisuals(gameState);
        SyncProjectileVisuals(gameState);
    }

    private void SyncPlayersVisuals(GameState gameState)
    {
        Vector3 p1World = gameState.Player1.Position.ToUnityWorld();
        Vector3 p2World = gameState.Player2.Position.ToUnityWorld();

        if (player1 == null)
            player1 = Instantiate(playerPrefab, p1World, Quaternion.identity);
        else
            player1.transform.position = p1World;

        if (player2 == null)
            player2 = Instantiate(playerPrefab, p2World, Quaternion.identity);
        else
            player2.transform.position = p2World;
    }

    private void SyncProjectileVisuals(GameState gameState)
    {
        var currentIds = new HashSet<int>(gameState.projectilesQ.Select(p => p.projectileId));

        var idsToRemove = projectileVisuals.Keys.Where(id => !currentIds.Contains(id)).ToList();
        foreach (int id in idsToRemove)
        {
            Destroy(projectileVisuals[id]);
            projectileVisuals.Remove(id);
        }

        foreach (var projectile in gameState.projectilesQ)
        {
            Vector3 worldPos = projectile.position.ToUnityWorld(1f);

            if (!projectileVisuals.ContainsKey(projectile.projectileId))
            {
                GameObject obj = Instantiate(projectilePrefab, worldPos, Quaternion.identity);
                projectileVisuals[projectile.projectileId] = obj;
            }
            else
            {
                projectileVisuals[projectile.projectileId].transform.position = worldPos;
            }
        }
    }


    // SERVER CONNECTION
    private GameState SendInputToServer(InputFrame input)
    {
        return Server.Instance.Tick(input, Time.fixedDeltaTime);
    }
}
