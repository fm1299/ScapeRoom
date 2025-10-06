using UnityEngine;

/// <summary>
/// Clase simplificada para las partículas del cable
/// </summary>
public class CableParticle
{
    private Vector3 position;
    private Vector3 oldPosition;
    private Transform boundTransform;
    private Rigidbody boundRigidbody;

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }

    public Vector3 Velocity
    {
        get { return position - oldPosition; }
    }

    public CableParticle(Vector3 initialPosition)
    {
        position = oldPosition = initialPosition;
    }

    public void UpdateVerlet(Vector3 gravity)
    {
        if (IsBound())
        {
            if (boundRigidbody == null)
            {
                position = boundTransform.position;
            }
            else
            {
                position = boundRigidbody.position;
            }
        }
        else
        {
            Vector3 newPosition = position + Velocity + gravity;
            oldPosition = position;
            position = newPosition;
        }
    }

    public void Bind(Transform transform)
    {
        boundTransform = transform;
        boundRigidbody = transform.GetComponent<Rigidbody>();
        position = oldPosition = transform.position;
    }

    public void Unbind()
    {
        boundTransform = null;
        boundRigidbody = null;
    }

    public bool IsFree()
    {
        return boundTransform == null;
    }

    public bool IsBound()
    {
        return boundTransform != null;
    }
}
