using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Versión simplificada para conectar cables - Solo maneja la conexión básica
/// </summary>
public class SimpleAttachWirePlug : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private SimpleWiresRiddleController controller;
    [SerializeField] private int outletId; // ID único del socket (0-4)
    
    [Header("Posiciones")]
    [SerializeField] private Vector3 pluggedPosition;
    [SerializeField] private Quaternion pluggedRotation;
    [SerializeField] private Vector3 initialPosition;
    [SerializeField] private Quaternion initialRotation;
    
    [Header("Audio")]
    [SerializeField] private AudioClip plugSound;
    
    private GameObject connectedPlug;
    private AudioSource audioSource;
    private bool isPlugged = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Detecta cuando un cable entra en el socket
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Solo aceptar objetos con tag "Plug" que no estén ya conectados
        if (!isPlugged && other.gameObject.CompareTag("Plug"))
        {
            ConnectWire(other.gameObject);
        }
    }

    /// <summary>
    /// Conecta el cable al socket
    /// </summary>
    private void ConnectWire(GameObject wire)
    {
        isPlugged = true;
        connectedPlug = wire;

        // Liberar el cable de la mano VR si está siendo agarrado
        OVRGrabbable grabbable = wire.GetComponent<OVRGrabbable>();
        if (grabbable != null && grabbable.isGrabbed)
        {
            grabbable.grabbedBy.ForceRelease(grabbable);
            grabbable.enabled = false; // Deshabilitar agarre temporalmente
        }

        // Hacer el cable no físico para que se quede en posición
        Rigidbody rb = wire.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Deshabilitar collider para evitar interferencias
        Collider col = wire.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Mover el cable a la posición de conexión
        StartCoroutine(MoveToPosition(wire, pluggedPosition, pluggedRotation));

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
    /// Mueve el cable suavemente a la posición de conexión
    /// </summary>
    private IEnumerator MoveToPosition(GameObject wire, Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = wire.transform.position;
        Quaternion startRot = wire.transform.rotation;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            wire.transform.position = Vector3.Lerp(startPos, targetPos, t);
            wire.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            
            yield return null;
        }

        // Asegurar posición exacta
        wire.transform.position = targetPos;
        wire.transform.rotation = targetRot;
    }

    /// <summary>
    /// Desconecta el cable del socket
    /// </summary>
    public void detach()
    {
        if (!isPlugged || connectedPlug == null) return;

        // Rehabilitar componentes del cable
        OVRGrabbable grabbable = connectedPlug.GetComponent<OVRGrabbable>();
        if (grabbable != null)
        {
            grabbable.enabled = true;
        }

        Rigidbody rb = connectedPlug.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Collider col = connectedPlug.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        // Mover de vuelta a posición inicial
        StartCoroutine(MoveToPosition(connectedPlug, initialPosition, initialRotation));

        // Reproducir sonido
        if (plugSound != null)
            audioSource.PlayOneShot(plugSound);

        // Limpiar referencias
        connectedPlug = null;
        isPlugged = false;

        Debug.Log($"Cable desconectado del socket {outletId}");
    }
}
