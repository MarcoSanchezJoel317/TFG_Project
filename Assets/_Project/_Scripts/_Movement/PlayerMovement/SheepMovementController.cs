using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class SheepMovementController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference movementAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("Movement Speeds")]
    [Tooltip("Velocidad de paseo")]
    [SerializeField] private float walkSpeed = 5f;
    [Tooltip("Velocidad de sprint")]
    [SerializeField] private float runSpeed = 10f;

    [Header("Animation")]
    [Tooltip("Multiplicador de velocidad de animación")]
    [SerializeField] private float animSpeedMultiplier = 1f;  // Base

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float rotationSpeedStopped = 720f;
    [Range(0f, 180f)]
    [SerializeField] private float brakeAngle = 120f;
    [SerializeField] private float stopThreshold = 0.1f;

    private Rigidbody _rb;
    private Animator _animator;
    private Vector2 _moveInput;
    private bool _wantsSprint;
    private Vector3 _inputDirWorld;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        movementAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        movementAction.action.Disable();
        sprintAction.action.Disable();
    }

    private void Update()
    {
        // 1) Leer inputs
        _moveInput = movementAction.action.ReadValue<Vector2>();
        _wantsSprint = sprintAction.action.IsPressed();

        // 2) Calculamos la magnitud efectiva del input (0..1)
        float inputMag = Mathf.Clamp01(_moveInput.magnitude);

        // 3) Decidir estados de animación
        bool isWalking = inputMag > 0.01f;
        bool isRunning = isWalking && _wantsSprint;

        _animator.SetBool("Wait", !isWalking);
        _animator.SetBool("Walk", isWalking && !isRunning);
        _animator.SetBool("Run", isRunning);

        // 4) Alimentar blend tree direccional SIN valores negativos
        //    XSpeed indica lateralidad (-1 izquierda, +1 derecha)
        //    YSpeed indica avance absoluto (0 atrás .. +1 adelante)
        _animator.SetFloat("XSpeed", _moveInput.x, 0.1f, Time.deltaTime);

        // YSpeed = 1 si hay movimiento en cualquier dirección, 0 si está en idle
        float ySpeedParam = (_moveInput.sqrMagnitude > 0.01f) ? 1f : 0f;
        _animator.SetFloat("YSpeed", ySpeedParam, 0.1f, Time.deltaTime);

        // 5) Control de velocidad de animación
        //    Puedes aumentar con animSpeedMultiplier y diferenciar walk/run si quieres
        _animator.SetFloat("AnimationSpeed", inputMag * animSpeedMultiplier);

        // 6) Dirección en mundo según cámara (normalizada)
        Vector3 camF = Camera.main.transform.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = Camera.main.transform.right; camR.y = 0; camR.Normalize();

        // *Usamos la normalización con inputMag para que diagonal no sobrepase*
        Vector3 rawDir = camF * _moveInput.y + camR * _moveInput.x;
        if (rawDir.sqrMagnitude > 0.001f)
        {
            _inputDirWorld = rawDir.normalized * inputMag;
        }
        else
        {
            _inputDirWorld = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        // Solo si estamos realmente en el blend tree de movimiento
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        bool inMoveSt = state.IsName("Walk Tree") || state.IsName("Run Tree");
        if (!inMoveSt)
            return;

        // 7) Elegir velocidad física (walk/run)
        float speed = _animator.GetBool("Run") ? runSpeed : walkSpeed;

        // 8) Movimiento proporcional al input
        if (_inputDirWorld.sqrMagnitude > 0.001f)
        {
            Vector3 targetPos = _rb.position + _inputDirWorld * speed * Time.fixedDeltaTime;
            _rb.MovePosition(targetPos);
        }

        // 9) Rotación “freno antes de girar” y suavizada
        Vector3 currentFwd = transform.forward;
        float velMag = _rb.linearVelocity.magnitude;
        float angleToDir = Vector3.Angle(currentFwd, _inputDirWorld);

        float rotSpeed = (velMag <= stopThreshold && angleToDir > brakeAngle)
            ? rotationSpeedStopped
            : rotationSpeed;

        if (_inputDirWorld.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(_inputDirWorld, Vector3.up);
            _rb.MoveRotation(Quaternion.RotateTowards(
                _rb.rotation,
                targetRot,
                rotSpeed * Time.fixedDeltaTime
            ));
        }
    }
}











