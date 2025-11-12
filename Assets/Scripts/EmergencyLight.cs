using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class EmergencyLight : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Base rotation speed in degrees per second")]
    [SerializeField] private float baseRotationSpeed = 90f;

    [Tooltip("Maximum extra speed as time runs out")]
    [SerializeField] private float maxExtraRotationSpeed = 180f;

    [Tooltip("Axis to rotate around (e.g. Y = vertical rotation)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Tooltip("If true, rotation is done in local space")]
    [SerializeField] private bool useLocalRotation = true;

    [Header("Alarm Settings")]
    [Tooltip("Alarm sound that will loop continuously")]
    [SerializeField] private AudioClip alarmSound;

    [Range(0f, 1f)]
    [SerializeField] private float alarmVolume = 0.7f;

    [Header("Light Flash Settings")]
    [Tooltip("Light component that flashes as time decreases")]
    [SerializeField] private Light alarmLight;

    [Tooltip("Base flash speed (cycles per second)")]
    [SerializeField] private float baseFlashSpeed = 1f;

    [Tooltip("Maximum extra flash speed as time runs out")]
    [SerializeField] private float maxExtraFlashSpeed = 5f;

    [Tooltip("Maximum light intensity when flashing")]
    [SerializeField] private float maxLightIntensity = 5f;

    [Header("Timer Settings")]
    [Tooltip("Total time before game over (in seconds)")]
    [SerializeField] private float totalTime = 60f;

    [Tooltip("Event called when time runs out")]
    public UnityEvent OnTimeExpired;

    private AudioSource audioSource;
    private float remainingTime;
    private bool isRunning = true;
    private Material alarmMaterial;
    private Color baseEmissionColor;
    private bool hasEmission = false;

    void Start()
    {
        remainingTime = totalTime;

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = alarmSound;
        audioSource.loop = true;
        audioSource.volume = alarmVolume;
        if (alarmSound != null && !audioSource.isPlaying)
            audioSource.Play();

        // Try to get material emission color if the object has a renderer
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.material.HasProperty("_EmissionColor"))
        {
            alarmMaterial = rend.material;
            baseEmissionColor = alarmMaterial.GetColor("_EmissionColor");
            hasEmission = true;
            // Ensure emission is enabled
            DynamicGI.SetEmissive(rend, baseEmissionColor);
        }
    }

    void Update()
    {
        if (!isRunning) return;

        // Decrease timer
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(remainingTime, 0f);
        float timeRatio = remainingTime / totalTime;

        // Adjust rotation speed
        float currentRotationSpeed = baseRotationSpeed + (1f - timeRatio) * maxExtraRotationSpeed;
        float rotationStep = currentRotationSpeed * Time.deltaTime;
        transform.Rotate(rotationAxis * rotationStep, useLocalRotation ? Space.Self : Space.World);

        // Flash speed increases as time runs out
        float currentFlashSpeed = baseFlashSpeed + (1f - timeRatio) * maxExtraFlashSpeed;
        float flash = Mathf.Abs(Mathf.Sin(Time.time * currentFlashSpeed * Mathf.PI));

        // Control light flashing
        if (alarmLight != null)
        {
            alarmLight.intensity = flash * maxLightIntensity;
        }

        // Control material emission flashing
        if (hasEmission && alarmMaterial != null)
        {
            Color flashColor = baseEmissionColor * Mathf.LinearToGammaSpace(flash);
            alarmMaterial.SetColor("_EmissionColor", flashColor);
            DynamicGI.SetEmissive(GetComponent<Renderer>(), flashColor);
        }

        // Check if time has expired
        if (remainingTime <= 0f)
        {
            isRunning = false;
            Debug.LogWarning("Time is up! Game over!");
            if (OnTimeExpired != null)
                OnTimeExpired.Invoke();
        }
    }

    public void ResetTimer(float newTime = -1f)
    {
        remainingTime = (newTime > 0f) ? newTime : totalTime;
        isRunning = true;
    }

    public float GetRemainingTime() => remainingTime;
}
