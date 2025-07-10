using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controla la posición y el pulso vertical del Espíritu Santo,
/// usando navegación sobre NavMesh y un suave efecto de “respiración”.
/// Además ajusta la emisión del material según su alineación con la Voluntad en el plano XZ.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SpiritMovement : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform de la oveja, punto de origen de la guía.")]
    [SerializeField] private Transform _origin;
    [Tooltip("Transform del punto objetivo hacia el que guiar al jugador.")]
    [SerializeField] private Transform _target;

    [Header("Ajustes de Navegación")]
    [Tooltip("Distancia máxima en el plano XZ desde el origen.")]
    [SerializeField] private float _maxDistanceFromOrigin = 10f;
    [Tooltip("Altura mínima sobre el origen.")]
    [SerializeField] private float _minHeightAboveOrigin = 4f;

    [Header("Pulso Vertical (Respiración)")]
    [Tooltip("Ciclos por segundo del pulso senoidal.")]
    [SerializeField] private float _breathingFrequency = 0.5f;
    [Tooltip("Amplitud en unidades Y del pulso de respiración.")]
    [SerializeField] private float _breathingAmplitude = 0.3f;

    [Header("Emisión por Alineación")]
    [Tooltip("Transform de la Voluntad para medir alineación.")]
    [SerializeField] private Transform _will;
    [Tooltip("Intensidad máxima de emisión cuando estén alineados.")]
    [SerializeField] private float _maxEmission = 5f;

    private NavMeshAgent _agent;
    private Material    _material;
    private Color       _baseEmission;

    /// <summary>
    /// Inicializa el agente de navegación y material para emisión.
    /// </summary>
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = true;
        _agent.updateUpAxis   = true;

        // Instanciar el material para modificar su emisión
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            _material = renderer.material;
            _baseEmission = _material.GetColor("_EmissionColor");
            _material.EnableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// Cada frame ajusta destino en XZ recortado al radio máximo.
    /// </summary>
    private void Update()
    {
        if (_origin == null || _target == null) return;

        // Proyección al plano XZ
        Vector3 originXZ = new Vector3(_origin.position.x, 0f, _origin.position.z);
        Vector3 targetXZ = new Vector3(_target.position.x, 0f, _target.position.z);

        // Cálculo de offset y recorte
        Vector3 offset = targetXZ - originXZ;
        if (offset.magnitude > _maxDistanceFromOrigin)
            offset = offset.normalized * _maxDistanceFromOrigin;

        // Destino dentro del radio permitido
        Vector3 dest = originXZ + offset;
        dest.y = _origin.position.y;

        _agent.SetDestination(dest);
    }

    /// <summary>
    /// Ajusta altura con pulso senoidal y modula emisión según alineación en XZ.
    /// </summary>
    private void LateUpdate()
    {
        if (_origin == null) return;

        // Pulso vertical
        float pulse = Mathf.Sin(Time.time * Mathf.PI * 2f * _breathingFrequency)
                    * _breathingAmplitude;
        float baseY = _origin.position.y + _minHeightAboveOrigin;
        Vector3 pos = transform.position;
        pos.y = baseY + pulse;
        transform.position = pos;

        // Emisión según alineación en el plano horizontal (XZ)
        if (_material != null && _will != null)
        {
            // Vectores desde el origen proyectados en XZ
            Vector3 toWill   = _will.position - _origin.position;
            Vector3 toSpirit = transform.position - _origin.position;
            toWill.y = 0f;
            toSpirit.y = 0f;

            float angle = Vector3.Angle(toWill, toSpirit);
            float threshold = 15f; // grados máximos para considerar “alineado”
            float t = (angle <= threshold)
                 ? 1f - (angle / threshold)  // De 1 (0°) a 0 (15°)
                 : 0f;                       // Fuera del umbral, emisión base


            Color emission = _baseEmission + Color.white * (t * _maxEmission);
            _material.SetColor("_EmissionColor", emission);
        }
    }

    /// <summary>
    /// Dibuja en editor el radio máximo y la altura mínima.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_origin == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_origin.position, _maxDistanceFromOrigin);

        Gizmos.color = Color.yellow;
        Vector3 floor   = _origin.position;
        Vector3 ceiling = floor + Vector3.up * _minHeightAboveOrigin;
        Gizmos.DrawLine(floor, ceiling);
    }
}







