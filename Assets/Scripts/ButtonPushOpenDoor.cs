using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ButtonPushOpenDoor : MonoBehaviour
{
    [Header("Configuracion de la Puerta")]
    public Animator animator;
    public string boolName = "Open";

    [Header("Estados del Boton")]
    [SerializeField] private bool isEnabled = false; // Boton deshabilitado por defecto
    [SerializeField] private MeshRenderer buttonRenderer; // Renderer del boton para cambiar color
    [SerializeField] private Color disabledColor = Color.red; // Color cuando esta deshabilitado
    [SerializeField] private Color enabledColor = Color.green; // Color cuando esta habilitado

    [Header("Feedback Visual")]
    [SerializeField] private float pulseSpeed = 2f; // Velocidad de parpadeo cuando esta deshabilitado

    private XRSimpleInteractable interactable;
    private Material buttonMaterial;
    private bool isPulsing = false;

    void Start()
    {
        // Obtener el componente de interaccion
        interactable = GetComponent<XRSimpleInteractable>();
        
        if (interactable != null)
        {
            // IMPORTANTE: El listener siempre verifica si esta habilitado antes de actuar
            interactable.selectEntered.AddListener((x) => TryOpenDoor());
        }

        // Obtener o crear material para el boton
        if (buttonRenderer != null)
        {
            // Crear una instancia del material para no afectar otros objetos
            buttonMaterial = new Material(buttonRenderer.material);
            buttonRenderer.material = buttonMaterial;
        }

        // Configurar estado inicial (deshabilitado)
        UpdateButtonVisual();
    }

    void Update()
    {
        // Si esta deshabilitado, hacer pulsar el color rojo como advertencia
        if (!isEnabled && isPulsing && buttonMaterial != null)
        {
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            Color currentColor = Color.Lerp(disabledColor * 0.5f, disabledColor, pulse);
            buttonMaterial.color = currentColor;
        }
    }

    /// <summary>
    /// Intenta abrir/cerrar la puerta (solo si el boton esta habilitado)
    /// </summary>
    public void TryOpenDoor()
    {
        if (!isEnabled)
        {
            Debug.LogWarning("¡BOTON BLOQUEADO! Completa el puzzle de cables primero.");
            // Activar pulsacion temporal para feedback visual
            StartCoroutine(PulseDisabledFeedback());
            return;
        }

        // Si esta habilitado, abrir/cerrar la puerta
        OpenDoor();
    }

    /// <summary>
    /// Abre/cierra la puerta
    /// </summary>
    private void OpenDoor()
    {
        if (animator == null)
        {
            Debug.LogError("¡ERROR! No hay Animator asignado al boton.");
            return;
        }

        Debug.Log("Boton VERDE presionado - Abriendo/Cerrando puerta");
        bool isOpen = animator.GetBool(boolName);
        animator.SetBool(boolName, !isOpen);
    }

    /// <summary>
    /// Habilita el boton (llamado por SimpleWiresRiddleController)
    /// </summary>
    public void EnableButton()
    {
        isEnabled = true;
        isPulsing = false;
        UpdateButtonVisual();
        Debug.Log("✓ BOTON HABILITADO - ¡Ahora puedes presionarlo para abrir la puerta!");
    }

    /// <summary>
    /// Deshabilita el boton
    /// </summary>
    public void DisableButton()
    {
        isEnabled = false;
        UpdateButtonVisual();
        Debug.Log("✗ BOTON DESHABILITADO - Completa el puzzle primero");
    }

    /// <summary>
    /// Actualiza el estado visual del boton
    /// </summary>
    private void UpdateButtonVisual()
    {
        if (buttonMaterial == null) return;

        if (isEnabled)
        {
            // Boton HABILITADO - Verde brillante
            buttonMaterial.color = enabledColor;
            
            // Activar emision para hacerlo brillar
            buttonMaterial.EnableKeyword("_EMISSION");
            buttonMaterial.SetColor("_EmissionColor", enabledColor * 0.8f);
            
            isPulsing = false;
        }
        else
        {
            // Boton DESHABILITADO - Rojo apagado
            buttonMaterial.color = disabledColor * 0.7f;
            
            // Desactivar emision
            buttonMaterial.DisableKeyword("_EMISSION");
            buttonMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    /// <summary>
    /// Feedback visual cuando intentan presionar el boton deshabilitado
    /// </summary>
    private System.Collections.IEnumerator PulseDisabledFeedback()
    {
        isPulsing = true;
        yield return new WaitForSeconds(1f);
        
        if (!isEnabled) // Si sigue deshabilitado, volver al color normal
        {
            isPulsing = false;
            UpdateButtonVisual();
        }
    }

    /// <summary>
    /// Obtiene el estado actual del boton
    /// </summary>
    public bool IsEnabled()
    {
        return isEnabled;
    }
}
