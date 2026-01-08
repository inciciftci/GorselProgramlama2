using UnityEngine;
using FruitMerge.Core;
using FruitMerge.Data;
using FruitMerge.UI;

namespace FruitMerge.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FruitDatabaseSO fruitDatabase;
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private MergeSystem mergeSystem;
        [SerializeField] private SpawnQueueManager spawnQueueManager;
        [SerializeField] private FruitSpawner fruitSpawner;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private FailLineManager failLineManager;
        [SerializeField] private UIHud uiHud;

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        public enum GameState
        {
            MainMenu,
            Playing,
            GameOver,
            Paused
        }

        public System.Action<GameState> OnGameStateChanged;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            if (fruitDatabase != null)
            {
                fruitDatabase.Initialize();
            }

            if (mergeSystem != null)
            {
                mergeSystem.OnMergeCompleted += HandleMergeCompleted;
            }

            if (failLineManager != null)
            {
                failLineManager.OnGameOver += HandleGameOver;
            }
        }

        private void Start()
        {
            WarmupPools();

            StartGame();
        }

        private void WarmupPools()
        {
            if (fruitDatabase == null || poolManager == null) return;

            foreach (var fruitType in fruitDatabase.GetAllTypes())
            {
                if (fruitType != null)
                {
                    poolManager.WarmupPool(fruitType, 5);
                }
            }

            if (debugMode)
            {
                Debug.Log("[GameManager] Pool warmup tamamlandı.");
            }
        }

        public void StartGame()
        {
            if (debugMode)
            {
                Debug.Log("[GameManager] Oyun başlatılıyor...");
            }

            ChangeState(GameState.Playing);

            if (spawnQueueManager != null)
            {
                spawnQueueManager.InitializeQueue();
            }

            if (fruitSpawner != null)
            {
                fruitSpawner.AllowInput = true;
                fruitSpawner.StartSpawning();
            }

            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }

            if (failLineManager != null)
            {
                failLineManager.ResetFailLine();
            }
        }

        public void RestartGame()
        {
            if (debugMode)
            {
                Debug.Log("[GameManager] Oyun yeniden başlatılıyor...");
            }

            if (poolManager != null)
            {
                poolManager.ReturnAll();
            }

            Fruit[] allFruitsInScene = FindObjectsOfType<Fruit>();
            foreach (Fruit fruit in allFruitsInScene)
            {
                if (fruit != null && fruit.gameObject.activeInHierarchy)
                {
                    if (poolManager != null)
                    {
                        poolManager.Return(fruit);
                    }
                    else
                    {
                        Destroy(fruit.gameObject);
                    }
                }
            }

            if (debugMode && allFruitsInScene.Length > 0)
            {
                Debug.Log($"[GameManager] Sahne içinde {allFruitsInScene.Length} meyve bulundu ve temizlendi.");
            }

            if (mergeSystem != null)
            {
                mergeSystem.ClearQueue();
            }

            if (fruitSpawner != null)
            {
                fruitSpawner.ResetSpawner();
            }

            if (spawnQueueManager != null)
            {
                spawnQueueManager.ResetQueue();
            }

            if (scoreManager != null)
            {
                scoreManager.ResetScore();
            }

            if (failLineManager != null)
            {
                failLineManager.ResetFailLine();
            }

            Invoke(nameof(StartGame), 0.2f);
        }

        private void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            if (debugMode)
            {
                Debug.Log($"[GameManager] State değişti: {newState}");
            }

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    if (fruitSpawner != null) fruitSpawner.AllowInput = true;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 1f;
                    if (fruitSpawner != null) fruitSpawner.AllowInput = false;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    if (fruitSpawner != null) fruitSpawner.AllowInput = false;
                    break;

                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    if (fruitSpawner != null) fruitSpawner.AllowInput = false;
                    break;
            }
        }

        private void HandleMergeCompleted(FruitTypeSO newFruitType, Vector2 position)
        {
            if (currentState != GameState.Playing) return;

            if (scoreManager != null && newFruitType != null)
            {
                scoreManager.AddScore(newFruitType.scoreValue);
            }
        }

        private void HandleGameOver()
        {
            if (currentState != GameState.Playing) return;

            ChangeState(GameState.GameOver);

            if (debugMode)
            {
                Debug.Log("[GameManager] GAME OVER!");
            }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        private void OnDestroy()
        {
            if (mergeSystem != null)
            {
                mergeSystem.OnMergeCompleted -= HandleMergeCompleted;
            }

            if (failLineManager != null)
            {
                failLineManager.OnGameOver -= HandleGameOver;
            }
        }

        public void QuitGame()
        {
            if (debugMode)
            {
                Debug.Log("[GameManager] Oyundan çıkılıyor...");
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
