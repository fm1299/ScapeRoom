using UnityEngine;

public class FloatingRock : MonoBehaviour
{
    [Header("Zero-Gravity Motion")]
    public Vector3 localVelocity; // motion relative to parent (Space Outside)
    public float initialSpeed = 2f;
    public float impulseStrength = 5f;
    public bool isActive = false; // start inactive

    void Start()
    {
        // Random direction relative to parent space
        localVelocity = Random.onUnitSphere * initialSpeed;
    }

    void Update()
    {
        if (!isActive) return;

        // Move relative to parent Space Outside
        transform.localPosition += localVelocity * Time.deltaTime;
    }

    public void ApplyImpulse(Vector3 direction, float magnitude)
    {
        localVelocity += direction.normalized * magnitude;
    }

    public void ActivateMotion(bool active)
    {
        isActive = active;
    }
}
