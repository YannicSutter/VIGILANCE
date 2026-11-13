using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LiteNetLib;

public class MainMenuManager : MonoBehaviour
{
    // UI Canvas
    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas Queue;
    [SerializeField] private Canvas QueuePop;

    // Main Menu Buttons
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button multiPlayerButton;
    [SerializeField] private Button exitButton;

    // Queue Buttons
    [SerializeField] private Button cancelQueueButton;
    [SerializeField] private Button declineQueueButton;
    [SerializeField] private Button acceptQueueButton;


    private string gameSceneName = "Arena";
    private GameMode requestedMode;

    // Unity Methods
    private void Awake()
    {
        singlePlayerButton.onClick.AddListener(OnSinglePlayer);
        multiPlayerButton.onClick.AddListener(OnMultiPlayer);
        cancelQueueButton.onClick.AddListener(OnCancelQueue);
        declineQueueButton.onClick.AddListener(OnDeclineQueue);
        acceptQueueButton.onClick.AddListener(OnAcceptQueue);
        exitButton.onClick.AddListener(OnExit);

        ShowCanvas(mainMenu);
    }

    private void Start()
    {
        NetworkClient.Instance.Connected += OnConnected;
        NetworkClient.Instance.Disconnected += OnDisconnected;
    }

    private void OnConnected()
    {
         Debug.Log("Menu: connected");
    }

    private void OnDisconnected(DisconnectReason reason)
    {
        Debug.Log($"Menu: disconnected, reason {reason}");
    }

    private void OnDestroy()
    {
        singlePlayerButton.onClick.RemoveListener(OnSinglePlayer);
        multiPlayerButton.onClick.RemoveListener(OnMultiPlayer);
        exitButton.onClick.RemoveListener(OnExit);
    }

    private void ShowCanvas(Canvas target)
    {
        mainMenu.gameObject.SetActive(mainMenu == target);
        Queue.gameObject.SetActive(Queue == target);
        QueuePop.gameObject.SetActive(QueuePop == target);
    }

    private void OnSinglePlayer()
    {
        requestedMode = GameMode.SinglePlayer;
        NetworkClient.Instance.Connect(requestedMode);

        ShowCanvas(Queue);
    }

    private void OnMultiPlayer()
    {
        requestedMode = GameMode.MultiPlayer;
        NetworkClient.Instance.Connect(requestedMode);

        ShowCanvas(Queue);
    }

    private void OnCancelQueue()
    {
        //TODO: Cancel Network Connection

        ShowCanvas(mainMenu);
    }

    private void OnDeclineQueue()
    {
        //TODO: Send QueueDecline to server

        ShowCanvas(mainMenu);
    }

    private void OnAcceptQueue()
    {
        //TODO: Send QueueAccept to server

        //TODO: Only switch scenes if the match is created by the server, else go back into the queue
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnExit()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
