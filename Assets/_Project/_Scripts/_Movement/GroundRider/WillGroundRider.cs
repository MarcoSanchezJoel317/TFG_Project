using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mantiene la esfera de Voluntad flotando sobre el terreno,
/// usando un comportamiento de resorte amortiguado y respiración senoidal.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WillGroundRider : MonoBehaviour
{
    #region Inspector Settings

    [Header("Detección de Suelo")]
    [Tooltip("Dirección local del raycast hacia abajo.")]
    [SerializeField] private Vector3 _downDir = Vector3.down;
    [Tooltip("Distancia máxima para el raycast.")]
    [SerializeField] private float _maxRayDist = 100f;
    [Tooltip("Capas detectadas como suelo.")]
    [SerializeField] private LayerMask _rayMask;
    [Tooltip("Margen extra para considerar que está en el suelo.")]
    [SerializeField] private float _groundThreshold = 0.6f;

    [Header("Parámetros del Resorte")]
    [Tooltip("Altura base deseada sobre el suelo.")]
    [SerializeField] private float _baseHeight = 2.5f;
    [Tooltip("Fuerza de resorte.")]
    [SerializeField] private float _springStrength = 300f;
    [Tooltip("Amortiguador del resorte.")]
    [SerializeField] private float _springDamper = 15f;

    [Header("Efecto de Respiración")]
    [Tooltip("Amplitud de la oscilación vertical.")]
    [SerializeField] private float _breathingAmplitude = 0.4f;
    [Tooltip("Frecuencia de la oscilación (ciclos/segundo).")]
    [SerializeField] private float _breathingFrequency = 1f;

    #endregion

    private Rigidbody _rb;
    private Transform _t;
    private bool _hit;
    private RaycastHit _hitInfo;
    private float _breathingOffset;

    // Gizmo radii
    private const float GizmoBaseRadius = 0.1f;
    private const float GizmoBreathRadius = 0.15f;
    private const float GizmoThreshold = 0.05f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _t = transform;
    }

    private void FixedUpdate()
    {
        CastGroundRay();
        UpdateBreathingOffset();
        ApplySpringForce();
    }

    /// <summary>
    /// Lanza un raycast hacia abajo para medir la distancia al suelo.
    /// </summary>
    private void CastGroundRay()
    {
        Vector3 worldDown = _t.TransformDirection(_downDir);
        _hit = Physics.Raycast(
            _t.position,
            worldDown,
            out _hitInfo,
            _maxRayDist,
            _rayMask
        );
    }

    /// <summary>
    /// Calcula un offset vertical senoidal para simular respiración.
    /// </summary>
    private void UpdateBreathingOffset()
    {
        _breathingOffset = Mathf.Sin(Time.time * _breathingFrequency)
                         * _breathingAmplitude;
    }

    /// <summary>
    /// Aplica un resorte amortiguado para mantener la altura deseada.
    /// </summary>
    private void ApplySpringForce()
    {
        if (!_hit) return;

        Vector3 worldDown = _t.TransformDirection(_downDir);
        var ownVel = _rb.linearVelocity;
        var otherVel = _hitInfo.rigidbody?.linearVelocity ?? Vector3.zero;
        float relVel = Vector3.Dot(worldDown, ownVel - otherVel);

        float target = _baseHeight + _breathingOffset;
        float error = _hitInfo.distance - target;

        float force = error * _springStrength - relVel * _springDamper;
        _rb.AddForce(worldDown * force);

        if (_hitInfo.rigidbody != null)
            _hitInfo.rigidbody.AddForceAtPosition(-worldDown * force, _hitInfo.point);
    }

    /// <summary>
    /// Comprueba si la esfera está cerca del suelo.
    /// </summary>
    public bool IsGrounded()
    {
        return _hit && _hitInfo.distance <= _baseHeight + _groundThreshold;
    }

    private void OnDrawGizmos()
    {
        if (_t == null) _t = transform;

        Vector3 down = _t.TransformDirection(_downDir);
        Vector3 pos = _t.position;
        bool didHit = Physics.Raycast(pos, down, out RaycastHit info, _maxRayDist, _rayMask);

        // Raycast line
        Gizmos.color = didHit ? Color.blue : Color.red;
        Gizmos.DrawLine(pos, pos + down * (didHit ? info.distance : _maxRayDist));

        // Base height
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos + down * _baseHeight, GizmoBaseRadius);

        // Breathing height
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(pos + down * (_baseHeight + _breathingOffset), GizmoBreathRadius);

        // Ground threshold
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(pos + down * (_baseHeight + _groundThreshold), GizmoThreshold);
    }
}

