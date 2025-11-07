using UnityEngine;

/// <summary>
/// Controla el movimiento de objetos externos para simular el movimiento de una nave
/// El lever controla velocidad adelante/atrás
/// El wheel/knob controla rotación izquierda/derecha
/// </summary>
public class SpaceOutsideController : MonoBehaviour
{
    [Header("Control References")]
    [Tooltip("Lever handle transform - detecta rotación en X para adelante/atrás")]
    public Transform lever;

    [Tooltip("Wheel/Knob transform - detecta rotación en Y para izquierda/derecha")]
    public Transform knob;

    [Header("Speed Settings")]
    [Tooltip("Velocidad máxima de movimiento hacia adelante")]
    public float maxForwardSpeed = 10f;

    [Tooltip("Velocidad de rotación lateral (grados por segundo)")]
    public float maxRotationSpeed = 30f;

    [Tooltip("Suavizado del movimiento")]
    [Range(0.1f, 10f)]
    public float smoothing = 5f;

    [Header("Lever Configuration")]
    [Tooltip("Ángulo del lever cuando está en posición neutral (parado)")]
    public float leverNeutralAngle = 0f;

    [Tooltip("Ángulo máximo del lever hacia adelante (velocidad máxima)")]
    public float leverMaxForwardAngle = 70f;

    [Tooltip("Ángulo máximo del lever hacia atrás (reversa)")]
    public float leverMaxBackwardAngle = -70f;

    [Tooltip("Zona muerta del lever (para evitar movimiento no intencional)")]
    [Range(0f, 20f)]
    public float leverDeadzone = 5f;

    [Header("Wheel Configuration")]
    [Tooltip("Zona muerta del wheel (para evitar rotación no intencional)")]
    [Range(0f, 30f)]
    public float wheelDeadzone = 10f;

    [Tooltip("Ángulo máximo del wheel para rotación completa")]
    public float wheelMaxRotation = 90f;

    [Header("Debug")]
    [Tooltip("Mostrar información de debug en consola")]
    public bool showDebugInfo = false;

    // Variables privadas
    private float currentForwardSpeed = 0f;
    private float currentRotationSpeed = 0f;
    private float targetForwardSpeed = 0f;
    private float targetRotationSpeed = 0f;

    // Almacenar rotación previa del wheel para detectar cambios
    private float previousKnobRotation = 0f;

    void Start()
    {
        // Inicializar rotación del knob
        if (knob != null)
        {
            previousKnobRotation = GetKnobRotationAngle();
        }

        // Verificar referencias
        if (lever == null)
            Debug.LogWarning("SpaceOutsideController: No se asignó el lever!");

        if (knob == null)
            Debug.LogWarning("SpaceOutsideController: No se asignó el knob/wheel!");

        //// Configurar audio
        //if (engineSound != null)
        //{
        //    engineSound.pitch = minEnginePitch;
        //    if (!engineSound.isPlaying)
        //        engineSound.Play();
        //}

        //// Configurar partículas
        //if (thrusterEffect != null)
        //{
        //    var emission = thrusterEffect.emission;
        //    emission.rateOverTime = minEmissionRate;
        //}
    }

    void Update()
    {
        // Calcular velocidades objetivo basadas en controles
        CalculateTargetSpeeds();

        // Suavizar las velocidades
        SmoothSpeeds();

        // Aplicar movimiento a los objetos externos
        ApplyMovement();

        // Actualizar feedback visual y audio
        UpdateFeedback();

        // Debug
        if (showDebugInfo)
        {
            Debug.Log($"Forward: {currentForwardSpeed:F2} | Rotation: {currentRotationSpeed:F2}");
        }
    }

    /// <summary>
    /// Calcula las velocidades objetivo basadas en la posición del lever y wheel
    /// </summary>
    void CalculateTargetSpeeds()
    {
        // === LEVER: Controla velocidad adelante/atrás ===
        if (lever != null)
        {
            float leverAngle = GetLeverAngle();

            // Aplicar zona muerta
            if (Mathf.Abs(leverAngle - leverNeutralAngle) < leverDeadzone)
            {
                targetForwardSpeed = 0f;
            }
            else
            {
                // Mapear ángulo del lever a velocidad
                if (leverAngle > leverNeutralAngle)
                {
                    // Hacia adelante
                    float normalizedAngle = (leverAngle - leverNeutralAngle) / (leverMaxForwardAngle - leverNeutralAngle);
                    targetForwardSpeed = Mathf.Clamp01(normalizedAngle) * maxForwardSpeed;
                }
                else
                {
                    // Hacia atrás (reversa)
                    float normalizedAngle = (leverAngle - leverNeutralAngle) / (leverMaxBackwardAngle - leverNeutralAngle);
                    targetForwardSpeed = -Mathf.Clamp01(normalizedAngle) * maxForwardSpeed * 0.5f; // Reversa más lenta
                }
            }
        }

        // === WHEEL/KNOB: Controla rotación izquierda/derecha ===
        if (knob != null)
        {
            float knobAngle = GetKnobRotationAngle();

            // Aplicar zona muerta
            if (Mathf.Abs(knobAngle) < wheelDeadzone)
            {
                targetRotationSpeed = 0f;
            }
            else
            {
                // Mapear rotación del wheel a velocidad de giro
                float normalizedRotation = Mathf.Clamp(knobAngle / wheelMaxRotation, -1f, 1f);
                targetRotationSpeed = normalizedRotation * maxRotationSpeed;
            }
        }
    }

    /// <summary>
    /// Obtiene el ángulo actual del lever en el eje X local
    /// </summary>
    float GetLeverAngle()
    {
        // Obtener rotación local del lever
        Vector3 eulerAngles = lever.localEulerAngles;
        float angle = eulerAngles.x;

        // Convertir de rango 0-360 a -180 a 180
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    /// <summary>
    /// Obtiene el ángulo de rotación del knob/wheel
    /// Detecta rotación acumulada (puede girar múltiples veces)
    /// </summary>
    float GetKnobRotationAngle()
    {
        // Para un wheel que gira libremente, queremos detectar la rotación en Y
        Vector3 eulerAngles = knob.localEulerAngles;
        float angle = eulerAngles.y;

        // Convertir a rango -180 a 180
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    /// <summary>
    /// Suaviza las transiciones de velocidad
    /// </summary>
    void SmoothSpeeds()
    {
        currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, targetForwardSpeed, smoothing * Time.deltaTime);
        currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, smoothing * Time.deltaTime);
    }

    /// <summary>
    /// Aplica el movimiento a los objetos externos
    /// </summary>
    void ApplyMovement()
    {
        // Movimiento hacia adelante/atrás (en el eje Z local negativo)
        // Los objetos externos se mueven en dirección opuesta a la "nave"
        Vector3 moveDirection = -transform.forward * currentForwardSpeed * Time.deltaTime;
        transform.position += moveDirection;

        // Rotación (alrededor del eje Y)
        // Los objetos rotan en sentido opuesto al giro de la nave
        float rotationAmount = -currentRotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationAmount, Space.World);
    }

    /// <summary>
    /// Actualiza efectos visuales y audio según la velocidad
    /// </summary>
    void UpdateFeedback()
    {
        // Calcular intensidad basada en velocidad (0 a 1)
        float speedIntensity = Mathf.Abs(currentForwardSpeed) / maxForwardSpeed;

        //// === AUDIO ===
        //if (engineSound != null)
        //{
        //    // Ajustar pitch del motor según velocidad
        //    engineSound.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedIntensity);

        //    // Ajustar volumen (opcional)
        //    engineSound.volume = Mathf.Lerp(0.3f, 1f, speedIntensity);
        //}

        //// === PARTÍCULAS (thrusters) ===
        //if (thrusterEffect != null)
        //{
        //    var emission = thrusterEffect.emission;
        //    float targetEmission = Mathf.Lerp(minEmissionRate, maxEmissionRate, speedIntensity);
        //    emission.rateOverTime = targetEmission;

        //    // Activar/desactivar según si hay movimiento
        //    if (speedIntensity > 0.01f && !thrusterEffect.isPlaying)
        //    {
        //        thrusterEffect.Play();
        //    }
        //    else if (speedIntensity <= 0.01f && thrusterEffect.isPlaying)
        //    {
        //        thrusterEffect.Stop();
        //    }
        //}
    }

    /// <summary>
    /// Obtiene la velocidad actual hacia adelante (útil para otros scripts)
    /// </summary>
    public float GetCurrentForwardSpeed()
    {
        return currentForwardSpeed;
    }

    /// <summary>
    /// Obtiene la velocidad de rotación actual
    /// </summary>
    public float GetCurrentRotationSpeed()
    {
        return currentRotationSpeed;
    }

    /// <summary>
    /// Para detener inmediatamente todo movimiento (emergencia)
    /// </summary>
    public void EmergencyStop()
    {
        currentForwardSpeed = 0f;
        currentRotationSpeed = 0f;
        targetForwardSpeed = 0f;
        targetRotationSpeed = 0f;

        Debug.Log("Emergency stop activated!");
    }

    /// <summary>
    /// Reinicia la posición del espacio exterior
    /// </summary>
    public void ResetPosition()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        EmergencyStop();

        Debug.Log("Space Outside position reset");
    }

    /// <summary>
    /// Visualización en el editor
    /// </summary>
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Dibujar dirección de movimiento
        Gizmos.color = Color.cyan;
        Vector3 moveDir = -transform.forward * (currentForwardSpeed * 0.5f);
        Gizmos.DrawRay(transform.position, moveDir);

        // Dibujar velocidad de rotación
        if (Mathf.Abs(currentRotationSpeed) > 0.1f)
        {
            Gizmos.color = currentRotationSpeed > 0 ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}