using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version SIMPLIFICADA para conectar cables - Funciona con cualquier Grabbable
/// </summary>
public class SimpleAttachWirePlug : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private SimpleWiresRiddleController controller;
    [SerializeField] private int outletId; // ID unico del socket (0-4)

    [Header("Atraccion Magnetica")]
    [SerializeField] private float magneticRange = 0.15f; // Rango de atraccion magnetica
    [SerializeField] private float velocityThreshold = 0.1f; // Velocidad minima para considerar "soltado"

    [Header("Audio")]
    [SerializeField] private AudioClip plugSound;

    private GameObject connectedPlug;
    private AudioSource audioSource;
    private bool isPlugged = false;
    private GameObject potentialPlug; // Cable que esta cerca pero no conectado
    private float timeInTrigger = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Si hay un cable cerca pero no conectado, verificar si debe conectarse
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
            timeInTrigger = 0f;
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
            timeInTrigger = 0f;
        }
    }

    /// <summary>
    /// Verifica si el cable debe conectarse magneticamente
    /// LOGICA SIMPLE: Si el cable esta cerca Y casi sin moverse, conectar
    /// </summary>
    private void CheckMagneticSnap()
    {
        if (potentialPlug == null) return;

        timeInTrigger += Time.deltaTime;

        // Esperar un poco antes de intentar conectar (para dar tiempo al jugador de soltar)
        if (timeInTrigger < 0.1f) return;

        Rigidbody rb = potentialPlug.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Si el cable esta casi quieto (fue soltado)
            float velocity = rb.linearVelocity.magnitude;
            
            if (velocity < velocityThreshold)
            {
                float distance = Vector3.Distance(transform.position, potentialPlug.transform.position);
                
                if (distance <= magneticRange)
                {
                    ConnectWire(potentialPlug);
                }
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
        timeInTrigger = 0f;

        // Deshabilitar TODOS los Grabbables que pueda tener
        MonoBehaviour[] allComponents = wire.GetComponents<MonoBehaviour>();
        foreach (var comp in allComponents)
        {
            if (comp != null && comp.GetType().Name.Contains("Grabbable"))
            {
                comp.enabled = false;
            }
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

        // Rehabilitar TODOS los Grabbables
        MonoBehaviour[] allComponents = connectedPlug.GetComponents<MonoBehaviour>();
        foreach (var comp in allComponents)
        {
            if (comp != null && comp.GetType().Name.Contains("Grabbable"))
            {
                comp.enabled = true;
            }
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
