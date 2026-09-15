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
    [SerializeField] private LayerMask groundLayerMask;


    // UNITY METHODS
    private void Awake()
    {
        controls = new PlayerInput();
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

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }


    // INPUT METHODS
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            RecordMoveTarget(new Vector2(hit.point.x, hit.point.z));
        }
    }

    private void OnQPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            Vector3 rawDirection = hit.point - transform.position;
            Vector2 direction = new Vector2(rawDirection.x, rawDirection.z).normalized; // explicit, correct axis mapping

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
