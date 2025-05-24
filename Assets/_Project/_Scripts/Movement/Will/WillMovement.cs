using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class WillMovement : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _movementInputAction;
    [SerializeField] private InputActionReference _sprintInputAction;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float _minRadius = 2f;
    [SerializeField] private float _maxRadius = 6f;
    [SerializeField] private float _sprintRadiusBonus = 1f;

    private Rigidbody _rb;
    private Transform _sheepTransform;
    private Transform _cameraTransform;
    private Vector2 _input;
    private bool _isSprinting;

    public bool IsSprinting => _isSprinting;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        var sheepGo = GameObject.FindGameObjectWithTag("Player");
        if (sheepGo != null)
            _sheepTransform = sheepGo.transform;
        else
            Debug.LogError("WillMovement: Sheep (tag 'Player') not found.", this);

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
        else
            Debug.LogWarning("WillMovement: MainCamera not found in scene.", this);
    }

    private void OnEnable()
    {
        _movementInputAction.action.Enable();
        _movementInputAction.action.performed += OnMovePerformed;
        _movementInputAction.action.canceled += OnMoveCanceled;

        _sprintInputAction.action.Enable();
        _sprintInputAction.action.performed += OnSprintPerformed;
        _sprintInputAction.action.canceled += OnSprintCanceled;
    }

    private void OnDisable()
    {
        _movementInputAction.action.performed -= OnMovePerformed;
        _movementInputAction.action.canceled -= OnMoveCanceled;
        _movementInputAction.action.Disable();

        _sprintInputAction.action.performed -= OnSprintPerformed;
        _sprintInputAction.action.canceled -= OnSprintCanceled;
        _sprintInputAction.action.Disable();
    }

    private void FixedUpdate()
    {
        if (_sheepTransform == null) return;

        // Build camera-relative move direction in XZ plane
        Vector3 forward = (_cameraTransform ? _cameraTransform.forward : Vector3.forward);
        Vector3 right = (_cameraTransform ? _cameraTransform.right : Vector3.right);
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 moveDir = forward * _input.y + right * _input.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Adjust speed for sprint
        float speed = _moveSpeed * (_isSprinting ? _sprintSpeedMultiplier : 1f);
        Vector3 desiredPos = transform.position + moveDir * speed * Time.fixedDeltaTime;

        // Clamp to radial limits around the sheep
        float maxRadius = _maxRadius + (_isSprinting ? _sprintRadiusBonus : 0f);
        float distToSheep = Vector3.Distance(desiredPos, _sheepTransform.position);

        if (distToSheep > maxRadius)
        {
            Vector3 dir = (desiredPos - _sheepTransform.position).normalized;
            desiredPos = _sheepTransform.position + dir * maxRadius;
        }
        else if (distToSheep < _minRadius)
        {
            Vector3 dir = (desiredPos - _sheepTransform.position).normalized;
            desiredPos = _sheepTransform.position + dir * _minRadius;
        }

        _rb.MovePosition(desiredPos);
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _input = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _input = Vector2.zero;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        _isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        _isSprinting = false;
    }
}




