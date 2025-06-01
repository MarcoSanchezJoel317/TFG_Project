using UnityEngine;
using UnityEngine.InputSystem;  // Necesitarás el paquete "Input System"
using System.Collections;

public class Cursor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Material mat;
    [SerializeField] private float distanceFromCamera = 10f;

    [Header("Brillo")]
    public Color emissiveColor = new Color(0f, 154f / 255f, 224f / 255f);
    public float minIntensity = 6f;
    public float maxIntensity = 10f;
    public float fadeDuration = 1f;
    public float pulseInterval = 3f;

    [Header("Controles")]
    [SerializeField] private float gamepadSpeed = 5f; // Velocidad con el mando

    private float _currentIntensity;
    private Coroutine _pulseCoroutine;
    private Vector2 _gamepadInput; // Input analógico del mando

    private void Start()
    {
        if (mat == null)
            mat = GetComponent<MeshRenderer>().material;

        _currentIntensity = minIntensity;
        UpdateEmission();
        _pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool isGamepadConnected = Input.GetJoystickNames().Length > 0;
        Vector3 targetPosition = transform.position; // Mantenemos la posición actual por defecto

        // Opción 1: Movimiento con Mando (si está conectado y hay input)
        if (isGamepadConnected && _gamepadInput != Vector2.zero)
        {
            targetPosition += new Vector3(_gamepadInput.x, _gamepadInput.y, 0f) * gamepadSpeed * Time.deltaTime;
        }
        // Opción 2: Movimiento con Ratón (solo si no hay mando conectado)
        else if (!isGamepadConnected)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = distanceFromCamera;
            targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        }

        // Aplicar movimiento
        transform.position = targetPosition;
    }

    // Método llamado por el Input System (asignado en el Inspector)
    public void OnGamepadMove(InputAction.CallbackContext context)
    {
        _gamepadInput = context.ReadValue<Vector2>();
    }

    // --- Resto del código (corrutinas de brillo, etc.) ---
    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(FadeBrightness(minIntensity, maxIntensity, fadeDuration));
            yield return StartCoroutine(FadeBrightness(maxIntensity, minIntensity, fadeDuration));
            yield return new WaitForSeconds(pulseInterval);
        }
    }

    private IEnumerator FadeBrightness(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            _currentIntensity = Mathf.Lerp(start, end, elapsed / duration);
            UpdateEmission();
            elapsed += Time.deltaTime;
            yield return null;
        }
        _currentIntensity = end;
        UpdateEmission();
    }

    private void UpdateEmission()
    {
        mat.SetColor("_EmissiveColor", emissiveColor * _currentIntensity);
#if UNITY_EDITOR
        if (Application.isPlaying)
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
#endif
    }

    private void OnDisable()
    {
        if (_pulseCoroutine != null)
            StopCoroutine(_pulseCoroutine);
    }
}