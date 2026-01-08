using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FruitMerge.Managers;
using FruitMerge.Data;

namespace FruitMerge.UI
{
    public class UIHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private SpawnQueueManager spawnQueueManager;
        [SerializeField] private GameManager gameManager;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Image nextFruitImage;
        [SerializeField] private Button restartButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button gameOverRestartButton;

        [Header("Settings")]
        [SerializeField] private string scoreFormat = "Score: {0}";
        [SerializeField] private string highScoreFormat = "Best: {0}";

        private void OnEnable()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScoreText;
                scoreManager.OnHighScoreChanged += UpdateHighScoreText;
                Debug.Log("[UIHud] ScoreManager event'lerine subscribe olundu.");
            }
            else
            {
                Debug.LogWarning("[UIHud] ScoreManager null! Inspector'da Score Manager alanını kontrol edin.");
            }

            if (spawnQueueManager != null)
            {
                spawnQueueManager.OnNextChanged += UpdateNextFruitPreview;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= UpdateScoreText;
                scoreManager.OnHighScoreChanged -= UpdateHighScoreText;
            }

            if (spawnQueueManager != null)
            {
                spawnQueueManager.OnNextChanged -= UpdateNextFruitPreview;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }
        }

        private void Start()
        {
            if (scoreManager != null)
            {
                UpdateScoreText(scoreManager.CurrentScore);
                UpdateHighScoreText(scoreManager.HighScore);
            }
            else
            {
                Debug.LogWarning("[UIHud] Start: ScoreManager null! Inspector'da Score Manager alanını kontrol edin.");
            }

            if (scoreText == null)
            {
                Debug.LogWarning("[UIHud] Start: ScoreText null! Inspector'da Score Text alanını kontrol edin.");
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void UpdateScoreText(int score)
        {
            Debug.Log($"[UIHud] UpdateScoreText çağrıldı. Skor: {score}");
            
            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, score);
                Debug.Log($"[UIHud] ScoreText güncellendi: {scoreText.text}");
            }
            else
            {
                Debug.LogWarning("[UIHud] ScoreText null! Inspector'da Score Text alanını kontrol edin.");
            }
        }

        private void UpdateHighScoreText(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = string.Format(highScoreFormat, highScore);
            }
        }

        private void UpdateNextFruitPreview(FruitTypeSO nextFruit)
        {
            if (nextFruitImage != null)
            {
                if (nextFruit != null && nextFruit.sprite != null)
                {
                    nextFruitImage.sprite = nextFruit.sprite;
                    nextFruitImage.enabled = true;
                }
                else
                {
                    nextFruitImage.enabled = false;
                }
            }
        }

        private void OnRestartButtonClicked()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }

        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.GameOver)
            {
                ShowGameOverPanel();
            }
            else if (newState == GameManager.GameState.Playing)
            {
                HideGameOverPanel();
            }
        }

        private void HideGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void ShowGameOverPanel()
        {
            Debug.Log("[UIHud] ShowGameOverPanel çağrıldı!");

            if (gameOverPanel == null)
            {
                Debug.LogError("[UIHud] Game Over Panel null! Inspector'da Game Over Panel alanını kontrol edin.");
                return;
            }

            gameOverPanel.SetActive(true);

            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            Canvas parentCanvas = gameOverPanel.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                parentCanvas.gameObject.SetActive(true);
                
                if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogWarning($"[UIHud] Canvas Render Mode {parentCanvas.renderMode} olarak ayarlı! Screen Space - Overlay'e değiştiriliyor...");
                    parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                
                Debug.Log($"[UIHud] Parent Canvas bulundu: {parentCanvas.name}, Render Mode: {parentCanvas.renderMode}");
            }
            else
            {
                Debug.LogWarning("[UIHud] Parent Canvas bulunamadı! Panel Canvas içinde mi?");
            }

            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = "GAME OVER!";
                gameOverTitleText.gameObject.SetActive(true);
                Debug.Log("[UIHud] Game Over Title Text güncellendi: GAME OVER!");
            }
            else
            {
                Debug.LogWarning("[UIHud] Game Over Title Text null! Inspector'da kontrol edin.");
            }

            if (finalScoreText != null && scoreManager != null)
            {
                finalScoreText.text = $"Score: {scoreManager.CurrentScore}";
                finalScoreText.gameObject.SetActive(true);
                Debug.Log($"[UIHud] Final Score Text güncellendi: Score: {scoreManager.CurrentScore}");
            }
            else
            {
                if (finalScoreText == null)
                    Debug.LogWarning("[UIHud] Final Score Text null! Inspector'da kontrol edin.");
                if (scoreManager == null)
                    Debug.LogWarning("[UIHud] ScoreManager null! Inspector'da kontrol edin.");
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.gameObject.SetActive(true);
                gameOverRestartButton.interactable = true;
                Debug.Log("[UIHud] Game Over Restart Button aktif edildi.");
            }
            else
            {
                Debug.LogWarning("[UIHud] Game Over Restart Button null! Inspector'da kontrol edin.");
            }

            Debug.Log("[UIHud] Game Over Panel gösterildi!");
        }

        [ContextMenu("Test Update Next Preview")]
        private void TestUpdateNextPreview()
        {
            if (spawnQueueManager != null && spawnQueueManager.NextFruit != null)
            {
                UpdateNextFruitPreview(spawnQueueManager.NextFruit);
            }
        }
    }
}
