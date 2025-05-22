using UnityEngine;

public class WillGroundRider : MonoBehaviour
{
    private Rigidbody _rb;

    [Header("Raycast Setup")]

    [SerializeField]
    private Vector3 _downDir = new Vector3(0f, -1f, 0f);

    [SerializeField]
    private float _maxRayDist;

    [SerializeField]
    private LayerMask _rayMask;

    [SerializeField]
    private float _isGroundThreshold;

    private bool _rayDidHit;

    private RaycastHit _rayHit;




    [Header("Ride Setup")]

    [SerializeField]
    private float _rideHeight;

    [SerializeField]
    private float _rideSpringStrenght;

    [SerializeField]
    private float _rideSpringDamper;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 rayDir = transform.TransformDirection(_downDir);

        // Realizar el raycast para detectar el suelo
        _rayDidHit = Physics.Raycast(transform.position, rayDir, out _rayHit, _maxRayDist, _rayMask);

        CustomLogger.Log(this, _rayDidHit.ToString());

        Color debugColor = _rayDidHit ? Color.blue : Color.red; // Verde si golpeó, rojo si no
        CustomLogger.Log(this, debugColor.ToString());

        float debugDrawDistance = _rayDidHit ? _rayHit.distance : _maxRayDist; // Dibuja hasta el punto de impacto o hasta la distancia máxima
        Debug.DrawLine(transform.position, transform.position + rayDir * debugDrawDistance, debugColor);

        //Dibujar Gizmos
        MantenerAFlote();
    }


    private void MantenerAFlote()
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

            float x = _rayHit.distance - _rideHeight;

            float springForce = (x * _rideSpringStrenght) - (relVel * _rideSpringDamper);

            Debug.DrawLine(transform.position, transform.position + (rayDir * springForce), Color.yellow);

            _rb.AddForce(rayDir * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
            }
        }

    }
}
