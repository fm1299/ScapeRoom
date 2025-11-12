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
    [SerializeField] private AudioClip Sound;

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
        if (!isPlugged && (other.gameObject.CompareTag("Plug") || other.gameObject.tag == "PowerSocket"))
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
        {
            audioSource.PlayOneShot(plugSound);
        }
        //if (wire.tag == "PowerSocket")
        //{
        //    audioSource.PlayOneShot(Sound);
        //}
        //else
        //{
        //    audioSource.PlayOneShot(plugSound);
        //}

        // Informar al controlador
        if (controller != null)
        {
            if (wire.tag == "Plug")
            {
                Debug.LogWarning($"→ Llamando a controller.OnWirePlugged({wire.name}, {outletId})");
                controller.OnWirePlugged(wire, outletId);
            }
            else if (wire.tag == "PowerSocket")
            {
                controller.turnOnScreen();
                Debug.LogWarning("→ Llamando a controller.turnOnScreen()");
            }
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
        Debug.Log($"Posición inicial del cable: {startPos}");
        Debug.Log($"Rotación inicial del cable (euler): {startRot.eulerAngles}");
        Debug.Log($"Rotación inicial del cable: {startRot}");
        Vector3 targetPos = transform.position; // Posicion del socket
        Quaternion targetRot = transform.rotation; // Rotacion del socket
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            if (pluggedPosition != null)
            {
                Debug.Log(targetPos);
                targetPos = transform.position + pluggedPosition;
            }

            wire.transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (pluggedRotation != null)
            {
                targetRot = Quaternion.Euler(pluggedRotation.eulerAngles);
            }
            wire.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            
            yield return null;
        }

        // Asegurar posicion exacta en el socket
        wire.transform.position = targetPos;
        wire.transform.rotation = targetRot;
    }

    /// <summary>
    /// Mueve el cable a su posicion inicial
    /// </summary>
    private IEnumerator MoveWireToInitialPosition(GameObject wire)
    {
        Vector3 startPos = wire.transform.position;
        Quaternion startRot = wire.transform.rotation;
        float duration = 0.3f;
        // Posicion inicial guardada
        Vector3 targetPos = initialPosition;
        Quaternion targetRot = initialRotation;

        float elapsed = 0f;

        Debug.LogWarning($"Moviendo cable desde {startPos} hacia posición inicial {targetPos}");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            wire.transform.position = Vector3.Lerp(startPos, targetPos, t);
            wire.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }
        // Habilitar componentes nuevamente
        MonoBehaviour grabbable = wire.GetComponent("Grabbable") as MonoBehaviour;
        //if (grabbable == null)
        //    grabbable = wire.GetComponent("Grabbable") as MonoBehaviour;
        if (grabbable != null)
        {
            grabbable.enabled = true;
            Debug.Log("Grabbable re-habilitado");
        }

        Rigidbody rb = wire.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            Debug.Log("Rigidbody restaurado");
        }

        Collider col = wire.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            Debug.LogWarning("Collider habilitado");
        }

        // Asegurar posicion exacta
        wire.transform.position = targetPos;
        wire.transform.rotation = targetRot;

        Debug.LogWarning($"Cable retornado a posición inicial: {wire.transform.position}");
    }

    /// <summary>
    /// Desconecta el cable del socket y lo devuelve a su posicion inicial
    /// </summary>
    public void Detach()
    {
        if (!isPlugged || connectedPlug == null)
        {
            Debug.LogWarning($"Socket {outletId} - No hay cable para desconectar");
            return;
        }

        Debug.LogWarning("");
        Debug.LogWarning($"╔════════════════════════════════════════╗");
        Debug.LogWarning($"║   DESCONECTANDO CABLE del Socket {outletId}    ║");
        Debug.LogWarning($"╚════════════════════════════════════════╝");
        // Mover a posicion inicial
        StartCoroutine(MoveWireToInitialPosition(connectedPlug));

        // Limpiar estado
        connectedPlug = null;
        isPlugged = false;

        Debug.LogWarning($"✓ Cable desconectado del socket {outletId}");
        Debug.LogWarning("═══════════════════════════════════════");
    }
}
