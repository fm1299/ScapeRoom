using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup gameOverCanvas;
    [SerializeField] private Button retryButton;

    void Start()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.interactable = false;
            gameOverCanvas.blocksRaycasts = false;
        }

        if (retryButton != null)
            retryButton.onClick.AddListener(RestartLevel);
    }

    public void ShowGameOver()
    {
        if (gameOverCanvas == null) return;

        gameOverCanvas.alpha = 1f;
        gameOverCanvas.interactable = true;
        gameOverCanvas.blocksRaycasts = true;

        Debug.Log("Game Over screen shown");
    }

    private void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
