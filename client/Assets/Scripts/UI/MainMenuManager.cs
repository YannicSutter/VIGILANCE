using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // UI Canvas
    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas multiPlayerQueue;
    [SerializeField] private Canvas multiPlayerQueuePop;

    // Main Menu Buttons
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button multiPlayerButton;
    [SerializeField] private Button exitButton;

    // Queue Buttons
    [SerializeField] private Button cancelQueueButton;
    [SerializeField] private Button declineQueueButton;
    [SerializeField] private Button acceptQueueButton;

    // Scenes
    private string gameSceneName = "Arena";


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

    private void OnDestroy()
    {
        singlePlayerButton.onClick.RemoveListener(OnSinglePlayer);
        multiPlayerButton.onClick.RemoveListener(OnMultiPlayer);
        exitButton.onClick.RemoveListener(OnExit);
    }

    private void ShowCanvas(Canvas target)
    {
        mainMenu.gameObject.SetActive(mainMenu == target);
        multiPlayerQueue.gameObject.SetActive(multiPlayerQueue == target);
        multiPlayerQueuePop.gameObject.SetActive(multiPlayerQueuePop == target);
    }

    private void OnSinglePlayer()
    {
        //TODO; Create Network Connection for Singleplayer

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnMultiPlayer()
    {
        // TODO: Create network connection for Multiplayer

        ShowCanvas(multiPlayerQueue);
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
