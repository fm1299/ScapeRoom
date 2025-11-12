using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller simplificado del puzzle de cables - con penalización por errores
/// </summary>
public class SimpleWiresRiddleController : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] public GameObject[] screens;
    [SerializeField] private GameObject[] correctPlugArr; // Los 5 cables en el orden correcto
    [SerializeField] private SimpleAttachWirePlug[] socketArr;  // Los 5 sockets

    [Header("Audio Settings")]
    [SerializeField] private AudioClip wrongSound;        // Sonido al conectar mal
    [SerializeField] private AudioClip winSound;          // Sonido al completar puzzle

    [Header("Button / Door Configuration")]
    [SerializeField] private ButtonPushOpenDoor buttonController;
    [SerializeField] private Animator animator;
    [SerializeField] private string boolName = "Open";

    [Header("Timer Reference")]
    [Tooltip("Referencia al script de luz de emergencia que maneja el tiempo del juego")]
    [SerializeField] private EmergencyLight emergencyLight;

    [Tooltip("Cantidad de segundos que se restan al tiempo por cada error")]
    [SerializeField] private float timePenalty = 5f;

    private GameObject[] outletArr;  // Cables actualmente conectados
    private int correctlyPluggedCounter = 0;
    private bool puzzleCompleted = false;

    void Start()
    {
        outletArr = new GameObject[5];
        foreach (GameObject screen in screens)
        {
            screen.SetActive(false);
        }

        //// Subscribe to time over event
        //if (emergencyLight != null)
        //    emergencyLight.OnTimeOver += HandleGameOver;
    }

    /// <summary>
    /// Turn on the screen with the solution of the wires placement
    /// </summary>
    public void turnOnScreen()
    {
        foreach (GameObject screen in screens)
        {
            screen.SetActive(true);
            GameManager.Instance.UpdateGameState(RiddlesProgress.PowerPlugged);
        }
    }

    /// <summary>
    /// Se llama cuando un cable se conecta
    /// </summary>
    public void OnWirePlugged(GameObject plug, int outletId)
    {
        if (puzzleCompleted)
        {
            Debug.LogWarning("Puzzle ya completado - ignorando");
            return;
        }

        bool isCorrect = correctPlugArr[outletId] == plug;
        outletArr[outletId] = plug;

        // Buscar el componente CableComponent desde el padre (Cable_start)
        CableComponent cable = plug.GetComponent<CableComponent>();
        if (cable == null && plug.transform.parent != null)
            cable = plug.transform.parent.GetComponentInChildren<CableComponent>();

        // Cambiar color del cable
        if (cable != null)
        {
            cable.SetCableColor(isCorrect ? Color.green : Color.red);
        }

        // Reacciones según sea correcto o incorrecto
        if (isCorrect)
        {
            correctlyPluggedCounter++;
            Debug.Log($"Cable {plug.name} correcto ({correctlyPluggedCounter}/5)");
        }
        else
        {
            Debug.LogWarning($"Cable {plug.name} incorrecto");

            // Reproducir sonido de error
            if (wrongSound != null)
                AudioSource.PlayClipAtPoint(wrongSound, transform.position);

            // Aplicar penalización de tiempo
            if (emergencyLight != null)
            {
                float remaining = emergencyLight.GetRemainingTime();
                float newTime = Mathf.Max(remaining - timePenalty, 0f);
                emergencyLight.ResetTimer(newTime);
                Debug.LogWarning($"Tiempo penalizado: -{timePenalty}s (restante: {newTime:F1}s)");
            }
        }

        if (AllWiresPlugged())
        {
            Debug.LogWarning("Todos los cables conectados. Verificando solución...");
            CheckSolution();
        }
    }

    private bool AllWiresPlugged()
    {
        for (int i = 0; i < outletArr.Length; i++)
        {
            if (outletArr[i] == null)
                return false;
        }
        return true;
    }

    private void CheckSolution()
    {
        for (int i = 0; i < correctPlugArr.Length; i++)
        {
            if (outletArr[i] != correctPlugArr[i])
            {
                Debug.LogWarning("Solución incorrecta. Algunos cables están mal conectados.");

                // Ya reproducimos el sonido y penalizamos al conectar mal, así que solo salimos
                return;
            }
        }

        // Si todos coinciden → Victoria
        PuzzleCompleted();
    }

    private void PuzzleCompleted()
    {
        if (puzzleCompleted) return;
        puzzleCompleted = true;

        Debug.Log("Puzzle completado correctamente!");

        // Sonido de victoria
        if (winSound != null)
            AudioSource.PlayClipAtPoint(winSound, transform.position);

        // Animación de puerta
        if (animator != null)
        {
            bool wasOpen = animator.GetBool(boolName);
            animator.SetBool(boolName, !wasOpen);
            Debug.Log("Puerta abierta automáticamente.");
        }

        // Habilitar botón (opcional)
        if (buttonController != null)
            buttonController.EnableButton();
    }

    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        correctlyPluggedCounter = 0;
        outletArr = new GameObject[5];

        // Desconectar todos los sockets
        foreach (var socket in socketArr)
        {
            if (socket != null)
                socket.Detach();
        }

        // Restaurar color de los cables
        foreach (var plug in correctPlugArr)
        {
            if (plug != null)
            {
                CableComponent cable = plug.GetComponent<CableComponent>();
                if (cable == null && plug.transform.parent != null)
                    cable = plug.transform.parent.GetComponentInChildren<CableComponent>();

                if (cable != null)
                    cable.SetCableColor(Color.white);
            }
        }

        Debug.Log("Puzzle reseteado y colores restaurados.");
    }

    //private void HandleGameOver()
    //{
    //    Debug.LogWarning("Time’s up! Game Over.");
    //    puzzleCompleted = true;

    //    if (gameOverUI != null)
    //        gameOverUI.ShowGameOver();
    //}
}
