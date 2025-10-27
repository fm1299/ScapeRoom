using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller simplificado del puzzle de cables - Solo muestra WIN cuando se completa
/// </summary>
public class SimpleWiresRiddleController : MonoBehaviour
{
    [Header("Configuración del Puzzle")]
    [SerializeField] private GameObject[] correctPlugArr; // Los 5 cables en el orden correcto
    [SerializeField] private SimpleAttachWirePlug[] socketArr;  // Los 5 sockets
    [SerializeField] private AudioClip wrongSound;        // Sonido de error

    [Header("Botón a Habilitar")]
    [SerializeField] private ButtonPushOpenDoor buttonController; // Referencia al botón

    private GameObject[] outletArr;                       // Cables actualmente conectados
    private int correctlyPluggedCounter = 0;              // Contador de cables correctos
    private bool puzzleCompleted = false;                 // Evita múltiples victorias

    void Start()
    {
        outletArr = new GameObject[5];
        
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   PUZZLE DE CABLES INICIANDO           ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        Debug.LogWarning($"Botón asignado: {(buttonController != null ? "SÍ ✓" : "NO ✗")}");
        Debug.LogWarning($"Cables correctos configurados: {correctPlugArr.Length}");
        Debug.LogWarning($"Sockets configurados: {socketArr.Length}");
        Debug.LogWarning("═══════════════════════════════════════");
    }

    /// <summary>
    /// Se llama cuando un cable se conecta
    /// </summary>
    public void OnWirePlugged(GameObject plug, int outletId)
    {
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   CABLE CONECTADO                      ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        Debug.LogWarning($"Cable: {plug.name}");
        Debug.LogWarning($"Socket ID: {outletId}");
        Debug.LogWarning($"Cable esperado: {(correctPlugArr[outletId] != null ? correctPlugArr[outletId].name : "NULL")}");
        
        if (puzzleCompleted)
        {
            Debug.LogWarning("Puzzle ya completado - ignorando");
            return;
        }

        // Verificar si el cable es correcto para este socket
        bool isCorrect = correctPlugArr[outletId] == plug;
        if (isCorrect)
        {
            correctlyPluggedCounter++;
            Debug.LogWarning($"✓ CORRECTO! ({correctlyPluggedCounter}/5)");
        }
        else
        {
            Debug.LogWarning($"✗ INCORRECTO");
        }

        // Guardar el cable conectado
        outletArr[outletId] = plug;

        // Mostrar estado actual
        Debug.LogWarning($"Cables conectados: {CountConnectedWires()}/5");
        Debug.LogWarning("═══════════════════════════════════════");

        // Verificar si todos los cables están conectados
        if (AllWiresPlugged())
        {
            Debug.LogWarning("¡Todos los cables conectados! Verificando solución...");
            CheckSolution();
        }
    }

    /// <summary>
    /// Cuenta cuántos cables están conectados
    /// </summary>
    private int CountConnectedWires()
    {
        int count = 0;
        for (int i = 0; i < 5; i++)
        {
            if (outletArr[i] != null)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Verifica si todos los cables están conectados
    /// </summary>
    private bool AllWiresPlugged()
    {
        for (int i = 0; i < 5; i++)
        {
            if (outletArr[i] == null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Verifica si la solución es correcta
    /// </summary>
    private void CheckSolution()
    {
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   VERIFICANDO SOLUCIÓN                 ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        
        // Verificar si todos los cables están en la posición correcta
        for (int i = 0; i < 5; i++)
        {
            Debug.LogWarning($"Socket {i}: {outletArr[i].name} == {correctPlugArr[i].name} ? {(outletArr[i] == correctPlugArr[i])}");
            
            if (outletArr[i] != correctPlugArr[i])
            {
                // Solución incorrecta
                Debug.LogWarning("╔════════════════════════════════════════╗");
                Debug.LogWarning("║   ✗✗✗ SOLUCIÓN INCORRECTA ✗✗✗         ║");
                Debug.LogWarning("║   Algunos cables están mal conectados  ║");
                Debug.LogWarning("╚════════════════════════════════════════╝");
                
                if (wrongSound != null)
                    AudioSource.PlayClipAtPoint(wrongSound, transform.position);
                
                return;
            }
        }

        // ¡Solución correcta!
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   ✓✓✓ SOLUCIÓN CORRECTA ✓✓✓           ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
        
        PuzzleCompleted();
    }

    /// <summary>
    /// Se ejecuta cuando el puzzle se completa correctamente
    /// </summary>
    private void PuzzleCompleted()
    {
        if (puzzleCompleted) return;

        puzzleCompleted = true;
        
        Debug.LogWarning("");
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   🎉🎉🎉 PUZZLE COMPLETADO 🎉🎉🎉      ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");

        // Habilitar el botón para abrir la puerta
        if (buttonController != null)
        {
            Debug.LogWarning("→ Llamando a buttonController.EnableButton()...");
            buttonController.EnableButton();
            Debug.LogWarning("→ Botón habilitado exitosamente!");
        }
        else
        {
            Debug.LogError("╔════════════════════════════════════════╗");
            Debug.LogError("║   ✗✗✗ ERROR CRÍTICO ✗✗✗               ║");
            Debug.LogError("║   No hay botón asignado                ║");
            Debug.LogError("║   Asigna BigRedButton en el Inspector  ║");
            Debug.LogError("╚════════════════════════════════════════╝");
        }

        // Mostrar mensaje de victoria
        ShowWinMessage();
    }

    /// <summary>
    /// Muestra el mensaje de victoria
    /// </summary>
    private void ShowWinMessage()
    {
        Debug.LogWarning("╔════════════════════════════════════════╗");
        Debug.LogWarning("║   ¡FELICIDADES!                        ║");
        Debug.LogWarning("║   ¡Ahora puedes abrir la puerta!       ║");
        Debug.LogWarning("╚════════════════════════════════════════╝");
    }

    /// <summary>
    /// Resetea el puzzle (opcional - para reiniciar)
    /// </summary>
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        correctlyPluggedCounter = 0;
        outletArr = new GameObject[5];

        // Desconectar todos los cables
        foreach (SimpleAttachWirePlug socket in socketArr)
        {
            if (socket != null)
                socket.detach();
        }

        Debug.LogWarning("Puzzle reseteado!");
    }
}
