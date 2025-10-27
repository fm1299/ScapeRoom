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

    private XRSimpleInteractable interactable;
    private Material buttonMaterial;

    void Start()
    {
        // Obtener el componente de interaccion
        interactable = GetComponent<XRSimpleInteractable>();
        
        if (interactable != null)
        {
            interactable.selectEntered.AddListener((x) => TryOpenDoor());
        }

        // Obtener o crear material para el boton
        if (buttonRenderer != null)
        {
            buttonMaterial = buttonRenderer.material;
        }

        // Configurar estado inicial (deshabilitado)
        SetButtonState(isEnabled);
    }

    /// <summary>
    /// Intenta abrir/cerrar la puerta (solo si el boton esta habilitado)
    /// </summary>
    public void TryOpenDoor()
    {
        if (!isEnabled)
        {
            Debug.LogWarning("El boton esta deshabilitado! Completa el puzzle de cables primero.");
            return;
        }

        OpenDoor();
    }

    /// <summary>
    /// Abre/cierra la puerta
    /// </summary>
    private void OpenDoor()
    {
        Debug.Log("Boton presionado - Abriendo/Cerrando puerta");
        bool isOpen = animator.GetBool(boolName);
        animator.SetBool(boolName, !isOpen);
    }

    /// <summary>
    /// Habilita el boton (llamado por SimpleWiresRiddleController)
    /// </summary>
    public void EnableButton()
    {
        isEnabled = true;
        SetButtonState(true);
        Debug.Log("Boton HABILITADO - Ahora puedes presionarlo!");
    }

    /// <summary>
    /// Deshabilita el boton
    /// </summary>
    public void DisableButton()
    {
        isEnabled = false;
        SetButtonState(false);
        Debug.Log("Boton DESHABILITADO");
    }

    /// <summary>
    /// Configura el estado visual del boton
    /// </summary>
    private void SetButtonState(bool enabled)
    {
        if (buttonMaterial != null)
        {
            // Cambiar color segun el estado
            buttonMaterial.color = enabled ? enabledColor : disabledColor;
            
            // Opcional: Cambiar emision para hacerlo mas visible
            if (enabled)
            {
                buttonMaterial.EnableKeyword("_EMISSION");
                buttonMaterial.SetColor("_EmissionColor", enabledColor * 0.5f);
            }
            else
            {
                buttonMaterial.DisableKeyword("_EMISSION");
            }
        }

        // Opcional: Cambiar la interactividad visual del XRSimpleInteractable
        if (interactable != null)
        {
            interactable.enabled = enabled; // Deshabilitar interaccion completamente si esta deshabilitado
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
