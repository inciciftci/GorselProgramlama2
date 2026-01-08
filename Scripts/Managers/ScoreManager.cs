using UnityEngine;

namespace FruitMerge.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int highScore = 0;

        [Header("Settings")]
        [SerializeField] private string highScoreKey = "HighScore";

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnHighScoreChanged;

        public int CurrentScore => currentScore;
        public int HighScore => highScore;

        private void Start()
        {
            LoadHighScore();
        }

        public void AddScore(int points)
        {
            if (points <= 0) return;

            currentScore += points;
            
            if (OnScoreChanged != null)
            {
                OnScoreChanged.Invoke(currentScore);
                Debug.Log($"[ScoreManager] OnScoreChanged event tetiklendi. Yeni skor: {currentScore} (Subscriber sayısı: {OnScoreChanged.GetInvocationList().Length})");
            }
            else
            {
                Debug.LogWarning("[ScoreManager] OnScoreChanged event null! UIHud subscribe olmamış olabilir.");
            }

            if (debugMode)
            {
                Debug.Log($"[ScoreManager] +{points} puan. Toplam: {currentScore}");
            }

            if (currentScore > highScore)
            {
                highScore = currentScore;
                OnHighScoreChanged?.Invoke(highScore);
                SaveHighScore();

                if (debugMode)
                {
                    Debug.Log($"[ScoreManager] Yeni high score: {highScore}");
                }
            }
        }

        public void ResetScore()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);

            if (debugMode)
            {
                Debug.Log("[ScoreManager] Skor resetlendi.");
            }
        }

        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt(highScoreKey, 0);
            OnHighScoreChanged?.Invoke(highScore);

            if (debugMode)
            {
                Debug.Log($"[ScoreManager] High score yüklendi: {highScore}");
            }
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(highScoreKey, highScore);
            PlayerPrefs.Save();

            if (debugMode)
            {
                Debug.Log($"[ScoreManager] High score kaydedildi: {highScore}");
            }
        }

        public void ClearHighScore()
        {
            highScore = 0;
            PlayerPrefs.DeleteKey(highScoreKey);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(highScore);

            if (debugMode)
            {
                Debug.Log("[ScoreManager] High score silindi.");
            }
        }
    }
}
