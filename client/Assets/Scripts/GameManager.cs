using UnityEngine;

public class GameManager : MonoBehaviour
{
    // SERIALIZED FIELDS
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private GameObject spawnPosition1;
    [SerializeField] private GameObject spawnPosition2;
    [SerializeField] private InputHandler inputHandler;

    // VARIABLES
    private enum Modes
    {
        SinglePlayer,
        MultiPlayer
    }
    private Modes PlayMode;

    private GameState gameState = new GameState();


    // UNITY METHODS
    private void Start()
    {
        // DEBUG TESTING
        PlayMode = Modes.SinglePlayer;

        SpawnPlayers(playerPrefab, spawnPosition1.transform.position, spawnPosition2.transform.position, PlayMode);
    }

    private void FixedUpdate()
    {
        InputFrame input = inputHandler.ConsumeInput();
        gameState = SendInputToServer(input);
    }


    // GENERAL METHODS
    private void SpawnPlayers(GameObject playerPrefab, Vector3 spawnPosition1, Vector3 spawnPosition2, Modes mode)
    {
        switch (mode)
        {
            case Modes.SinglePlayer:
                SinglePlayerSpawnPlayers(playerPrefab, spawnPosition1, spawnPosition2);
                break;
            case Modes.MultiPlayer:
                MultiPlayerSpawnPlayers(playerPrefab, spawnPosition1, spawnPosition2);
                break;
            default:
                Debug.LogError("Invalid game mode selected.");
                break;
        }
    }

    // SINGLE PLAYER METHODS
    private void SinglePlayerSpawnPlayers(GameObject playerPrefab, Vector3 spawnPosition1, Vector3 spawnPosition2)
    {
        Instantiate(playerPrefab, spawnPosition1, Quaternion.identity);
        Instantiate(npcPrefab, spawnPosition2, Quaternion.identity);
    }

    // MULTI PLAYER METHODS
    private void MultiPlayerSpawnPlayers(GameObject playerPrefab, Vector3 spawnPosition1, Vector3 spawnPosition2)
    {
        throw new System.NotImplementedException();
    }


    // SERVER CONNECTION
    private GameState SendInputToServer(InputFrame input)
    {
        return Server.Instance.Tick(input, Time.fixedDeltaTime);
    }

    System.Numerics.Vector3 ConvertToSystemVector(UnityEngine.Vector3 v)
    => new System.Numerics.Vector3(v.x, v.y, v.z);
}
