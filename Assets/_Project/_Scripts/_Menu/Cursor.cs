/*
    Cursor.cs
    Autor: Álvaro R. Acosta
    Trabajo de Fin de Grado (TFG)
    ---------------------------------------------------------------------------
    Este script implementa un cursor personalizado para menús UI en Unity 6
    utilizando el nuevo Input System. Proporciona:

    1. Soporte de movimiento por ratón y gamepad, con cambio automático
       de modo según el último dispositivo usado.
    2. Restricción (clamping) para que el cursor no salga de los límites
       de la pantalla.
    3. Simulación de clic UI con gamepad, disparando eventos PointerClick
       en los elementos bajo el cursor.
    4. Suavizado (lerp) en el movimiento con gamepad para evitar saltos
       bruscos.

    Uso:
    - Añadir el componente a un GameObject con SpriteRenderer.
    - Asignar el material emissive (que tenga _EmissionColor habilitado).
    - Configurar referencias InputActionReference: Move (Vector2) y Click.
    - Asegurarse de tener un EventSystem con Input System UI Module.
*/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : MonoBehaviour
{
    [Header("Cursor Visual")]
    [Tooltip("Distancia en Z para convertir ScreenToWorldPoint.")]
    [SerializeField] private float distanceFromCamera = 10f;

    [Header("Movement")]
    [Tooltip("Velocidad base del cursor con gamepad.")]
    [SerializeField] private float gamepadSpeed = 5f;
    [Tooltip("Factor de suavizado (0=saltar; 1=inmediato).")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothFactor = 0.2f;

    [Header("Input Actions")]
    [Tooltip("Referencia a la acción Move (Vector2)")]
    public InputActionReference moveAction;
    [Tooltip("Referencia a la acción Click (Button)")]
    public InputActionReference clickAction;

    // Componentes privados
    private Vector2 _gamepadInput;

    // Modo de entrada
    private enum InputMode { None, Mouse, Gamepad }
    private InputMode _lastInputMode = InputMode.None;

    void Awake()
    {
        // Obtenemos componentes y material

        // Suscribimos acciones del nuevo Input System
        moveAction.action.performed += OnGamepadMove;
        moveAction.action.canceled += OnGamepadMove;
        clickAction.action.performed += _ => OnGamepadClick();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        clickAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        clickAction.action.Disable();
    }

    void Update()
    {
        // Movimiento y clamping cada frame
        HandleMovement();
    }

    /// <summary>
    /// Ajusta nueva entrada de stick gamepad.
    /// </summary>
    private void OnGamepadMove(InputAction.CallbackContext ctx)
    {
        _gamepadInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Simula un "click" UI en la posición del cursor.
    /// </summary>
    private void OnGamepadClick()
    {
        print("Me stan llamando");
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = Camera.main.WorldToScreenPoint(transform.position)
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);
        if (results.Count > 0)
        {
            Debug.Log(results[0].gameObject.name);
            ExecuteEvents.Execute(results[0].gameObject,
                                  pointer,
                                  ExecuteEvents.pointerClickHandler);
        }
    }

    /// <summary>
    /// Mueve el cursor con ratón o gamepad, y lo ajusta a pantalla.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 targetPos = transform.position;

        // 1) Ratón
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if (mouseDelta.sqrMagnitude > 0f)
        {
            _lastInputMode = InputMode.Mouse;
            Vector3 mp = Mouse.current.position.ReadValue();
            mp.z = distanceFromCamera;
            targetPos = Camera.main.ScreenToWorldPoint(mp);
        }

        // 2) Gamepad
        if (_gamepadInput.sqrMagnitude > 0f)
        {
            _lastInputMode = InputMode.Gamepad;
            targetPos += new Vector3(_gamepadInput.x, _gamepadInput.y, 0f)
                         * gamepadSpeed * Time.deltaTime;

                       // ¡Aquí warp el cursor virtual!
            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetPos);
                       if (Mouse.current != null)
                Mouse.current.WarpCursorPosition(screenPos);
        }

        // 3) Suavizado
        Vector3 smoothPos = Vector3.Lerp(transform.position,
                                         targetPos,
                                         smoothFactor);

        // 4) Clamping
        Vector3 min = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, distanceFromCamera));
        Vector3 max = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, distanceFromCamera));
        smoothPos.x = Mathf.Clamp(smoothPos.x, min.x, max.x);
        smoothPos.y = Mathf.Clamp(smoothPos.y, min.y, max.y);

        transform.position = smoothPos;
    }

}
