using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;  // Necesario si usas TextMeshPro para la UI

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class SheepMovementController : MonoBehaviour
{
    [Header("Input 🎮")]
    [Tooltip("Acción de movimiento (Vector2) desde el Input System)")]
    [SerializeField] private InputActionReference _movementAction;
    [Tooltip("Acción de sprint (Button) desde el Input System)")]
    [SerializeField] private InputActionReference _sprintAction;

    [Header("Movement Speeds")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _runSpeed = 10f;

    [Header("Animation Speeds")]
    [Tooltip("Multiplicador de velocidad de la animación de caminar")]
    [SerializeField] private float _walkAnimationSpeed = 1f;
    [Tooltip("Multiplicador de velocidad de la animación de sprint")]
    [SerializeField] private float _runAnimationSpeed = 1.5f;


    [Header("Sprint Energy ⚡")]
    [Tooltip("Cantidad máxima de energía para sprint")]
    [SerializeField] private float _maxEnergy = 5f;
    [Tooltip("Unidades de energía consumidas por segundo al sprintar")]
    [SerializeField] private float _sprintDrainRate = 1f;
    [Tooltip("Unidades de energía regeneradas por segundo cuando no sprinta")]
    [SerializeField] private float _energyRegenRate = 0.5f;

    [Header("UI 🇹")]
    [Tooltip("Texto de la UI (TextMeshPro) para mostrar la energía actual")]
    [SerializeField] private TMP_Text _energyText;

    [Header("Rotation 🔄")]
    [Tooltip("Velocidad de giro suave mientras se mueve (grados/s)")]
    [SerializeField] private float _rotationSpeed = 360f;
    [Tooltip("Velocidad de giro rápido cuando está prácticamente parado")]
    [SerializeField] private float _rotationSpeedStopped = 720f;
    [Tooltip("Ángulo (grados) a partir del cual empieza a frenar antes de girar")]
    [Range(0f, 180f)]
    [SerializeField] private float _brakeAngle = 120f;
    [Tooltip("Umbral de velocidad (m/s) por debajo del cual se considera parado")]
    [SerializeField] private float _stopThreshold = 0.1f;

    [Header("Turn & Move")]
    [Tooltip("Ángulo (grados) por debajo del cual ya consideramos que estamos alineados y podemos movernos")]
    [SerializeField] private float _rotationThreshold = 5f;




    [Header("Slope Alignment 🏔️")]

    [Tooltip("Distancia máxima del raycast hacia abajo")]
    [SerializeField] private float _slopeRayDistance = 2f;
    [SerializeField] private LayerMask _groundMask;




    // Componentes
    private Rigidbody _rb;
    private Animator _animator;

    // Hashes para estados
    private int _walkTreeHash;
    private int _runTreeHash;

    // Hashes para parámetros de Animator
    private int _waitParam;
    private int _walkParam;
    private int _runParam;
    private int _xSpeedParam;
    private int _ySpeedParam;
    private int _walkAnimSpeedParam;
    private int _runAnimSpeedParam;

    // Estado interno
    private Vector2 _moveInput;       // Input 2D
    private bool _wantsSprint;     // Sprint pulsado
    private float _energy;          // Energía actual
    private Vector3 _inputDirWorld;    // Dirección en mundo

    private RaycastHit _hitInfo;


    // Umbral estático
    private static readonly float _inputThreshold = 0.001f;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _energy = _maxEnergy;

        //Animation to hash
        _walkTreeHash = Animator.StringToHash("Walk Tree");
        _runTreeHash = Animator.StringToHash("Run Tree");

        _waitParam = Animator.StringToHash("Wait");
        _walkParam = Animator.StringToHash("Walk");
        _runParam = Animator.StringToHash("Run");
        _xSpeedParam = Animator.StringToHash("XSpeed");
        _ySpeedParam = Animator.StringToHash("YSpeed");
        _walkAnimSpeedParam = Animator.StringToHash("WalkAnimationSpeed");
        _runAnimSpeedParam = Animator.StringToHash("RunAnimationSpeed");

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
        // Sin input, nada
        if (_inputDirWorld.sqrMagnitude < 0.001f)
            return;

        // Obtenemos el StateInfo una sola vez
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // Si estamos dentro de cualquier estado taggeado "Wait", no rotamos ni movemos
        if (stateInfo.IsTag("Waiting"))
            return;

        // 1) Si estamos parados, rotamos en parado
        if (_rb.linearVelocity.magnitude <= _stopThreshold)
        {
            HandleMovement();
            //float angle = HandleStationaryRotation();
            //// Y sólo movemos si ya pasamos el umbral
            //if (angle <= _rotationThreshold && IsInMoveState())
            //{
            //    HandleMovement();
                
            //}
        }
        else
        {
            // 2) Si ya íbamos en marcha, rotamos y movemos a la vez
            //HandleRotation();
            if (IsInMoveState())
            {
                HandleMovement();
                
            }
        }
    }
    #region Metodos de movimiento

    // -------------------- MÉTODOS DE MOVIMIENTO --------------------

    /// <summary>
    /// 5) Desplaza el Rigidbody según la dirección y velocidad,
    ///    usando walkSpeed si estamos blendando entre Walk y Run.
    /// </summary>
    private void HandleMovement()
    {
        // 0) Salimos si no hay input significativo
        if (_inputDirWorld.sqrMagnitude <= _inputThreshold)
            return;

        // 1) Determinamos velocidad según Animator
        float speed = GetCurrentSpeed();

        // 2) Calculamos dirección de movimiento sobre la pendiente
        Vector3 moveDir = _inputDirWorld.normalized;
        Vector3 groundNormal = GetGroundNormal(out bool hitGround);
        if (hitGround)
            moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;

        // 3) Movimiento del Rigidbody
        Vector3 targetPos = _rb.position + moveDir * speed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);

        // 4) Rotación según pendiente
        HandleRotation(moveDir, hitGround ? groundNormal : Vector3.up);
    }



    #endregion

    #region Metodos de Rotación

    // -------------------- MÉTODOS DE ROTACIÓN --------------------

    /// <summary>
    /// Gira la oveja en movimiento hacia _inputDirWorld, inclinándose según la pendiente.
    /// </summary>
    private void HandleRotation(Vector3 forwardDir, Vector3 upDir)
    {
        Quaternion desired = Quaternion.LookRotation(forwardDir, upDir);
        Quaternion smooth = Quaternion.RotateTowards(
            _rb.rotation,
            desired,
            _rotationSpeed * Time.fixedDeltaTime
        );
        _rb.MoveRotation(smooth);
    }



    /// <summary>
    /// Gira la oveja en su sitio hacia _inputDirWorld, inclinándose según la pendiente.
    /// Devuelve el ángulo restante para alinearse (en grados).
    /// </summary>
    private float HandleStationaryRotation()

    {

        // Calcula el ángulo actual

        float angle = Vector3.Angle(transform.forward, _inputDirWorld);

        if (angle > 0.01f)

        {

            // Gira más rápido en parado si quieres:

            float rotSpd = _rotationSpeedStopped;

            Quaternion target = Quaternion.LookRotation(_inputDirWorld, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(

              transform.rotation,

              target,

              rotSpd * Time.fixedDeltaTime

            );

        }

        return angle;

    }




    #endregion

    #region Metodos de INPUT

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

    #endregion

    #region Metodos de SPRINT
    
    // -------------------- MÉTODOS DE SPRINT --------------------

    /// <summary>
    /// 2) Gestiona la energía: se drena al sprintar y se regenera al no sprintar.
    ///     Ahora sólo permite sprintar si _energy >= 1 (condición comentada para futura eliminación).
    /// </summary>
    /// <summary>
    /// Gestiona la energía: se drena solo si estamos en el estado de sprint real
    /// (animación “Run Tree” activa) y además hay movimiento.
    /// </summary>
    private void UpdateEnergyAndSprint()
    {
        // Comprobamos si el Animator está realmente en “Run Tree”
        bool isInRunTree = _animator.GetCurrentAnimatorStateInfo(0).IsName("Run Tree");
        // Y además tenemos input de movimiento
        bool isMoving = _inputDirWorld.sqrMagnitude > 0.001f;

        if (isInRunTree && isMoving && _energy > 0f)
        {
            // Consumimos energía solo mientras ejecutamos la animación de sprint en marcha
            _energy -= _sprintDrainRate * Time.deltaTime;
            if (_energy <= 0f)
            {
                _energy = 0f;
                _wantsSprint = false;  // bloquea intentos de sprint adicionales
            }
        }
        else
        {
            // Regeneramos energía si no estamos corriendo
            _energy = Mathf.Min(_maxEnergy, _energy + _energyRegenRate * Time.deltaTime);
        }
    }

    #endregion

    #region Metodos de actualizacion

    // -------------------- MÉTODOS DE ACTUALZACIÓN --------------------

    /// <summary>
    /// 3) Actualiza los bools y floats del Animator para las animaciones.
    /// </summary>
    private void UpdateAnimator()
    {
        bool isMoving = _inputDirWorld.sqrMagnitude > _inputThreshold;
        bool isRunning = isMoving && _wantsSprint;

        // Usamos SetBool/SetFloat con hashes en vez de strings
        _animator.SetBool(_waitParam, !isMoving);
        _animator.SetBool(_walkParam, isMoving && !isRunning);
        _animator.SetBool(_runParam, isRunning);

        _animator.SetFloat(_xSpeedParam, _moveInput.x, 0.1f, Time.deltaTime);
        _animator.SetFloat(_ySpeedParam, isMoving ? 1f : 0f, 0.1f, Time.deltaTime);

        _animator.SetFloat(_walkAnimSpeedParam, isRunning ? 1f : _walkAnimationSpeed);
        _animator.SetFloat(_runAnimSpeedParam, isRunning ? _runAnimationSpeed : 1f);
    }

    /// <summary>
    /// 4) Actualiza el texto de la UI con la energía actual.
    /// </summary>
    private void UpdateUI()
    {
        // Early-out si no hay texto asignado
        if (_energyText == null) return;

        // Formateo simple y directo
        _energyText.text = $"Energía: {_energy:0.0} / {_maxEnergy}";
    }

    #endregion

    #region Metodos auxiliares

    // -------------------- MÉTODOS AUXILIARES --------------------

    /// <summary>
    /// Comprueba si el Animator está en el estado de caminata o sprint.
    /// </summary>
    private bool IsInMoveState()
    {
        var st = _animator.GetCurrentAnimatorStateInfo(0);
        int hash = st.shortNameHash;
        return hash == _walkTreeHash || hash == _runTreeHash;
    }

    private float GetCurrentSpeed()
    {
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        bool running = !_animator.IsInTransition(0) && state.shortNameHash == _runTreeHash;
        return running ? _runSpeed : _walkSpeed;
    }


    /// <summary>
    /// Obtiene la normal del terreno justo debajo de la oveja.
    /// </summary>
    private Vector3 GetGroundNormal(out bool hitGround)
    {
        Vector3 origin = transform.position + Vector3.up;
        if (Physics.Raycast(origin, Vector3.down, out _hitInfo, _slopeRayDistance, _groundMask))
        {
            hitGround = true;
            return _hitInfo.normal;
        }
        else
        {
            hitGround = false;
            Debug.LogWarning($"Slope ray missed at {origin}", this);
            return Vector3.up;
        }
    }


    #endregion

    #region Metodos de acceso externo

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

    #endregion
}















