using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el New Input System
using static UnityEngine.InputSystem.InputAction; // Para usar CallbackContext directamente

public class CameraController_v0_1_0 : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("Asigna la Input Action Reference para la rotación de la cámara (Vector2, ej: mouse delta o right stick).")]
    [SerializeField] private InputActionReference _lookInputAction; // Acción de input para mirar/rotar cámara.

    [Header("Parámetros de Rotación")]
    [Tooltip("Velocidad de rotación de la cámara (sensibilidad).")]
    [SerializeField] private float _lookSensitivity = 0.1f; // Sensibilidad del mouse/stick.

    [Tooltip("Objeto al que la cámara 'sigue' o alrededor del cual orbita (ej: la oveja).")]
    [SerializeField] private Transform _followTarget; // El target a seguir/orbitar.

    [Tooltip("Distancia a la que la cámara intenta mantenerse del follow target.")]
    [SerializeField] private float _distanceFromTarget = 5.0f; // Distancia de la cámara al target.

    [Tooltip("Offset vertical de la cámara respecto al target.")]
    [SerializeField] private float _heightOffset = 1.0f; // Altura de la cámara sobre el target.

    [Tooltip("Límite de rotación vertical hacia arriba (grados).")]
    [SerializeField] private float _verticalLookLimitUp = 80f; // Ángulo máximo hacia arriba.

    [Tooltip("Límite de rotación vertical hacia abajo (grados).")]
    [SerializeField] private float _verticalLookLimitDown = -60f; // Ángulo máximo hacia abajo.


    private Vector2 _lookInput; // Input de rotación leído.
    private float _currentYaw = 0f; // Rotación horizontal actual (grados).
    private float _currentPitch = 0f; // Rotación vertical actual (grados).


    void Awake()
    {
        // Opcional: Ocultar y bloquear el cursor del mouse al inicio.
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        // Si no se asigna un target, intentar usar el padre del objeto cámara si existe.
        if (_followTarget == null && transform.parent != null)
        {
            _followTarget = transform.parent;
        }
    }

    private void OnEnable()
    {
        // Habilitar la Input Action de mirar.
        if (_lookInputAction != null && _lookInputAction.action != null)
        {
            _lookInputAction.action.Enable();
        }
        else
        {
            Debug.LogError("Look Input Action Reference no está asignada o no es válida en OnEnable.", this);
        }
    }

    private void OnDisable()
    {
        // Deshabilitar la Input Action de mirar.
        if (_lookInputAction != null && _lookInputAction.action != null)
        {
            _lookInputAction.action.Disable();
        }
    }

    private void OnDestroy()
    {
        // Deshabilitar la Input Action al destruir.
        if (_lookInputAction != null && _lookInputAction.action != null)
        {
            _lookInputAction.action.Disable();
        }
    }

    void Update()
    {
        // Leer el input de rotación (mouse delta o right stick).
        if (_lookInputAction != null && _lookInputAction.action != null)
        {
            _lookInput = _lookInputAction.action.ReadValue<Vector2>();
        }
    }

    void LateUpdate() // Usamos LateUpdate para que la cámara siga DESPUÉS de que el personaje se ha movido en FixedUpdate.
    {
        // Asegurarse de tener un target.
        if (_followTarget == null)
        {
            Debug.LogWarning("Camera Follow Target no asignado.", this);
            return;
        }

        // --- Lógica de Rotación de la Cámara ---

        // Acumular el input de rotación, escalado por sensibilidad.
        // _lookInput.x afecta el Yaw (rotación horizontal alrededor del eje Y global).
        // _lookInput.y afecta el Pitch (rotación vertical alrededor del eje X local).
        _currentYaw += _lookInput.x * _lookSensitivity;
        _currentPitch -= _lookInput.y * _lookSensitivity; // Restamos porque mover mouse/stick hacia arriba suele significar mirar hacia ARRIBA (Pitch negativo si el eje X local apunta a la derecha).

        // Limitar la rotación vertical (Pitch).
        _currentPitch = Mathf.Clamp(_currentPitch, _verticalLookLimitDown, _verticalLookLimitUp);

        // Calcular la rotación final deseada (Yaw primero, luego Pitch).
        Quaternion yawRotation = Quaternion.Euler(0f, _currentYaw, 0f); // Solo rotación horizontal.
        Quaternion pitchRotation = Quaternion.Euler(_currentPitch, 0f, 0f); // Solo rotación vertical (local).

        // La rotación final es la combinación (Pitch * Yaw).
        // Nota: El orden importa. Pitch local (rotar alrededor del eje X local)
        // después de Yaw global (rotar alrededor del eje Y global) es común para cámaras de 3ª persona.
        Quaternion targetRotation = yawRotation * pitchRotation;


        // --- Lógica de Posicionamiento de la Cámara ---

        // Calcular la posición deseada de la cámara: Target + Offset Vertical + Rotación * Distancia hacia atrás.
        Vector3 targetPosition = _followTarget.position + Vector3.up * _heightOffset; // Posición base del target con offset de altura.

        // La posición final de la cámara está detrás del target base, rotada por la rotación calculada.
        // Multiplicamos la rotación por Vector3.forward (o Vector3.back, dependiendo de cómo definas "adelante" para la cámara)
        // y luego escalamos por la distancia.
        // Si la rotación calculada apunta hacia DÓNDE mira la cámara, entonces la posición está detrás de eso.
        // Vector3 positionOffset = targetRotation * Vector3.forward * _distanceFromTarget; // Si la cámara mira "adelante" en su propio espacio.
        // O mejor: la posición es el target base MENOS la dirección "hacia atrás" (la dirección forward rotada) * distancia.
        Vector3 positionOffset = targetRotation * Vector3.back * _distanceFromTarget; // La cámara está detrás del target.


        // Establecer la posición y rotación de la cámara.
        transform.position = targetPosition + positionOffset;
        transform.rotation = targetRotation;


        // Opcional: Asegurarse de que la cámara siempre "mire" al target, aunque la rotación ya debería hacerlo.
        // transform.LookAt(_followTarget.position + Vector3.up * _heightOffset);


        // --- Comentarios ---
        // Este script proporciona control básico de una cámara de órbita/free look.
        // Necesita una Input Action "Look" (Vector2).
        // Necesita que se le asigne el Transform del objeto a seguir (_followTarget).
        // Puedes ponerlo en una cámara principal o en un GameObject padre que controle la cámara.
        // Cinemachine Free Look ya hace algo similar de forma más avanzada.

    }

    // --- Logica de Salto (Ejemplo de cómo se engancharía) ---
    // public void Jump(CallbackContext callbackContext) // Esta función se vincularía en el Input Action Asset de la oveja.
    // {
    //     if (callbackContext.performed)
    //     {
    //         // Llama a una función pública o accede al script de movimiento de la oveja
    //         // para indicarle que realice la acción de salto.
    //         // playerMovementScript.PerformJump();
    //     }
    // }
}
