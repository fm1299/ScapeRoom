using UnityEngine;

/// <summary>
/// Versión simplificada del componente de cable - Solo simulación básica
/// </summary>
public class SimpleCableComponent : MonoBehaviour
{
    [Header("Configuración del Cable")]
    [SerializeField] private Transform endPoint;           // Punto final del cable
    [SerializeField] private Material cableMaterial;      // Material del cable
    [SerializeField] private float cableLength = 0.5f;    // Longitud del cable
    [SerializeField] private int totalSegments = 5;       // Número de segmentos
    [SerializeField] private float cableWidth = 0.1f;     // Grosor del cable
    
    private LineRenderer lineRenderer;
    private CableParticle[] particles;
    private int segments;

    void Start()
    {
        InitializeCable();
    }

    void Update()
    {
        UpdateCableVisual();
    }

    void FixedUpdate()
    {
        UpdateCablePhysics();
    }

    /// <summary>
    /// Inicializa el cable con partículas y renderer
    /// </summary>
    private void InitializeCable()
    {
        // Calcular segmentos
        segments = totalSegments;
        
        // Crear partículas
        particles = new CableParticle[segments + 1];
        Vector3 direction = (endPoint.position - transform.position).normalized;
        float segmentLength = cableLength / segments;
        
        for (int i = 0; i <= segments; i++)
        {
            Vector3 position = transform.position + direction * (segmentLength * i);
            particles[i] = new CableParticle(position);
        }
        
        // Conectar partículas inicial y final
        particles[0].Bind(transform);
        particles[segments].Bind(endPoint);
        
        // Crear LineRenderer
        CreateLineRenderer();
    }

    /// <summary>
    /// Crea el LineRenderer para visualizar el cable
    /// </summary>
    private void CreateLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = cableMaterial;
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;
    }

    /// <summary>
    /// Actualiza la visualización del cable
    /// </summary>
    private void UpdateCableVisual()
    {
        if (lineRenderer == null || particles == null) return;
        
        for (int i = 0; i <= segments; i++)
        {
            lineRenderer.SetPosition(i, particles[i].Position);
        }
    }

    /// <summary>
    /// Actualiza la física del cable
    /// </summary>
    private void UpdateCablePhysics()
    {
        if (particles == null) return;
        
        // Aplicar gravedad
        Vector3 gravity = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
        foreach (CableParticle particle in particles)
        {
            particle.UpdateVerlet(gravity);
        }
        
        // Resolver restricciones de distancia
        float segmentLength = cableLength / segments;
        for (int i = 0; i < segments; i++)
        {
            ResolveDistanceConstraint(particles[i], particles[i + 1], segmentLength);
        }
    }

    /// <summary>
    /// Resuelve la restricción de distancia entre dos partículas
    /// </summary>
    private void ResolveDistanceConstraint(CableParticle a, CableParticle b, float targetDistance)
    {
        Vector3 delta = b.Position - a.Position;
        float currentDistance = delta.magnitude;
        
        if (currentDistance == 0) return;
        
        float errorFactor = (currentDistance - targetDistance) / currentDistance;
        
        // Solo mover partículas libres
        if (a.IsFree() && b.IsFree())
        {
            a.Position += errorFactor * 0.5f * delta;
            b.Position -= errorFactor * 0.5f * delta;
        }
        else if (a.IsFree())
        {
            a.Position += errorFactor * delta;
        }
        else if (b.IsFree())
        {
            b.Position -= errorFactor * delta;
        }
    }
}

