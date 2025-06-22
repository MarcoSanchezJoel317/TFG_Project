/*
    Cursor.cs
    Autor: Álvaro R. Acosta
    Trabajo de Fin de Grado (TFG)
    ---------------------------------------------------------------------------
    Controla un cursor UI personalizado en Unity usando el nuevo Input System.
    --------------------------------------------------------------------------------
    Características principales:
    1. Movimiento por ratón y gamepad, con detección automática de modo.
    2. Clamping para mantener el cursor dentro de la pantalla.
    3. Simulación de clics UI mediante eventos PointerClick.
    4. Suavizado (lerp) del movimiento de gamepad para evitar saltos bruscos.
*/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Controla la posición y la interacción del cursor en menús UI.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : MonoBehaviour
{
    [Header("Configuración Visual")]
    [Tooltip("Profundidad Z para convertir ScreenToWorldPoint.")]
    [SerializeField] private float _distanceFromCamera = 10f;

    [Header("Movimiento con Gamepad")]
    [Tooltip("Velocidad de movimiento cuando se usa gamepad.")]
    [SerializeField] private float _gamepadSpeed = 5f;
    [Tooltip("Factor de suavizado (0 = sin suavizado; 1 = sin interpolación).")]
    [Range(0f, 1f)]
    [SerializeField] private float _smoothFactor = 0.2f;

    [Header("Referencias Input System")]
    [Tooltip("Acción de entrada para movimiento (Vector2).")]
    public InputActionReference moveAction;
    [Tooltip("Acción de entrada para 'click' (Button).")]
    public InputActionReference clickAction;

    // Estados posibles de entrada
    private enum InputMode { None, Mouse, Gamepad }
    private InputMode _lastInputMode = InputMode.None;

    // Lectura de valores del stick
    private Vector2 _gamepadInput;

    // Cacheo de referencias frecuentes
    private Camera _cam;
    private EventSystem _eventSystem;

    #region Ciclo de Vida

    void Awake()
    {
        // Cacheamos la cámara principal y el EventSystem para acelerar accesos frecuentes
        _cam = Camera.main;
        _eventSystem = EventSystem.current;

        // Suscribimos métodos a los callbacks del Input System, con null-check
        if (moveAction?.action != null)
        {
            moveAction.action.performed += OnGamepadMove;
            moveAction.action.canceled += OnGamepadMove;
        }
        if (clickAction?.action != null)
            clickAction.action.performed += _ => OnGamepadClick();
    }

    void OnEnable()
    {
        // Activamos las acciones para recibir eventos
        moveAction?.action.Enable();
        clickAction?.action.Enable();
    }

    void OnDisable()
    {
        // Desactivamos las acciones al deshabilitar el objeto
        moveAction?.action.Disable();
        clickAction?.action.Disable();
    }

    void OnDestroy()
    {
        // Nos desuscribimos para evitar memory leaks
        if (moveAction?.action != null)
        {
            moveAction.action.performed -= OnGamepadMove;
            moveAction.action.canceled -= OnGamepadMove;
        }
        if (clickAction?.action != null)
            clickAction.action.performed -= _ => OnGamepadClick();
    }

    void Update()
    {
        // 1. Determinamos la posición objetivo según dispositivo
        Vector3 target = transform.position;
        // Si hubo movimiento de ratón, lo procesamos; si no, procesamos gamepad
        if (!TryHandleMouse(ref target))
            TryHandleGamepad(ref target);

        // 2. Suavizamos el movimiento para gamepad (ratón no usa lerp)
        Vector3 smoothed = ApplySmoothing(target);

        // 3. Clampeamos la posición para que no salga de la pantalla
        transform.position = ApplyClamping(smoothed);
    }

    #endregion

    #region Callbacks Input System

    /// <summary>
    /// Callback del stick de gamepad: simplemente guardamos el vector 2.
    /// </summary>
    /// <param name="ctx">Contexto que contiene el Vector2 del stick.</param>
    private void OnGamepadMove(InputAction.CallbackContext ctx)
    {
        _gamepadInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Simula un clic UI en la posición actual del cursor.
    /// </summary>
    private void OnGamepadClick()
    {
        // Log para depuración centralizada
        CustomLogger.Log(this,$"[Cursor] Clic de gamepad en modo {_lastInputMode}");

        // Creamos un PointerEventData con la posición de pantalla del cursor
        var pointer = new PointerEventData(_eventSystem)
        {
            position = _cam.WorldToScreenPoint(transform.position)
        };

        // Raycast contra todos los elementos UI
        var results = new List<RaycastResult>();
        _eventSystem.RaycastAll(pointer, results);

        if (results.Count > 0)
        {
            // Ejecutamos el evento pointerClickHandler en el primer elemento hit
            var hitGO = results[0].gameObject;
            CustomLogger.Log(this, $"[Cursor] Elemento UI clicado: {hitGO.name}");
            ExecuteEvents.Execute(hitGO, pointer, ExecuteEvents.pointerClickHandler);
        }
    }

    #endregion

    #region Movimiento y Clamping

    /// <summary>
    /// Maneja movimiento por ratón.  
    /// </summary>
    /// <param name="target">Referencia a la posición objetivo a modificar.</param>
    /// <returns>True si se detectó y aplicó movimiento de ratón.</returns>
    private bool TryHandleMouse(ref Vector3 target)
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        // Si no hay movimiento de ratón, devolvemos false
        if (delta.sqrMagnitude <= 0f)
            return false;

        // Actualizamos modo e interpretamos posición absoluta del ratón
        _lastInputMode = InputMode.Mouse;
        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = _distanceFromCamera;  // Profundidad para ScreenToWorldPoint
        target = _cam.ScreenToWorldPoint(screenPos);
        return true;
    }

    /// <summary>
    /// Maneja movimiento por gamepad, con warping del cursor hardware.  
    /// </summary>
    /// <param name="target">Referencia a la posición objetivo a modificar.</param>
    /// <returns>True si se detectó y aplicó movimiento de gamepad.</returns>
    private bool TryHandleGamepad(ref Vector3 target)
    {
        if (_gamepadInput.sqrMagnitude <= 0f)
            return false;

        _lastInputMode = InputMode.Gamepad;
        // Desplazamos target según stick y velocidad
        target += new Vector3(_gamepadInput.x, _gamepadInput.y, 0f)
                  * _gamepadSpeed * Time.deltaTime;

        // Opcional: mover físicamente el cursor del sistema para coherencia
        if (Mouse.current != null)
        {
            Vector2 screenPos = _cam.WorldToScreenPoint(target);
            Mouse.current.WarpCursorPosition(screenPos);
        }

        return true;
    }

    /// <summary>
    /// Interpola linealmente la posición actual hacia la posición objetivo.  
    /// </summary>
    private Vector3 ApplySmoothing(Vector3 target)
    {
        // El ratón no se ve afectado (delta fuerte), pero un valor pequeño de _smoothFactor
        // suaviza desplazamientos de gamepad.
        return Vector3.Lerp(transform.position, target, _smoothFactor);
    }

    /// <summary>
    /// Restringe la posición dentro de los límites de la pantalla.  
    /// </summary>
    private Vector3 ApplyClamping(Vector3 pos)
    {
        // Convertimos esquinas de pantalla a coordenadas de mundo
        Vector3 min = _cam.ScreenToWorldPoint(new Vector3(0, 0, _distanceFromCamera));
        Vector3 max = _cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _distanceFromCamera));

        // Clampeo en X e Y para no salirnos del rectángulo de la cámara
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        return pos;
    }

    #endregion
}

