using UnityEngine;

/// <summary>
/// Controls the external environment to simulate ship movement.
/// Moves the parent "SpaceOutside" so it looks like the ship is moving.
/// </summary>
public class SpaceOutsideController : MonoBehaviour
{
    [Header("Control References")]
    public Transform lever;
    public Transform knob;

    [Header("Speed Settings")]
    public float maxForwardSpeed = 10f;
    public float maxRotationSpeed = 30f;
    [Range(0.1f, 10f)] public float smoothing = 5f;

    [Header("Lever Configuration")]
    public float leverNeutralAngle = 0f;
    public float leverMaxForwardAngle = 70f;
    public float leverMaxBackwardAngle = -70f;
    [Range(0f, 20f)] public float leverDeadzone = 5f;

    [Header("Wheel Configuration")]
    [Range(0f, 30f)] public float wheelDeadzone = 10f;
    public float wheelMaxRotation = 90f;

    [Header("Debug")]
    public bool showDebugInfo = false;

    // Internal variables
    private float currentForwardSpeed = 0f;
    private float currentRotationSpeed = 0f;
    private float targetForwardSpeed = 0f;
    private float targetRotationSpeed = 0f;

    private SpaceRockManager rockManager;

    void Start()
    {
        // Cache rock manager and activate rocks
        rockManager = FindObjectOfType<SpaceRockManager>();
        if (rockManager != null)
            rockManager.ActivateRocks();

        if (lever == null)
            Debug.LogWarning("SpaceOutsideController: Lever not assigned!");

        if (knob == null)
            Debug.LogWarning("SpaceOutsideController: Knob/Wheel not assigned!");
    }

    // Use FixedUpdate to move the world so physics stays stable
    void FixedUpdate()
    {
        CalculateTargetSpeeds();
        SmoothSpeeds();
        ApplyMovement();

        if (showDebugInfo)
            Debug.Log($"Forward: {currentForwardSpeed:F2} | Rotation: {currentRotationSpeed:F2}");
    }

    void CalculateTargetSpeeds()
    {
        // === LEVER: Forward/Backward ===
        if (lever != null)
        {
            float leverAngle = GetLeverAngle();

            if (Mathf.Abs(leverAngle - leverNeutralAngle) < leverDeadzone)
                targetForwardSpeed = 0f;
            else if (leverAngle > leverNeutralAngle)
            {
                float normalized = (leverAngle - leverNeutralAngle) / (leverMaxForwardAngle - leverNeutralAngle);
                targetForwardSpeed = Mathf.Clamp01(normalized) * maxForwardSpeed;
            }
            else
            {
                float normalized = (leverAngle - leverNeutralAngle) / (leverMaxBackwardAngle - leverNeutralAngle);
                targetForwardSpeed = -Mathf.Clamp01(normalized) * maxForwardSpeed * 0.5f;
            }
        }

        // === WHEEL: Rotation ===
        if (knob != null)
        {
            float knobAngle = GetKnobRotationAngle();

            if (Mathf.Abs(knobAngle) < wheelDeadzone)
                targetRotationSpeed = 0f;
            else
            {
                float normalized = Mathf.Clamp(knobAngle / wheelMaxRotation, -1f, 1f);
                targetRotationSpeed = normalized * maxRotationSpeed;
            }
        }
    }

    void SmoothSpeeds()
    {
        currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, targetForwardSpeed, smoothing * Time.fixedDeltaTime);
        currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, smoothing * Time.fixedDeltaTime);
    }

    void ApplyMovement()
    {
        Vector3 moveDirection = -transform.forward * currentForwardSpeed * Time.fixedDeltaTime;
        transform.position += moveDirection;

        float rotationAmount = -currentRotationSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up, rotationAmount, Space.World);
    }

    float GetLeverAngle()
    {
        Vector3 euler = lever.localEulerAngles;
        float angle = euler.x;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    float GetKnobRotationAngle()
    {
        Vector3 euler = knob.localEulerAngles;
        float angle = euler.y;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    public float GetCurrentForwardSpeed() => currentForwardSpeed;
    public float GetCurrentRotationSpeed() => currentRotationSpeed;
}
