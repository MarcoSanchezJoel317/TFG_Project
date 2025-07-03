using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SheepMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _willTransform;
    [SerializeField] private WillMovement _willMovement;

    [Header("Chase Settings")]
    [SerializeField] private float _minRadius = 2f;
    [SerializeField] private float _maxRadius = 6f;
    [SerializeField] private float _baseSpeedFactor = 1.0f;
    [SerializeField] private float _sprintSpeedFactor = 1.5f;

    [Header("Acceleration")]
    [SerializeField, Range(0f, 50f)] private float _acceleration = 8f;
    [SerializeField] private AnimationCurve _accelerationCurve = AnimationCurve.EaseInOut(-1, 0.6f, 1, 1f);
    [SerializeField] private float _sprintAccelMultiplier = 1.5f;

    [Header("Force Limit")]
    [SerializeField, Range(0f, 200f)] private float _maxForce = 80f;
    [SerializeField] private AnimationCurve _forceCurve = AnimationCurve.EaseInOut(-1, 0.6f, 1, 1f);
    [SerializeField] private float _sprintForceMultiplier = 1.5f;
    [SerializeField] private Vector3 _forceScale = new Vector3(1, 0, 1);

    private Rigidbody _rb;
    private Vector3 _velGoal;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_willTransform == null)
            Debug.LogError("SheepMovement: Assign the Will Transform.", this);
        if (_willMovement == null)
            Debug.LogError("SheepMovement: Assign the WillMovement reference.", this);
    }

    private void FixedUpdate()
    {
        if (_willTransform == null) return;

        // Compute direction & distance to Will (XZ plane)
        Vector3 offset = _willTransform.position - transform.position;
        Vector3 dir = new Vector3(offset.x, 0f, offset.z);
        float dist = dir.magnitude;
        if (dist < _minRadius) return;  // Dead-zone

        dir.Normalize();
        float t = Mathf.InverseLerp(_minRadius, _maxRadius, dist);

        // Choose sprint or normal factors
        bool sprint = _willMovement != null && _willMovement.IsSprinting;
        float speedFactor = sprint ? _sprintSpeedFactor : _baseSpeedFactor;
        float accelMul = sprint ? _sprintAccelMultiplier : 1f;
        float forceMul = sprint ? _sprintForceMultiplier : 1f;

        // Desired velocity vector
        Vector3 desiredVelocity = dir * (_acceleration * t * speedFactor);

        // Compute acceleration toward desired vel
        float dot = Vector3.Dot(_rb.linearVelocity.normalized, dir);
        float accel = _acceleration * accelMul * _accelerationCurve.Evaluate(dot);
        _velGoal = Vector3.MoveTowards(_velGoal, desiredVelocity, accel * Time.fixedDeltaTime);

        // Compute needed accel and clamp to max force
        Vector3 neededAccel = (_velGoal - _rb.linearVelocity) / Time.fixedDeltaTime;
        float maxF = _maxForce * forceMul * _forceCurve.Evaluate(dot);
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxF);

        // Apply force to sheep Rigidbody
        Vector3 force = Vector3.Scale(neededAccel * _rb.mass, _forceScale);
        _rb.AddForce(force);

        // Optional: rotate sheep toward Will
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, 5f * Time.fixedDeltaTime));
        }
    }
}


