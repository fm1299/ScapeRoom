using UnityEngine;

/// <summary>
/// Manages all space rocks. Keeps them floating and responsive to collisions.
/// </summary>
public class SpaceRockManager : MonoBehaviour
{
    private Rigidbody[] rockRigidbodies;
    private bool activated = false;

    [Header("Zero-G Settings")]
    [Tooltip("Small drag applied to prevent rocks from drifting infinitely.")]
    public float linearDamping = 0.1f;
    public float angularDamping = 0.05f;

    [Tooltip("Initial random impulse for floating effect.")]
    public float initialImpulse = 0.5f;

    public void ActivateRocks()
    {
        if (activated) return;
        activated = true;

        rockRigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (var rb in rockRigidbodies)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.linearDamping = linearDamping;
            rb.angularDamping = angularDamping;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Give them a tiny random impulse so they slowly drift
            rb.linearVelocity = Random.onUnitSphere * initialImpulse;
            rb.angularVelocity = Random.onUnitSphere * initialImpulse * 0.5f;
        }

        Debug.Log($"[SpaceRockManager] Activated {rockRigidbodies.Length} rocks.");
    }
}
