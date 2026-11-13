using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LiteNetLib;
using TMPro;

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

    // Error message on the main menu
    [SerializeField] private TMP_Text errorText;

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

        HideError();
        ShowCanvas(mainMenu);
    }

    private void Start()
    {
        NetworkClient.Instance.Connected += OnConnected;
        NetworkClient.Instance.Disconnected += OnDisconnected;
        NetworkClient.Instance.MatchFound += OnMatchFound;
        NetworkClient.Instance.MatchCreated += OnMatchCreated;
    }

    private void OnDestroy()
    {
        singlePlayerButton.onClick.RemoveListener(OnSinglePlayer);
        multiPlayerButton.onClick.RemoveListener(OnMultiPlayer);
        cancelQueueButton.onClick.RemoveListener(OnCancelQueue);
        declineQueueButton.onClick.RemoveListener(OnDeclineQueue);
        acceptQueueButton.onClick.RemoveListener(OnAcceptQueue);
        exitButton.onClick.RemoveListener(OnExit);

        NetworkClient.Instance.Connected -= OnConnected;
        NetworkClient.Instance.Disconnected -= OnDisconnected;
        NetworkClient.Instance.MatchFound -= OnMatchFound;
        NetworkClient.Instance.MatchCreated -= OnMatchCreated;
    }

    // UI Helpers
    private void ShowCanvas(Canvas target)
    {
        mainMenu.gameObject.SetActive(mainMenu == target);
        Queue.gameObject.SetActive(Queue == target);
        QueuePop.gameObject.SetActive(QueuePop == target);
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        errorText.gameObject.SetActive(false);
    }

    // Button Handlers
    private void OnSinglePlayer()
    {
        HideError();
        requestedMode = GameMode.SinglePlayer;
        NetworkClient.Instance.Connect(requestedMode);

        ShowCanvas(Queue);
    }

    private void OnMultiPlayer()
    {
        HideError();
        requestedMode = GameMode.MultiPlayer;
        NetworkClient.Instance.Connect(requestedMode);

        ShowCanvas(Queue);
    }

    private void OnCancelQueue()
    {
        NetworkClient.Instance.Disconnect();
        ShowCanvas(mainMenu);
    }

    private void OnDeclineQueue()
    {
        // Disconnecting is the decline: the server treats a disconnect during the pop as a decline
        NetworkClient.Instance.Disconnect();
        ShowCanvas(mainMenu);
    }

    private void OnAcceptQueue()
    {
        NetworkClient.Instance.SendAccept();
        acceptQueueButton.interactable = false;
    }

    private void OnExit()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Network Events
    private void OnConnected()
    {
        Debug.Log("Menu: connected, waiting for the server");
    }

    private void OnDisconnected(DisconnectReason reason)
    {
        Debug.Log($"Menu: disconnected, reason {reason}");
        ShowCanvas(mainMenu);

        if (reason == DisconnectReason.ConnectionFailed)
        {
            ShowError("Could not reach the server.");
        }
        else if (reason == DisconnectReason.ConnectionRejected)
        {
            ShowError("The server rejected the connection.");
        }
    }

    private void OnMatchFound()
    {
        acceptQueueButton.interactable = true;
        ShowCanvas(QueuePop);
    }

    private void OnMatchCreated()
    {
        Debug.Log("Menu: match created");
        SceneManager.LoadScene(gameSceneName);
    }
}