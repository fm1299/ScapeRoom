using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version simplificada para conectar cables - Solo maneja la conexion basica
/// </summary>
public class SimpleAttachWirePlug : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private SimpleWiresRiddleController controller;
    [SerializeField] private int outletId; // ID unico del socket (0-4)

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
        
        Debug.LogWarning($"╔════════════════════════════════════════╗");
        Debug.LogWarning($"║   SOCKET {outletId} INICIANDO                   ║");
        Debug.LogWarning($"╚════════════════════════════════════════╝");
        Debug.LogWarning($"Objeto: {gameObject.name}");
        Debug.LogWarning($"Controller asignado: {(controller != null ? "SÍ ✓" : "NO ✗")}");
        
        // Verificar que tenga un Collider con IsTrigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.LogWarning($"Collider encontrado: {col.GetType().Name}");
            Debug.LogWarning($"IsTrigger: {(col.isTrigger ? "SÍ ✓" : "NO ✗ (PROBLEMA!)")}");
        }
        else
        {
            Debug.LogError($"✗✗✗ NO HAY COLLIDER en {gameObject.name} ✗✗✗");
        }
        
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Detecta cuando un cable entra en el socket
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("");
        Debug.LogWarning($"╔════════════════════════════════════════╗");
        Debug.LogWarning($"║   TRIGGER DETECTADO en Socket {outletId}        ║");
        Debug.LogWarning($"╚════════════════════════════════════════╝");
        Debug.LogWarning($"Objeto detectado: {other.gameObject.name}");
        Debug.LogWarning($"Tag del objeto: {other.gameObject.tag}");
        Debug.LogWarning($"¿Ya está conectado?: {isPlugged}");
        Debug.LogWarning($"¿Tiene tag 'Plug'?: {other.gameObject.CompareTag("Plug")}");
        
        // Solo aceptar objetos con tag "Plug" que no esten ya conectados
        if (!isPlugged && other.gameObject.CompareTag("Plug"))
        {
            Debug.LogWarning("✓ Condiciones cumplidas - Conectando cable...");
            ConnectWire(other.gameObject);
        }
        else
        {
            if (isPlugged)
                Debug.LogWarning("✗ Socket ya ocupado");
            else if (!other.gameObject.CompareTag("Plug"))
                Debug.LogWarning($"✗ Tag incorrecto (esperaba 'Plug', recibió '{other.gameObject.tag}')");
        }
        
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Conecta el cable al socket
    /// </summary>
    private void ConnectWire(GameObject wire)
    {
        Debug.LogWarning("");
        Debug.LogWarning($"╔════════════════════════════════════════╗");
        Debug.LogWarning($"║   CONECTANDO CABLE a Socket {outletId}         ║");
        Debug.LogWarning($"╚════════════════════════════════════════╝");
        
        isPlugged = true;
        connectedPlug = wire;

        // Liberar el cable de la mano VR si esta siendo agarrado
        // Intentar con diferentes tipos de Grabbable
        MonoBehaviour grabbable = wire.GetComponent("OVRGrabbable") as MonoBehaviour;
        if (grabbable == null)
            grabbable = wire.GetComponent("Grabbable") as MonoBehaviour;
        
        if (grabbable != null)
        {
            Debug.LogWarning($"Grabbable encontrado: {grabbable.GetType().Name}");
            grabbable.enabled = false;
        }
        else
        {
            Debug.LogWarning("No se encontró Grabbable (OVR o normal)");
        }

        // Hacer el cable no fisico para que se quede en posicion
        Rigidbody rb = wire.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.LogWarning("Rigidbody convertido a Kinematic");
        }

        // Deshabilitar collider para evitar interferencias
        Collider col = wire.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            Debug.LogWarning("Collider deshabilitado");
        }

        // Mover el cable a la posicion del socket (transform de este objeto)
        Debug.LogWarning($"Moviendo a posición del socket: {transform.position}");
        StartCoroutine(MoveToSocketPosition(wire));

        // Reproducir sonido
        if (plugSound != null)
            audioSource.PlayOneShot(plugSound);

        // Informar al controlador
        if (controller != null)
        {
            Debug.LogWarning($"→ Llamando a controller.OnWirePlugged({wire.name}, {outletId})");
            controller.OnWirePlugged(wire, outletId);
        }
        else
        {
            Debug.LogError("✗✗✗ ERROR: No hay controller asignado ✗✗✗");
        }

        Debug.LogWarning($"✓ Cable {wire.name} conectado al socket {outletId}");
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Mueve el cable suavemente a la posicion del socket
    /// </summary>
    private IEnumerator MoveToSocketPosition(GameObject wire)
    {
        Vector3 startPos = wire.transform.position;
        Quaternion startRot = wire.transform.rotation;
        Vector3 targetPos = transform.position; // Posicion del socket
        Quaternion targetRot = transform.rotation; // Rotacion del socket
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            wire.transform.position = Vector3.Lerp(startPos, targetPos, t);
            wire.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        // Asegurar posicion exacta en el socket
        wire.transform.position = targetPos;
        wire.transform.rotation = targetRot;
    }

    /// <summary>
    /// Desconecta el cable del socket
    /// </summary>
    public void detach()
    {
        if (!isPlugged || connectedPlug == null) return;

        Debug.LogWarning($"Desconectando cable del socket {outletId}");

        // Rehabilitar componentes del cable
        MonoBehaviour grabbable = connectedPlug.GetComponent("OVRGrabbable") as MonoBehaviour;
        if (grabbable == null)
            grabbable = connectedPlug.GetComponent("Grabbable") as MonoBehaviour;
            
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

        // Reproducir sonido
        if (plugSound != null)
            audioSource.PlayOneShot(plugSound);

        // Limpiar referencias
        connectedPlug = null;
        isPlugged = false;

        Debug.LogWarning($"Cable desconectado del socket {outletId}");
    }
}
