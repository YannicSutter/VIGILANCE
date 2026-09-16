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
    public GameObject Player1 {get; private set;}
    public  GameObject Player2 {get; private set;}
    private Dictionary<int, GameObject> projectileVisuals = new Dictionary<int, GameObject>();
    private GameState gameState = new GameState(null, null);


    // UNITY METHODS
    private void FixedUpdate()
    {
        if (gameState != null && !gameState.IsGameOver)
        {
            InputFrame input = inputHandler.ConsumeInput();
            NetworkClient.Instance.SendInput(input);

            if (NetworkClient.Instance.LatestState != null)
            {
                gameState = NetworkClient.Instance.LatestState;
                BuildGameState(gameState);
            }
        }
        Debug.Log($"Ping: {NetworkClient.Instance.Ping}ms");
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

        if (Player1 == null)
            Player1 = Instantiate(playerPrefab, p1World, Quaternion.identity);
        else
            Player1.transform.position = p1World;

        if (Player2 == null)
            Player2 = Instantiate(playerPrefab, p2World, Quaternion.identity);
        else
            Player2.transform.position = p2World;
    }

    private void SyncProjectileVisuals(GameState gameState)
    {
        var currentIds = new HashSet<int>(gameState.projectilesQ.Select(p => p.ProjectileId));

        var idsToRemove = projectileVisuals.Keys.Where(id => !currentIds.Contains(id)).ToList();
        foreach (int id in idsToRemove)
        {
            Destroy(projectileVisuals[id]);
            projectileVisuals.Remove(id);
        }

        foreach (var projectile in gameState.projectilesQ)
        {
            Vector3 worldPos = projectile.Position.ToUnityWorld(1f);

            if (!projectileVisuals.ContainsKey(projectile.ProjectileId))
            {
                GameObject obj = Instantiate(projectilePrefab, worldPos, Quaternion.identity);
                projectileVisuals[projectile.ProjectileId] = obj;
            }
            else
            {
                projectileVisuals[projectile.ProjectileId].transform.position = worldPos;
            }
        }
    }
}
