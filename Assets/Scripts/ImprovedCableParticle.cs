using UnityEngine;

/// <summary>
/// Versi�n mejorada de CableParticle con interpolaci�n de Rigidbody
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
    /// MEJORA: Incluye interpolaci�n de Rigidbody para movimiento m�s suave
    /// </summary>
    public void UpdateVerlet(Vector3 gravity)
    {
        if (IsBound())
        {
            // Si est� atado a un objeto
            if (boundRigidbody == null)
            {
                // Sin Rigidbody - solo seguir posici�n
                UpdatePosition(boundTransform.position);
            }
            else
            {
                // MEJORA: Con Rigidbody - considerar interpolaci�n y velocidad
                switch (boundRigidbody.interpolation)
                {
                    case RigidbodyInterpolation.Interpolate:
                        // Predicci�n suave del movimiento
                        Vector3 predictedPos = boundRigidbody.position +
                                             (boundRigidbody.linearVelocity * Time.fixedDeltaTime) * 0.5f;
                        UpdatePosition(predictedPos);
                        break;

                    case RigidbodyInterpolation.Extrapolate:
                        // Predicci�n completa del movimiento
                        Vector3 extrapolatedPos = boundRigidbody.position +
                                                (boundRigidbody.linearVelocity * Time.fixedDeltaTime);
                        UpdatePosition(extrapolatedPos);
                        break;

                    case RigidbodyInterpolation.None:
                    default:
                        // Sin interpolaci�n
                        UpdatePosition(boundRigidbody.position);
                        break;
                }
            }
        }
        else
        {
            // Part�cula libre - aplicar f�sica
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
