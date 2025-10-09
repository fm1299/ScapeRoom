using UnityEngine;

/// <summary>
/// Versión mejorada de CableParticle con interpolación de Rigidbody
/// </summary>
public class ImprovedCableParticle
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

    public ImprovedCableParticle(Vector3 initialPosition)
    {
        position = oldPosition = initialPosition;
    }

    /// <summary>
    /// MEJORA: Incluye interpolación de Rigidbody para movimiento más suave
    /// </summary>
    public void UpdateVerlet(Vector3 gravity)
    {
        if (IsBound())
        {
            // Si está atado a un objeto
            if (boundRigidbody == null)
            {
                // Sin Rigidbody - solo seguir posición
                UpdatePosition(boundTransform.position);
            }
            else
            {
                // MEJORA: Con Rigidbody - considerar interpolación y velocidad
                switch (boundRigidbody.interpolation)
                {
                    case RigidbodyInterpolation.Interpolate:
                        // Predicción suave del movimiento
                        Vector3 predictedPos = boundRigidbody.position + 
                                             (boundRigidbody.velocity * Time.fixedDeltaTime) * 0.5f;
                        UpdatePosition(predictedPos);
                        break;
                        
                    case RigidbodyInterpolation.Extrapolate:
                        // Predicción completa del movimiento
                        Vector3 extrapolatedPos = boundRigidbody.position + 
                                                (boundRigidbody.velocity * Time.fixedDeltaTime);
                        UpdatePosition(extrapolatedPos);
                        break;
                        
                    case RigidbodyInterpolation.None:
                    default:
                        // Sin interpolación
                        UpdatePosition(boundRigidbody.position);
                        break;
                }
            }
        }
        else
        {
            // Partícula libre - aplicar física
            Vector3 newPosition = position + Velocity + gravity;
            UpdatePosition(newPosition);
        }
    }

    public void UpdatePosition(Vector3 newPos)
    {
        oldPosition = position;
        position = newPos;
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

