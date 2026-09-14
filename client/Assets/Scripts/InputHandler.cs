using UnityEngine;
using UnityEngine.InputSystem;


public class InputHandler : MonoBehaviour
{
    // Variables
    private PlayerInput controls;

    private Vector2 targetPosition;
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
            Vector3 direction = (hit.point - transform.position);
            direction.y = 0f;
            direction.Normalize();

            RecordShooting(direction, true);
        }
    }

    private void RecordMoveTarget(Vector2 targetPosition)
    {
        this.targetPosition = targetPosition;
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
            targetPosition = this.targetPosition,
            lookingDirection = this.lookingDirection,
            isShooting = this.isShooting
        };

        isShooting = false;

        return frame;
    }

}
