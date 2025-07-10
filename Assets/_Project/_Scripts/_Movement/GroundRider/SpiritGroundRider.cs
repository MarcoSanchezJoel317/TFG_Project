using UnityEngine;
using UnityEngine.InputSystem;

public class SpiritGroundRider : MonoBehaviour
{
    private Rigidbody _rb;

    [Header("Raycast Setup")]
    [SerializeField]
    private Vector3 _downDir = new Vector3(0f, -1f, 0f);
    [SerializeField]
    private float _maxRayDist = 100f;
    [SerializeField]
    private LayerMask _rayMask;
    [SerializeField]
    private float _isGroundThreshold = 0.6f; // Usaremos esto para el IsGrounded()

    private bool _rayDidHit;
    private RaycastHit _rayHit;

    [Header("Ride Setup")]
    [SerializeField]
    private float _baseRideHeight = 2.5f; // Altura base deseada (tu 2.5)
    [SerializeField]
    private float _rideSpringStrenght = 300f;
    [SerializeField]
    private float _rideSpringDamper = 15f;

    [Header("Breathing/Floating Effect")]
    [SerializeField]
    private float _breathingAmplitude = 0.4f; // Magnitud de la oscilación (ej. 0.2 unidades hacia arriba/abajo)
    [SerializeField]
    private float _breathingFrequency = 1f; // Velocidad de la oscilación (ej. 1 ciclo por segundo)
    private float _currentBreathingOffset; // Offset calculado por la función sin

    Gamepad pad;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        CalculateBreathingOffset(); // Calcula el offset de la "respiración"
        Vector3 rayDir = transform.TransformDirection(_downDir);

        _rayDidHit = Physics.Raycast(transform.position, rayDir, out _rayHit, _maxRayDist, _rayMask);

        Balance();
    }

    private void CalculateBreathingOffset()
    {
        // Usa Time.time para un movimiento continuo y suave
        // Mathf.Sin oscila entre -1 y 1
        _currentBreathingOffset = Mathf.Sin(Time.time * _breathingFrequency) * _breathingAmplitude;
    }

    private void Balance()
    {
        if (_rayDidHit)
        {
            Vector3 vel = _rb.linearVelocity;
            Vector3 rayDir = transform.TransformDirection(_downDir);

            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = _rayHit.rigidbody;

            if (hitBody != null)
            {
                otherVel = hitBody.linearVelocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            // La altura objetivo ahora es la altura base más el offset de "respiración"
            float targetRideHeight = _baseRideHeight + _currentBreathingOffset;

            float x = _rayHit.distance - targetRideHeight;

            float springForce = (x * _rideSpringStrenght) - (relVel * _rideSpringDamper);

            _rb.AddForce(rayDir * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
            }
        }
    }

    // Método público para que el movimiento pueda comprobar si se está en el suelo
    public bool IsGrounded()
    {
        // Esta en el suelo si ha dado con el rayo 
        return _rayDidHit && _rayHit.distance <= _baseRideHeight + _isGroundThreshold;
    }


    
    // Dibuja los Gizmos en el Editor y en Play Mode (cuando el GameObject está seleccionado)
    private void OnDrawGizmos()
    {
        Vector3 rayDir = transform.TransformDirection(_downDir);
        RaycastHit hit;
        bool didHit = Physics.Raycast(transform.position, rayDir, out hit, _maxRayDist, _rayMask);

        // Dibuja el raycast
        Gizmos.color = didHit ? Color.blue : Color.red;
        float drawDistance = didHit ? hit.distance : _maxRayDist;
        Gizmos.DrawLine(transform.position, transform.position + rayDir * drawDistance);

        // Dibuja la altura base deseada (_baseRideHeight)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + rayDir * _baseRideHeight, 0.1f);

        // Dibuja la altura actual con el offset de "respiración"
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + rayDir * (_baseRideHeight + _currentBreathingOffset), 0.15f);

        // Dibuja el umbral de suelo
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + rayDir * (_baseRideHeight + _isGroundThreshold), 0.05f);
    }
}
