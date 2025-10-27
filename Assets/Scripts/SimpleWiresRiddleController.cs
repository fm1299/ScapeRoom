using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller simplificado del puzzle de cables - Habilita el boton cuando se completa
/// </summary>
public class SimpleWiresRiddleController : MonoBehaviour
{
    [Header("Configuracion del Puzzle")]
    [SerializeField] private GameObject[] correctPlugArr; // Los 5 cables en el orden correcto
    [SerializeField] private SimpleAttachWirePlug[] socketArr;  // Los 5 sockets
    [SerializeField] private AudioClip wrongSound;        // Sonido de error

    [Header("Boton a Habilitar")]
    [SerializeField] private ButtonPushOpenDoor buttonController; // Referencia al boton

    private GameObject[] outletArr;                       // Cables actualmente conectados
    private int correctlyPluggedCounter = 0;              // Contador de cables correctos
    private bool puzzleCompleted = false;                 // Evita multiples victorias

    void Start()
    {
        outletArr = new GameObject[5];
    }

    /// <summary>
    /// Se llama cuando un cable se conecta
    /// </summary>
    public void OnWirePlugged(GameObject plug, int outletId)
    {
        if (puzzleCompleted) return; // Si ya se complet�, no hacer nada

        // Verificar si el cable es correcto para este socket
        if (correctPlugArr[outletId] == plug)
        {
            correctlyPluggedCounter++;
            Debug.Log($"Cable correcto conectado! ({correctlyPluggedCounter}/5)");
        }

        // Guardar el cable conectado
        outletArr[outletId] = plug;

        // Verificar si todos los cables est�n conectados
        if (AllWiresPlugged())
        {
            CheckSolution();
        }
    }

    /// <summary>
    /// Verifica si todos los cables est�n conectados
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
    /// Verifica si la soluci�n es correcta
    /// </summary>
    private void CheckSolution()
    {
        // Verificar si todos los cables est�n en la posici�n correcta
        for (int i = 0; i < 5; i++)
        {
            if (outletArr[i] != correctPlugArr[i])
            {
                // Soluci�n incorrecta - reproducir sonido de error
                AudioSource.PlayClipAtPoint(wrongSound, transform.position);
                Debug.Log("Soluci�n incorrecta! Algunos cables est�n mal conectados.");
                return;
            }
        }

        // �Soluci�n correcta!
        PuzzleCompleted();
    }

    /// <summary>
    /// Se ejecuta cuando el puzzle se completa correctamente
    /// </summary>
    private void PuzzleCompleted()
    {
        if (puzzleCompleted) return;

        puzzleCompleted = true;
        Debug.Log("PUZZLE COMPLETADO! GANASTE!");

        // Habilitar el boton para abrir la puerta
        if (buttonController != null)
        {
            buttonController.EnableButton();
            Debug.Log("Boton habilitado! Ahora puedes abrir la puerta.");
        }

        // Mostrar mensaje de victoria
        ShowWinMessage();
    }

    /// <summary>
    /// Muestra el mensaje de victoria
    /// </summary>
    private void ShowWinMessage()
    {
        // Tambien mostrar en consola
        Debug.LogError("FELICIDADES! PUZZLE COMPLETADO! GANASTE!");
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

        Debug.Log("Puzzle reseteado!");
    }
}