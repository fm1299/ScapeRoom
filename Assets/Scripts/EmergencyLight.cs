using UnityEngine;

public class EmergencyLight : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Axis to rotate around (e.g. Y = vertical rotation)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Tooltip("If true, rotation is done in local space")]
    [SerializeField] private bool useLocalRotation = true;

    void Update()
    {
        // Determine rotation step based on speed and deltaTime
        float rotationStep = rotationSpeed * Time.deltaTime;

        // Apply rotation
        if (useLocalRotation)
        {
            transform.Rotate(rotationAxis * rotationStep, Space.Self);
        }
        else
        {
            transform.Rotate(rotationAxis * rotationStep, Space.World);
        }
    }
}
