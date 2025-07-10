using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mantiene a la oveja flotando a una altura constante sobre el terreno,
/// aplicando un comportamiento de muelle amortiguado y un suave efecto de respiración.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SheepGroundRider : MonoBehaviour
{
    #region Inspector Settings

    [Header("Raycast Configuración")]
    [Tooltip("Dirección del raycast hacia abajo (en espacio local).")]
    [SerializeField] private Vector3 _downDir = Vector3.down;
    [Tooltip("Distancia máxima para detectar el suelo.")]
    [SerializeField] private float _maxRayDist = 100f;
    [Tooltip("Capas consideradas como suelo.")]
    [SerializeField] private LayerMask _rayMask;
    [Tooltip("Margen extra para determinar si está en el suelo.")]
    [SerializeField] private float _isGroundThreshold = 0.6f;

    [Header("Parámetros del Resorte")]
    [Tooltip("Altura objetivo base respecto al suelo.")]
    [SerializeField] private float _baseRideHeight = 2.5f;
    [Tooltip("Constante de fuerza del resorte.")]
    [SerializeField] private float _rideSpringStrength = 300f;
    [Tooltip("Constante de amortiguación del resorte.")]
    [SerializeField] private float _rideSpringDamper = 15f;

    [Header("Efecto Respiración")]
    [Tooltip("Amplitud del movimiento senoidal.")]
    [SerializeField] private float _breathingAmplitude = 0.4f;
    [Tooltip("Frecuencia del movimiento senoidal (ciclos por segundo).")]
    [SerializeField] private float _breathingFrequency = 1f;

    #endregion

    private Rigidbody _rb;
    private Transform _cachedTransform;

    private bool _rayDidHit;
    private RaycastHit _rayHit;
    private float _currentBreathingOffset;

    // Constantes para gizmos
    private const float GizmoBaseRadius = 0.1f;
    private const float GizmoBreathRadius = 0.15f;
    private const float GizmoThresholdRad = 0.05f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cachedTransform = transform;
    }

    private void FixedUpdate()
    {
        PerformRaycast();
        CalculateBreathingOffset();
        ApplySpringBalance();
    }

    /// <summary>
    /// Lanza un raycast desde la posición de la oveja hacia abajo
    /// para detectar la altura del suelo.
    /// </summary>
    private void PerformRaycast()
    {
        Vector3 worldDown = _cachedTransform.TransformDirection(_downDir);
        _rayDidHit = Physics.Raycast(
            _cachedTransform.position,
            worldDown,
            out _rayHit,
            _maxRayDist,
            _rayMask
        );
    }

    /// <summary>
    /// Calcula el desplazamiento vertical de “respiración” usando una función seno.
    /// </summary>
    private void CalculateBreathingOffset()
    {
        _currentBreathingOffset = Mathf.Sin(
            Time.time * _breathingFrequency
        ) * _breathingAmplitude;
    }

    /// <summary>
    /// Aplica la fuerza de resorte amortiguado para mantener la altura deseada.
    /// </summary>
    private void ApplySpringBalance()
    {
        if (!_rayDidHit) return;

        Vector3 worldDown = _cachedTransform.TransformDirection(_downDir);

        // Velocidad relativa entre oveja y suelo
        Vector3 ownVel = _rb.linearVelocity;
        Vector3 otherVel = _rayHit.rigidbody?.linearVelocity ?? Vector3.zero;
        float relVel = Vector3.Dot(worldDown, ownVel - otherVel);

        // Altura objetivo = base + respiración
        float targetHeight = _baseRideHeight + _currentBreathingOffset;
        float heightError = _rayHit.distance - targetHeight;

        // Fuerza del resorte: restauradora y amortiguadora
        float springForce = (heightError * _rideSpringStrength)
                          - (relVel * _rideSpringDamper);

        // Aplicar fuerzas simétricas en oveja y suelo
        _rb.AddForce(worldDown * springForce);
        if (_rayHit.rigidbody != null)
            _rayHit.rigidbody.AddForceAtPosition(
                -worldDown * springForce,
                _rayHit.point
            );
    }

    /// <summary>
    /// Indica si la oveja está lo suficientemente cerca del suelo.
    /// </summary>
    public bool IsGrounded()
    {
        return _rayDidHit &&
               _rayHit.distance <= (_baseRideHeight + _isGroundThreshold);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && _cachedTransform == null)
            _cachedTransform = transform;

        Vector3 pos = _cachedTransform.position;
        Vector3 worldDown = _cachedTransform.TransformDirection(_downDir);

        // Raycast line
        Gizmos.color = _rayDidHit ? Color.blue : Color.red;
        float len = _rayDidHit ? _rayHit.distance : _maxRayDist;
        Gizmos.DrawLine(pos, pos + worldDown * len);

        // Altura base
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos + worldDown * _baseRideHeight, GizmoBaseRadius);

        // Altura con respiración
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            pos + worldDown * (_baseRideHeight + _currentBreathingOffset),
            GizmoBreathRadius
        );

        // Umbral de suelo
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(
            pos + worldDown * (_baseRideHeight + _isGroundThreshold),
            GizmoThresholdRad
        );
    }
}



