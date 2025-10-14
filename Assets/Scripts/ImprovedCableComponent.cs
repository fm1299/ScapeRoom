using UnityEngine;

/// <summary>
/// Versi�n mejorada del cable con caracter�sticas del original
/// </summary>
public class ImprovedCableComponent : MonoBehaviour
{
    [Header("Configuraci�n del Cable")]
    [SerializeField] private Transform endPoint;
    [SerializeField] private Material cableMaterial;
    [SerializeField] private float cableLength = 0.5f;
    [SerializeField] private int totalSegments = 5;
    [SerializeField] private float segmentsPerUnit = 2f;  // MEJORA: Auto-calcula segmentos
    [SerializeField] private float cableWidth = 0.02f;

    [Header("Configuraci�n de F�sica (M�s = M�s Realista)")]
    [SerializeField] private int verletIterations = 3;    // MEJORA: M�ltiples iteraciones
    [SerializeField] private int solverIterations = 3;    // MEJORA: M�s precisi�n

    private LineRenderer lineRenderer;
    private ImprovedCableParticle[] particles;
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
        // MEJORA: M�ltiples iteraciones para mejor estabilidad
        for (int i = 0; i < verletIterations; i++)
        {
            UpdateCablePhysics();
            SolveConstraints();
        }
    }

    private void InitializeCable()
    {
        // MEJORA: Calcula segmentos autom�ticamente si es necesario
        if (totalSegments > 0)
            segments = totalSegments;
        else
            segments = Mathf.CeilToInt(cableLength * segmentsPerUnit);

        // Crear part�culas
        particles = new ImprovedCableParticle[segments + 1];
        Vector3 direction = (endPoint.position - transform.position).normalized;
        float segmentLength = cableLength / segments;

        for (int i = 0; i <= segments; i++)
        {
            Vector3 position = transform.position + direction * (segmentLength * i);
            particles[i] = new ImprovedCableParticle(position);
        }

        // Conectar part�culas inicial y final
        particles[0].Bind(transform);
        particles[segments].Bind(endPoint);

        CreateLineRenderer();
    }

    private void CreateLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = cableMaterial;
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = true;
    }

    private void UpdateCableVisual()
    {
        if (lineRenderer == null || particles == null) return;

        for (int i = 0; i <= segments; i++)
        {
            lineRenderer.SetPosition(i, particles[i].Position);
        }
    }

    private void UpdateCablePhysics()
    {
        if (particles == null) return;

        // Aplicar gravedad con Verlet Integration
        Vector3 gravity = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
        foreach (ImprovedCableParticle particle in particles)
        {
            particle.UpdateVerlet(gravity);
        }
    }

    private void SolveConstraints()
    {
        // MEJORA: M�ltiples iteraciones del solver
        for (int iteration = 0; iteration < solverIterations; iteration++)
        {
            SolveDistanceConstraints();
            SolveStiffnessConstraint();  // MEJORA: Restricci�n de rigidez
        }
    }

    private void SolveDistanceConstraints()
    {
        float segmentLength = cableLength / segments;

        for (int i = 0; i < segments; i++)
        {
            ResolveDistanceConstraint(particles[i], particles[i + 1], segmentLength);
        }
    }

    private void ResolveDistanceConstraint(ImprovedCableParticle a, ImprovedCableParticle b, float targetDistance)
    {
        Vector3 delta = b.Position - a.Position;
        float currentDistance = delta.magnitude;

        if (currentDistance == 0) return;

        float errorFactor = (currentDistance - targetDistance) / currentDistance;

        // Solo mover part�culas libres
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

    /// <summary>
    /// MEJORA: Restricci�n de rigidez - Evita que el cable se estire demasiado
    /// </summary>
    private void SolveStiffnessConstraint()
    {
        float currentDistance = (particles[0].Position - particles[segments].Position).magnitude;

        // Si el cable se estir� m�s de lo permitido
        if (currentDistance > cableLength)
        {
            float stretchFactor = cableLength / currentDistance;
            Vector3 center = (particles[0].Position + particles[segments].Position) * 0.5f;

            // Acercar todas las part�culas hacia el centro proporcionalmente
            for (int i = 1; i < segments; i++) // No mover los extremos
            {
                if (particles[i].IsFree())
                {
                    Vector3 toCenter = center - particles[i].Position;
                    particles[i].Position += toCenter * (1f - stretchFactor) * 0.1f;
                }
            }
        }
    }
}
