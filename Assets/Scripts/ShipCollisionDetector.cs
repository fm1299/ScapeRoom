using UnityEngine;

/// <summary>
/// Detects collisions between the ship and space rocks.
/// When a collision occurs, it applies a push force to the rock's Rigidbody
/// so it starts drifting naturally in zero gravity.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ShipCollisionDetector : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Multiplier for how strongly the ship pushes rocks on impact.")]
    public float collisionForce = 5f;

    [Tooltip("Optional: enable this to draw debug lines when collisions happen.")]
    public bool showDebug = true;

    [Header("Optional Effects")]
    [Tooltip("Optional particle effect for collisions (e.g., sparks).")]
    public ParticleSystem collisionEffect;

    [Tooltip("Optional sound to play on impact.")]
    public AudioSource collisionSound;

    // Reference to SpaceRockManager (optional if you need to notify it)
    private SpaceRockManager rockManager;

    private void Start()
    {
        // Find the SpaceRockManager in the scene (optional)
        rockManager = FindObjectOfType<SpaceRockManager>();

        // Make sure the collider is a trigger or solid depending on setup
        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogWarning("ShipCollisionDetector: No collider found on the ship!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the other object is a space rock
        if (collision.collider.CompareTag("SpaceRock"))
        {
            Rigidbody rockRb = collision.collider.attachedRigidbody;
            if (rockRb != null)
            {
                // Apply a force away from the ship
                Vector3 pushDirection = collision.contacts[0].normal;
                rockRb.AddForce(pushDirection * collisionForce, ForceMode.Impulse);

                if (showDebug)
                {
                    Debug.DrawRay(collision.contacts[0].point, pushDirection * 3f, Color.red, 2f);
                    Debug.Log($"Ship collided with rock: {collision.collider.name}");
                }

                // Optional particle or sound feedback
                if (collisionEffect != null)
                {
                    collisionEffect.transform.position = collision.contacts[0].point;
                    collisionEffect.Play();
                }

                if (collisionSound != null)
                    collisionSound.Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // This supports triggers as well (if your ship collider is set to trigger)
        if (other.CompareTag("SpaceRock"))
        {
            Rigidbody rockRb = other.attachedRigidbody;
            if (rockRb != null)
            {
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                rockRb.AddForce(pushDirection * collisionForce, ForceMode.Impulse);

                if (showDebug)
                {
                    Debug.DrawRay(transform.position, pushDirection * 3f, Color.cyan, 2f);
                    Debug.Log($"Triggered rock: {other.name}");
                }

                if (collisionEffect != null)
                {
                    collisionEffect.transform.position = other.transform.position;
                    collisionEffect.Play();
                }

                if (collisionSound != null)
                    collisionSound.Play();
            }
        }
    }
}
