using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;  // Necesario si usas TextMeshPro para la UI

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class SheepMovementController : MonoBehaviour, IInputProvider
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
    
    [Tooltip("Umbral de velocidad (m/s) por debajo del cual se considera parado")]
    [SerializeField] private float _stopThreshold = 0.1f;

    [Header("Turn & Move")]
    [Tooltip("Ángulo (grados) por debajo del cual ya consideramos que estamos alineados y podemos movernos")]
    [SerializeField] private float _rotationThreshold = 5f;

    [SerializeField]
    [Tooltip("Longitud con la que se dibuja/computa el punto de input en Gizmos")]
    private float _drawLength = 3f;




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

    // Hashes para parámetros de Animator https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Animator.StringToHash.html
    private int _waitParam;
    private int _walkParam;
    private int _runParam;
    private int _xSpeedParam;
    private int _ySpeedParam;
    private int _walkAnimSpeedParam;
    private int _runAnimSpeedParam;

    // Estado interno
    private Vector2 _moveInput = Vector2.zero;
    private bool _wantsSprint = false;

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
        Debug.Log("Enabling input handlers");
        _movementAction.action.performed += OnMovePerformed;
        _movementAction.action.canceled += OnMoveCanceled;
        _movementAction.action.Enable();

        _sprintAction.action.performed += OnSprintPerformed;
        _sprintAction.action.canceled += OnSprintCanceled;
        _sprintAction.action.Enable();
    }


    private void OnDisable()
    {
        Debug.Log("Disabling input handlers");
        _movementAction.action.performed -= OnMovePerformed;
        _movementAction.action.canceled -= OnMoveCanceled;
        _movementAction.action.Disable();

        _sprintAction.action.performed -= OnSprintPerformed;
        _sprintAction.action.canceled -= OnSprintCanceled;
        _sprintAction.action.Disable();
    }



    // ---- Callbacks de InputSystem ----

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log($"OnMovePerformed: {ctx.ReadValue<Vector2>()}");
        _moveInput = ctx.ReadValue<Vector2>();
    }
    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnMoveCanceled");
        _moveInput = Vector2.zero;
    }
    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnSprintPerformed");
        _wantsSprint = true;
    }
    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnSprintCanceled");
        _wantsSprint = false;
    }

    private void Update()
    {
        CalculateInputDirWorld();

        UpdateEnergyAndSprint();
        UpdateAnimator();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        //print(_rb.linearVelocity.magnitude);
        // Sin input, nada
        if (_inputDirWorld.sqrMagnitude < 0.001f)
            return;

        // Obtenemos el StateInfo una sola vez
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // Si estamos dentro de cualquier estado taggeado "Wait", no rotamos ni movemos
        if (stateInfo.IsTag("Waiting"))
            return;

        //HandleMovement();
        // 1) Si estamos parados, rotamos en parado
        if (_rb.linearVelocity.magnitude <= _stopThreshold)
        {
            //HandleMovement();
            float angle = HandleStationaryRotation();
            // Y sólo movemos si ya pasamos el umbral
            if (angle <= _rotationThreshold && IsInMoveState())
            {
                HandleMovement();

            }
        }
        else
        {
            
            HandleMovement();
            //if (IsInMoveState())
            //{
            //    HandleMovement();

            //}
        }
    }

    private void CalculateInputDirWorld()
    {
        // ——————— Igual que tenías en ReadInput() ———————
        Vector3 camF = Camera.main.transform.forward;
        camF.y = 0; 
        camF.Normalize();
        Vector3 camR = Camera.main.transform.right;
        camR.y = 0; 
        camR.Normalize();

        Vector3 raw = camF * _moveInput.y + camR * _moveInput.x;
        float mag = Mathf.Clamp01(_moveInput.magnitude);

        _inputDirWorld = raw.sqrMagnitude > 0.001f
            ? raw.normalized * mag
            : Vector3.zero;
    }



    #region Metodos de movimiento

    // -------------------- MÉTODOS DE MOVIMIENTO --------------------

    /// <summary>
    /// 5) Desplaza el Rigidbody según la dirección y velocidad,
    ///    usando walkSpeed si estamos blendando entre Walk y Run.
    /// </summary>
    private void HandleMovement()
    {
        if (_inputDirWorld.sqrMagnitude <= _inputThreshold)
            return;

        float speed = GetCurrentSpeed();

        Vector3 moveDir = _inputDirWorld.normalized;
        Vector3 groundNormal = GetGroundNormal(out bool hitGround);
        if (hitGround)
            moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;

        Vector3 desiredVelocity = moveDir * speed;

        // Solo afectamos el movimiento horizontal (X y Z)
        Vector3 currentVelocity = _rb.linearVelocity;
        Vector3 velocityChange = new Vector3(
            desiredVelocity.x - currentVelocity.x,
            0f,  // No tocar la componente vertical
            desiredVelocity.z - currentVelocity.z
        );

        Vector3 force = (_rb.mass * velocityChange) / Time.fixedDeltaTime;
        _rb.AddForce(force, ForceMode.Force);


        HandleRotation(moveDir, hitGround ? groundNormal : Vector3.up, _rotationSpeed);
    }



    #endregion

    #region Metodos de Rotación

    // -------------------- MÉTODOS DE ROTACIÓN --------------------

    /// <summary>
    /// Gira la oveja en movimiento hacia _inputDirWorld, inclinándose según la pendiente.
    /// </summary>
    private void HandleRotation(Vector3 forwardDir, Vector3 upDir, float Rotation)
    {
        Quaternion desired = Quaternion.LookRotation(forwardDir, upDir);
        Quaternion smooth = Quaternion.RotateTowards(
            _rb.rotation,
            desired,
            Rotation * Time.fixedDeltaTime
        );
        _rb.MoveRotation(smooth);
    }



    /// <summary>
    /// Gira la oveja en su sitio hacia _inputDirWorld, inclinándose según la pendiente.
    /// Devuelve el ángulo restante para alinearse (en grados).
    /// </summary>
    private float HandleStationaryRotation()
    {
        // Asegúrate de tener una referencia al Rigidbody
        // private Rigidbody _rigidbody;
        // void Awake() { _rigidbody = GetComponent<Rigidbody>(); }
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        float angle = Vector3.Angle(flatForward, _inputDirWorld);
        Vector3 moveDir = _inputDirWorld.normalized;
        Vector3 groundNormal = GetGroundNormal(out bool hitGround);
        if (hitGround)
            moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;

        HandleRotation(moveDir, hitGround ? groundNormal : Vector3.up, _rotationSpeedStopped);

        //if (angle > 0.01f)
        //{
        //    float rotSpd = _rotationSpeedStopped;
        //    Quaternion target = Quaternion.LookRotation(_inputDirWorld, Vector3.up);

        //    // Obtener la rotación interpolada
        //    Quaternion newRotation = Quaternion.RotateTowards(
        //        _rb.rotation, // Usar la rotación del Rigidbody
        //        target,
        //        rotSpd * Time.fixedDeltaTime
        //    );

        //    // Mover el Rigidbody a la nueva rotación
        //    _rb.MoveRotation(newRotation);
        //}
        return angle;
    }




    #endregion

    #region Metodos de INPUT

    // -------------------- MÉTODOS MODULARES --------------------

    /// <summary>
    /// 1) Lee el input de movimiento y sprint,
    ///    y calcula la dirección de movimiento en espacio mundo.
    /// </summary>
    //private void ReadInput()
    //{
    //    _moveInput = _movementAction.action.ReadValue<Vector2>();
    //    _wantsSprint = _sprintAction.action.IsPressed();

    //    Vector3 camF = Camera.main.transform.forward; camF.y = 0; camF.Normalize();
    //    Vector3 camR = Camera.main.transform.right; camR.y = 0; camR.Normalize();
    //    Vector3 raw = camF * _moveInput.y + camR * _moveInput.x;

    //    float mag = Mathf.Clamp01(_moveInput.magnitude);
    //    _inputDirWorld = raw.sqrMagnitude > 0.001f
    //        ? raw.normalized * mag
    //        : Vector3.zero;
    //}

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

    public Vector3 GetInputDirection()
    {
        Vector3 inputDir = _inputDirWorld.normalized * _drawLength;
        
        return transform.position + inputDir;
    }

    public bool GetInput()
    {
        return _inputDirWorld.sqrMagnitude > 0f;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        // Dirección forward del transform en verde
        Gizmos.color = Color.green;
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 forwardDir = flatForward * _drawLength;

        Gizmos.DrawLine(origin, origin + forwardDir);
        Gizmos.DrawSphere(origin + forwardDir, 0.05f);

        // Dirección de input en rojo
        Gizmos.color = Color.red;
        if (_inputDirWorld.sqrMagnitude > 0f)
        {
            Vector3 inputDir = _inputDirWorld.normalized * _drawLength;
            Gizmos.DrawLine(origin, origin + inputDir);
            Gizmos.DrawSphere(origin + inputDir, 0.05f);
        }
    }

    #region Sound

    [Header("AudioSettings")]
    public AudioSource audioSource;
    public AudioClip walk;
    public AudioClip run;

    public void PlayWalk()
    {
        if (audioSource && walk)
        {
            if (audioSource.clip != walk || !audioSource.isPlaying)
            {
                audioSource.clip = walk;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
    public void PlayRun()
    {
        if (audioSource && run)
        {
            audioSource.clip = run;
            audioSource.Play();
        }
    }
    public void Stop()
    {
        if (audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    #endregion
}















