using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    // Variables
    private PlayerInput controls;

    private Vector2 targetPosition;
    private bool hasMoveTarget;
    private Vector2 lookingDirection;
    private bool isShooting;
    private GameManager gameManager;

    [SerializeField] private GameObject moveIndicatorPrefab;
    [SerializeField] private LayerMask groundLayerMask;


    // UNITY METHODS
    private void Awake()
    {
        controls = new PlayerInput();
        gameManager = GetComponent<GameManager>();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Q.performed += OnQPerformed;
    }

    void OnDisable()
    {
        controls.Player.Move.performed -= OnMovePerformed;
        controls.Player.Q.performed -= OnQPerformed;
        controls.Disable();
    }


    // INPUT METHODS
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            RecordMoveTarget(new Vector2(hit.point.x, hit.point.z));
            GameObject moveIndicator = Instantiate(moveIndicatorPrefab, hit.point, Quaternion.identity);
            Destroy(moveIndicator, 0.2f);
        }
    }

    private void OnQPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            Vector2 clickPos = new Vector2(hit.point.x, hit.point.z);
            Vector2 playerPos = new Vector2(gameManager.Player1.transform.position.x, gameManager.Player1.transform.position.z);

            Vector2 direction = (clickPos - playerPos).normalized;
            RecordShooting(direction, true);
        }
    }

    private void RecordMoveTarget(Vector2 targetPosition)
    {
        this.targetPosition = targetPosition;
        this.hasMoveTarget = true; // NEW
    }

    private void RecordShooting(Vector2 lookingDirection, bool isShooting)
    {
        this.lookingDirection = lookingDirection;
        this.isShooting = isShooting;
    }

    public InputFrame ConsumeInput()
    {
        InputFrame frame = new InputFrame
        {
            // TODO: Delete PlayerId when server connects to client and assigns it automatically
            PlayerId = 0,
            targetPosition = Helper.ToNumerics(this.targetPosition),
            hasMoveTarget = this.hasMoveTarget,
            lookingDirection = Helper.ToNumerics(this.lookingDirection),
            isShooting = this.isShooting
        };

        isShooting = false;

        return frame;
    }
}
