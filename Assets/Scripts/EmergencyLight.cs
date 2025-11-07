using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EmergencyLight : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float rotationSpeed = 90f;

    [Tooltip("Axis to rotate around (e.g. Y = vertical rotation)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Tooltip("If true, rotation is done in local space")]
    [SerializeField] private bool useLocalRotation = true;

    [Header("Alarm Settings")]
    [Tooltip("Alarm sound that will loop continuously")]
    [SerializeField] private AudioClip alarmSound;

    [Tooltip("Volume of the alarm sound (0.0 - 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float alarmVolume = 0.7f;

    private AudioSource audioSource;

    void Start()
    {
        // Get or add the AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Configure the audio source
        audioSource.clip = alarmSound;
        audioSource.loop = true;
        audioSource.volume = alarmVolume;

        // Play the alarm if a sound is assigned
        if (alarmSound != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

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
