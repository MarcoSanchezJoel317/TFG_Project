using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;  // Necesario si usas TextMeshPro para la UI

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class SheepMovementController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Acción de movimiento (Vector2) desde el Input System)")]
    [SerializeField] private InputActionReference _movementAction;
    [Tooltip("Acción de sprint (Button) desde el Input System)")]
    [SerializeField] private InputActionReference _sprintAction;

    [Header("Speeds")]
    [Tooltip("Velocidad de caminata (m/s)")]
    [SerializeField] private float _walkSpeed = 5f;
    [Tooltip("Velocidad de sprint (m/s)")]
    [SerializeField] private float _runSpeed = 10f;

    [Header("Energy")]
    [Tooltip("Cantidad máxima de energía para sprint")]
    [SerializeField] private float _maxEnergy = 5f;
    [Tooltip("Unidades de energía consumidas por segundo al sprintar")]
    [SerializeField] private float _sprintDrainRate = 1f;
    [Tooltip("Unidades de energía regeneradas por segundo cuando no sprinta")]
    [SerializeField] private float _energyRegenRate = 0.5f;

    [Header("UI")]
    [Tooltip("Texto de la UI (TextMeshPro) para mostrar la energía actual")]
    [SerializeField] private TMP_Text _energyText;

    [Header("Rotation")]
    [Tooltip("Velocidad de giro suave mientras se mueve (grados/s)")]
    [SerializeField] private float _rotationSpeed = 360f;
    [Tooltip("Velocidad de giro rápido cuando está prácticamente parado")]
    [SerializeField] private float _rotationSpeedStopped = 720f;
    [Tooltip("Ángulo (grados) a partir del cual empieza a frenar antes de girar")]
    [Range(0f, 180f)]
    [SerializeField] private float _brakeAngle = 120f;
    [Tooltip("Umbral de velocidad (m/s) por debajo del cual se considera parado")]
    [SerializeField] private float _stopThreshold = 0.1f;

    [Header("Slope Alignment")]
    [Tooltip("Altura desde el pivote para el raycast de pendiente")]
    [SerializeField] private float _slopeRayHeight = 1f;
    [Tooltip("Distancia máxima del raycast hacia abajo")]
    [SerializeField] private float _slopeRayDistance = 2f;
    [Tooltip("Velocidad de interpolación de la inclinación al terreno")]
    [SerializeField] private float _slopeAlignSpeed = 5f;

    // Componentes
    private Rigidbody _rb;
    private Animator _animator;

    // Estado interno
    private Vector2 _moveInput;       // Input 2D
    private bool _wantsSprint;     // Sprint pulsado
    private float _energy;          // Energía actual
    private Vector3 _inputDirWorld;    // Dirección en mundo

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _energy = _maxEnergy;
    }

    private void OnEnable()
    {
        _movementAction.action.Enable();
        _sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        _movementAction.action.Disable();
        _sprintAction.action.Disable();
    }

    private void Update()
    {
        ReadInput();
        UpdateEnergyAndSprint();
        UpdateAnimator();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        if (!IsInMoveState()) return;

        HandleMovement();
        HandleRotation();
        HandleSlopeAlignment();
    }

    // -------------------- MÉTODOS MODULARES --------------------

    /// <summary>
    /// 1) Lee el input de movimiento y sprint,
    ///    y calcula la dirección de movimiento en espacio mundo.
    /// </summary>
    private void ReadInput()
    {
        _moveInput = _movementAction.action.ReadValue<Vector2>();
        _wantsSprint = _sprintAction.action.IsPressed();

        Vector3 camF = Camera.main.transform.forward; camF.y = 0; camF.Normalize();
        Vector3 camR = Camera.main.transform.right; camR.y = 0; camR.Normalize();
        Vector3 raw = camF * _moveInput.y + camR * _moveInput.x;

        float mag = Mathf.Clamp01(_moveInput.magnitude);
        _inputDirWorld = raw.sqrMagnitude > 0.001f
            ? raw.normalized * mag
            : Vector3.zero;
    }

    /// <summary>
    /// 2) Gestiona la energía: se drena al sprintar y se regenera al no sprintar.
    ///     Ahora sólo permite sprintar si _energy >= 1 (condición comentada para futura eliminación).
    /// </summary>
    private void UpdateEnergyAndSprint()
    {
        // Sólo sprintar si queda al menos 1 de energía
        // if (_energy < 1f) _wantsSprint = false;  

        if (_wantsSprint && _inputDirWorld.sqrMagnitude > 0.001f && _energy > 0f /*&& _energy >= 1f*/)
        {
            // Consumimos energía mientras sprintamos
            _energy -= _sprintDrainRate * Time.deltaTime;
            if (_energy <= 0f)
            {
                // Si se agota, bloqueamos el sprint y dejamos energía en cero
                _energy = 0f;
                _wantsSprint = false;
            }
        }
        else
        {
            // Regeneramos energía gradualmente
            _energy = Mathf.Min(_maxEnergy, _energy + _energyRegenRate * Time.deltaTime);
        }
    }


    /// <summary>
    /// 3) Actualiza los bools y floats del Animator para las animaciones.
    /// </summary>
    private void UpdateAnimator()
    {
        bool isMoving = _inputDirWorld.sqrMagnitude > 0.001f;
        bool isRunning = isMoving && _wantsSprint;

        _animator.SetBool("Wait", !isMoving);
        _animator.SetBool("Walk", isMoving && !isRunning);
        _animator.SetBool("Run", isRunning);

        _animator.SetFloat("XSpeed", _moveInput.x, 0.1f, Time.deltaTime);
        _animator.SetFloat("YSpeed", isMoving ? 1f : 0f, 0.1f, Time.deltaTime);

        float animSpeed = isRunning ? (_runSpeed / _walkSpeed) : 1f;
        _animator.SetFloat("AnimationSpeed", animSpeed);
    }

    /// <summary>
    /// 4) Actualiza el texto de la UI con la energía actual.
    /// </summary>
    private void UpdateUI()
    {
        if (_energyText != null)
            _energyText.text = $"Energía: {_energy:0.0} / {_maxEnergy}";
    }

    /// <summary>
    /// Comprueba si el Animator está en el estado de caminata o sprint.
    /// </summary>
    private bool IsInMoveState()
    {
        var st = _animator.GetCurrentAnimatorStateInfo(0);
        return st.IsName("Walk Tree") || st.IsName("Run Tree");
    }

    /// <summary>
    /// 5) Desplaza el Rigidbody según la dirección y velocidad.
    /// </summary>
    private void HandleMovement()
    {
        float speed = _animator.GetBool("Run") ? _runSpeed : _walkSpeed;
        if (_inputDirWorld.sqrMagnitude > 0.001f)
        {
            Vector3 target = _rb.position + _inputDirWorld * speed * Time.fixedDeltaTime;
            _rb.MovePosition(target);
        }
    }

    /// <summary>
    /// 6) Gira el modelo suavemente, con "freno antes de girar" si se da la vuelta.
    /// </summary>
    private void HandleRotation()
    {
        Vector3 curFwd = transform.forward;
        float velMag = _rb.linearVelocity.magnitude;
        float angle = Vector3.Angle(curFwd, _inputDirWorld);

        float rotSpd = (velMag <= _stopThreshold && angle > _brakeAngle)
            ? _rotationSpeedStopped
            : _rotationSpeed;

        if (_inputDirWorld.sqrMagnitude > 0.001f)
        {
            Quaternion tgt = Quaternion.LookRotation(_inputDirWorld, Vector3.up);
            _rb.MoveRotation(Quaternion.RotateTowards(
                _rb.rotation, tgt, rotSpd * Time.fixedDeltaTime
            ));
        }
    }

    /// <summary>
    /// 7) Alinea el modelo con la pendiente del terreno.
    /// </summary>
    private void HandleSlopeAlignment()
    {
        Vector3 origin = transform.position + Vector3.up * _slopeRayHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _slopeRayDistance))
        {
            // Calcula la rotación que alinea el "up" con la normal del terreno
            Quaternion align = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, align, _slopeAlignSpeed * Time.fixedDeltaTime
            );
        }
    }

    // -------------------- MÉTODOS de acceso externo --------------------

    /// <summary>
    /// Recarga instantáneamente toda la energía disponible.
    /// </summary>
    public void ReloadEnergy()
    {
        _energy = _maxEnergy;
    }

    /// <summary>
    /// Ajusta la capacidad máxima de energía.
    /// </summary>
    public void MoreEnergy(int energy)
    {
        _maxEnergy = energy;
    }
}















