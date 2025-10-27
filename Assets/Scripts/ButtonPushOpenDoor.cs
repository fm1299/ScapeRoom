using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ButtonPushOpenDoor : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public Animator animator;
    public string boolName = "Open";

    [Header("Estados del Botón")]
    [SerializeField] private bool isEnabled = false; // Botón deshabilitado por defecto

    [Header("Indicador Visual (Opcional)")]
    [SerializeField] private Light buttonLight; // Luz para indicar estado (roja/verde)

    private XRSimpleInteractable interactable;

    void Start()
    {
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   ButtonPushOpenDoor INICIANDO         ║");
        Debug.LogWarning($"║   Objeto: {gameObject.name,-27} ║");
        Debug.LogWarning($"║   Estado inicial: {(isEnabled ? "HABILITADO " : "DESHABILITADO"),-17} ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        
        // Configurar el interactable
        interactable = GetComponent<XRSimpleInteractable>();
        
        if (interactable != null)
        {
            Debug.LogWarning("✓ XRSimpleInteractable encontrado");
            
            // IMPORTANTE: Remover TODOS los listeners anteriores
            interactable.selectEntered.RemoveAllListeners();
            Debug.LogWarning("✓ Listeners antiguos removidos");
            
            // Agregar SOLO nuestro listener
            interactable.selectEntered.AddListener(OnButtonPressed);
            Debug.LogWarning("✓ Nuevo listener agregado: OnButtonPressed");
        }
        else
        {
            Debug.LogError("╔════════════════════════════════════════╗");
            Debug.LogError("║      ✗✗✗ ERROR CRÍTICO ✗✗✗            ║");
            Debug.LogError("║   No se encontró XRSimpleInteractable  ║");
            Debug.LogError($"║   en {gameObject.name,-31} ║");
            Debug.LogError("║   El botón NO funcionará               ║");
            Debug.LogError("╚════════════════════════════════════════╝");
        }

        // Configurar estado visual inicial
        UpdateVisualState();
        
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Se llama cuando se presiona el botón - ESTE ES EL MÉTODO CRÍTICO
    /// </summary>
    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║        BOTÓN PRESIONADO !!!            ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        Debug.LogWarning($"Estado actual: isEnabled = {isEnabled}");
        
        // VERIFICACIÓN CRÍTICA: Si NO está habilitado, SALIR inmediatamente
        if (!isEnabled)
        {
            Debug.LogWarning("╔════════════════════════════════════════╗");
            Debug.LogWarning("║        ⛔ ACCESO DENEGADO ⛔           ║");
            Debug.LogWarning("║   El botón está BLOQUEADO              ║");
            Debug.LogWarning("║   Completa el puzzle de cables         ║");
            Debug.LogWarning("║   LA PUERTA NO SE ABRIRÁ               ║");
            Debug.LogWarning("╚════════════════════════════════════════╝");
            
            // SALIR - NO hacer nada más
            return;
        }

        // Si llegamos aquí, el botón ESTÁ habilitado
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║      ✓✓✓ ACCESO PERMITIDO ✓✓✓         ║");
        Debug.LogWarning("║   Abriendo/Cerrando la puerta...       ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        
        OpenDoor();
    }

    /// <summary>
    /// Abre/cierra la puerta - MÉTODO PÚBLICO que puede ser llamado desde UnityEvents
    /// </summary>
    public void OpenDoor()
    {
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║      OpenDoor() LLAMADO                ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        Debug.LogWarning($"isEnabled = {isEnabled}");
        
        // ⚠️ VERIFICACIÓN CRÍTICA: Si el botón NO está habilitado, BLOQUEAR
        if (!isEnabled)
        {
            Debug.LogWarning("╔════════════════════════════════════════╗");
            Debug.LogWarning("║      ⛔⛔⛔ BLOQUEADO ⛔⛔⛔            ║");
            Debug.LogWarning("║   El botón NO está habilitado          ║");
            Debug.LogWarning("║   Completa el puzzle de cables         ║");
            Debug.LogWarning("║   LA PUERTA NO SE ABRIRÁ               ║");
            Debug.LogWarning("╚════════════════════════════════════════╝");
            
            // ⚠️ SALIR SIN ABRIR LA PUERTA
            return;
        }
        
        // Solo si está habilitado, continuar
        if (animator == null)
        {
            Debug.LogError("✗ ERROR: No hay Animator asignado");
            return;
        }

        bool wasOpen = animator.GetBool(boolName);
        animator.SetBool(boolName, !wasOpen);
        
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║      ✓✓✓ PUERTA ACTIVADA ✓✓✓          ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        Debug.LogWarning($"→ Animator.SetBool(\"{boolName}\", {!wasOpen})");
        Debug.LogWarning($"→ Puerta {(wasOpen ? "CERRADA" : "ABIERTA")}");
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Habilita el botón (llamado por SimpleWiresRiddleController)
    /// </summary>
    public void EnableButton()
    {
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║    🟢🟢🟢 HABILITANDO BOTÓN 🟢🟢🟢     ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        
        isEnabled = true;
        UpdateVisualState();
        
        Debug.LogWarning("✓ Botón HABILITADO");
        Debug.LogWarning("✓ Ahora puedes presionarlo para abrir la puerta");
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Deshabilita el botón
    /// </summary>
    public void DisableButton()
    {
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║    🔴🔴🔴 DESHABILITANDO BOTÓN 🔴🔴🔴  ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        
        isEnabled = false;
        UpdateVisualState();
        
        Debug.LogWarning("✓ Botón DESHABILITADO");
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Actualiza el estado visual del botón
    /// </summary>
    private void UpdateVisualState()
    {
        // Cambiar color de la luz si existe
        if (buttonLight != null)
        {
            buttonLight.color = isEnabled ? Color.green : Color.red;
            buttonLight.intensity = 2f;
            buttonLight.enabled = true;
            Debug.LogWarning($"→ Luz cambiada a {(isEnabled ? "VERDE" : "ROJO")}");
        }
        else
        {
            Debug.LogWarning("(No hay Light asignada para indicador visual)");
        }
    }

    /// <summary>
    /// Obtiene el estado actual del botón
    /// </summary>
    public bool IsEnabled()
    {
        return isEnabled;
    }
}
