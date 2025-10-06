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
    
    [Header("Sistema de WIN")]
    [SerializeField] private GameObject winPanel;         // Panel que aparece al ganar
    [SerializeField] private Text winText;                // Texto de WIN
    [SerializeField] private AudioClip winSound;          // Sonido de victoria
    
    private GameObject[] outletArr;                       // Cables actualmente conectados
    private int correctlyPluggedCounter = 0;              // Contador de cables correctos
    private bool puzzleCompleted = false;                 // Evita múltiples victorias
    
    void Start()
    {
        outletArr = new GameObject[5];
        
        // Ocultar panel de victoria al inicio
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    /// <summary>
    /// Se llama cuando un cable se conecta
    /// </summary>
    public void OnWirePlugged(GameObject plug, int outletId)
    {
        if (puzzleCompleted) return; // Si ya se completó, no hacer nada
        
        // Verificar si el cable es correcto para este socket
        if (correctPlugArr[outletId] == plug)
        {
            correctlyPluggedCounter++;
            Debug.Log($"Cable correcto conectado! ({correctlyPluggedCounter}/5)");
        }
        
        // Guardar el cable conectado
        outletArr[outletId] = plug;
        
        // Verificar si todos los cables están conectados
        if (AllWiresPlugged())
        {
            CheckSolution();
        }
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
        // Verificar si todos los cables están en la posición correcta
        for (int i = 0; i < 5; i++)
        {
            if (outletArr[i] != correctPlugArr[i])
            {
                // Solución incorrecta - reproducir sonido de error
                AudioSource.PlayClipAtPoint(wrongSound, transform.position);
                Debug.Log("Solución incorrecta! Algunos cables están mal conectados.");
                return;
            }
        }
        
        // ¡Solución correcta!
        PuzzleCompleted();
    }

    /// <summary>
    /// Se ejecuta cuando el puzzle se completa correctamente
    /// </summary>
    private void PuzzleCompleted()
    {
        if (puzzleCompleted) return;
        
        puzzleCompleted = true;
        Debug.Log("¡PUZZLE COMPLETADO! ¡GANASTE!");
        
        // Mostrar mensaje de victoria
        ShowWinMessage();
        
        // Reproducir sonido de victoria
        if (winSound != null)
            AudioSource.PlayClipAtPoint(winSound, transform.position);
    }

    /// <summary>
    /// Muestra el mensaje de victoria
    /// </summary>
    private void ShowWinMessage()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (winText != null)
            {
                winText.text = "¡FELICIDADES!\n¡PUZZLE COMPLETADO!\n¡GANASTE!";
            }
        }
        
        // También mostrar en consola
        Debug.Log("🎉 ¡FELICIDADES! ¡PUZZLE COMPLETADO! ¡GANASTE! 🎉");
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
        
        // Ocultar panel de victoria
        if (winPanel != null)
            winPanel.SetActive(false);
        
        Debug.Log("Puzzle reseteado!");
    }
}
