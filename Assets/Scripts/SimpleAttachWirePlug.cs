using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Version mejorada para conectar cables - Compatible con XR Interaction Toolkit
/// </summary>
public class SimpleAttachWirePlug : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private SimpleWiresRiddleController controller;
    [SerializeField] private int outletId; // ID unico del socket (0-4)

    [Header("Atraccion Magnetica")]
    [SerializeField] private float magneticRange = 0.15f; // Rango de atraccion magnetica
    [SerializeField] private float snapSpeed = 5f; // Velocidad de ajuste magnetico

    [Header("Audio")]
    [SerializeField] private AudioClip plugSound;

    private GameObject connectedPlug;
    private AudioSource audioSource;
    private bool isPlugged = false;
    private XRGrabInteractable currentGrabbable;
    private GameObject potentialPlug; // Cable que esta cerca pero no conectado

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Si hay un cable cerca pero no conectado, atraerlo magneticamente
        if (!isPlugged && potentialPlug != null)
        {
            CheckMagneticSnap();
        }
    }

    /// <summary>
    /// Detecta cuando un cable entra en el rango del socket
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Solo aceptar objetos con tag "Plug" que no esten ya conectados
        if (!isPlugged && other.gameObject.CompareTag("Plug"))
        {
            potentialPlug = other.gameObject;
        }
    }

    /// <summary>
    /// Detecta cuando un cable sale del rango del socket
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == potentialPlug)
        {
            potentialPlug = null;
        }
    }

    /// <summary>
    /// Verifica si el cable debe conectarse magneticamente
    /// </summary>
    private void CheckMagneticSnap()
    {
        if (potentialPlug == null) return;

        // Obtener el XRGrabInteractable del cable
        XRGrabInteractable grabbable = potentialPlug.GetComponent<XRGrabInteractable>();
        
        // Si el cable NO esta siendo agarrado y esta dentro del rango, conectarlo
        if (grabbable != null && !grabbable.isSelected)
        {
            float distance = Vector3.Distance(transform.position, potentialPlug.transform.position);
            
            if (distance <= magneticRange)
            {
                ConnectWire(potentialPlug);
            }
        }
    }

    /// <summary>
    /// Conecta el cable al socket
    /// </summary>
    private void ConnectWire(GameObject wire)
    {
        isPlugged = true;
        connectedPlug = wire;
        potentialPlug = null;

        // Liberar el cable de la mano VR si esta siendo agarrado
        XRGrabInteractable grabbable = wire.GetComponent<XRGrabInteractable>();
        if (grabbable != null)
        {
            currentGrabbable = grabbable;
            
            // Forzar liberacion si esta agarrado
            if (grabbable.isSelected)
            {
                foreach (var interactor in grabbable.interactorsSelecting)
                {
                    (interactor as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor)?.interactionManager.SelectExit(interactor, grabbable);
                }
            }
            
            // Deshabilitar agarre mientras esta conectado
            grabbable.enabled = false;
        }

        // Hacer el cable completamente estatico
        Rigidbody rb = wire.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Deshabilitar collider para evitar interferencias
        Collider col = wire.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Mover el cable a la posicion de conexion del socket
        wire.transform.SetParent(transform); // Hacerlo hijo del socket
        StartCoroutine(MoveToPosition(wire));

        // Reproducir sonido
        if (plugSound != null)
            audioSource.PlayOneShot(plugSound);

        // Informar al controlador
        if (controller != null)
        {
            controller.OnWirePlugged(wire, outletId);
        }

        Debug.Log($"Cable conectado al socket {outletId}");
    }

    /// <summary>
    /// Mueve el cable suavemente a la posicion de conexion
    /// </summary>
    private IEnumerator MoveToPosition(GameObject wire)
    {
        Vector3 startPos = wire.transform.localPosition;
        Quaternion startRot = wire.transform.localRotation;
        Vector3 targetPos = Vector3.zero; // Posicion local (0,0,0) porque ahora es hijo del socket
        Quaternion targetRot = Quaternion.identity; // Rotacion identidad
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            wire.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            wire.transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        // Asegurar posicion exacta
        wire.transform.localPosition = targetPos;
        wire.transform.localRotation = targetRot;
    }

    /// <summary>
    /// Desconecta el cable del socket
    /// </summary>
    public void detach()
    {
        if (!isPlugged || connectedPlug == null) return;

        // Desparentar el cable
        connectedPlug.transform.SetParent(null);

        // Rehabilitar componentes del cable
        if (currentGrabbable != null)
        {
            currentGrabbable.enabled = true;
        }

        Rigidbody rb = connectedPlug.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider col = connectedPlug.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Limpiar referencias
        connectedPlug = null;
        currentGrabbable = null;
        isPlugged = false;

        Debug.Log($"Cable desconectado del socket {outletId}");
    }

    /// <summary>
    /// Permite desconectar el cable cuando el jugador lo agarra de nuevo
    /// </summary>
    public void OnCableGrabbed()
    {
        if (isPlugged)
        {
            detach();
        }
    }
}
