using UnityEngine;
using System.Collections.Generic;
using FruitMerge.Data;

namespace FruitMerge.Core
{
    public class MergeSystem : MonoBehaviour
    {
        public static MergeSystem Instance { get; private set; }

        [Header("References")]
        [SerializeField] private FruitDatabaseSO fruitDatabase;
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private FruitMerge.Managers.ScoreManager scoreManager;

        [Header("Settings")]
        [SerializeField] private float mergeDelay = 0.05f;
        [SerializeField] private Vector2 spawnOffset = new Vector2(0, 0.3f);

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private struct MergePair
        {
            public Fruit fruitA;
            public Fruit fruitB;
            public Vector2 contactPoint;

            public MergePair(Fruit a, Fruit b, Vector2 contact)
            {
                fruitA = a;
                fruitB = b;
                contactPoint = contact;
            }
        }

        private Queue<MergePair> mergeQueue = new Queue<MergePair>();
        private float mergeTimer = 0f;

        public System.Action<FruitTypeSO, Vector2> OnMergeCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[MergeSystem] Birden fazla instance tespit edildi! İlki kullanılacak.");
                Destroy(gameObject);
                return;
            }

            if (fruitDatabase != null)
            {
                fruitDatabase.Initialize();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
        }

        public void OnFruitCollision(Fruit fruitA, Fruit fruitB, Vector2 contactPoint)
        {
            if (fruitA == null || fruitB == null)
            {
                Debug.LogWarning("[MergeSystem] OnFruitCollision: fruitA veya fruitB null!");
                return;
            }
            
            if (fruitA.IsLockedForMerge || fruitB.IsLockedForMerge)
            {
                if (debugMode)
                {
                    Debug.Log($"[MergeSystem] OnFruitCollision: Bir meyve zaten kilitli. A: {fruitA.IsLockedForMerge}, B: {fruitB.IsLockedForMerge}");
                }
                return;
            }
            
            if (fruitA.FruitType == null || fruitB.FruitType == null)
            {
                Debug.LogWarning("[MergeSystem] OnFruitCollision: FruitType null!");
                return;
            }

            int idA = fruitA.FruitType.id;
            int idB = fruitB.FruitType.id;
            
            Debug.Log($"[MergeSystem] OnFruitCollision çağrıldı: {fruitA.FruitType.displayName} (ID:{idA}) vs {fruitB.FruitType.displayName} (ID:{idB})");
            
            if (idA == idB)
            {
                mergeQueue.Enqueue(new MergePair(fruitA, fruitB, contactPoint));

                Debug.Log($"[MergeSystem] ✅ Merge pair queue'ya eklendi: {fruitA.FruitType.displayName} (ID:{idA}) <-> {fruitB.FruitType.displayName} (ID:{idB})");
                Debug.Log($"[MergeSystem] Queue size: {mergeQueue.Count}");
            }
            else
            {
                Debug.Log($"[MergeSystem] ❌ Farklı tip, merge yok: {fruitA.FruitType.displayName} (ID:{idA}) vs {fruitB.FruitType.displayName} (ID:{idB})");
            }
        }

        private void FixedUpdate()
        {
            if (mergeQueue.Count > 0)
            {
                mergeTimer += Time.fixedDeltaTime;

                if (mergeTimer >= mergeDelay)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[MergeSystem] FixedUpdate: Processing {mergeQueue.Count} merge pairs");
                    }
                    ProcessMergeQueue();
                    mergeTimer = 0f;
                }
            }
        }

        private void ProcessMergeQueue()
        {
            int processedCount = 0;

            while (mergeQueue.Count > 0)
            {
                MergePair pair = mergeQueue.Dequeue();

                if (!ValidateMergePair(pair))
                {
                    continue;
                }

                pair.fruitA.LockForMerge();
                pair.fruitB.LockForMerge();

                PerformMerge(pair);

                processedCount++;
            }

            if (debugMode && processedCount > 0)
            {
                Debug.Log($"[MergeSystem] {processedCount} merge işlendi.");
            }
        }

        private bool ValidateMergePair(MergePair pair)
        {
            if (pair.fruitA == null || pair.fruitB == null) return false;
            if (!pair.fruitA.gameObject.activeInHierarchy || !pair.fruitB.gameObject.activeInHierarchy) return false;

            if (pair.fruitA.IsLockedForMerge || pair.fruitB.IsLockedForMerge) return false;

            if (pair.fruitA.FruitType.id != pair.fruitB.FruitType.id) return false;

            return true;
        }

        private void PerformMerge(MergePair pair)
        {
            FruitTypeSO currentType = pair.fruitA.FruitType;
            FruitTypeSO nextType = fruitDatabase.GetNextType(currentType);

            if (nextType == null)
            {
                Debug.Log($"[MergeSystem] {currentType.displayName} maksimum level, merge yok.");
                return;
            }

            Vector2 spawnPosition = (pair.fruitA.transform.position + pair.fruitB.transform.position) / 2f;
            spawnPosition += spawnOffset;

            Vector2 averageVelocity = (pair.fruitA.Rigidbody.velocity + pair.fruitB.Rigidbody.velocity) / 2f;

            poolManager.Return(pair.fruitA);
            poolManager.Return(pair.fruitB);

            Fruit newFruit = poolManager.Get(nextType, spawnPosition, isKinematic: false);

            if (newFruit != null)
            {
                newFruit.Rigidbody.velocity = averageVelocity * 0.5f;

                if (scoreManager != null)
                {
                    scoreManager.AddScore(nextType.scoreValue);
                    Debug.Log($"[MergeSystem] Skor eklendi: +{nextType.scoreValue} puan (Toplam: {scoreManager.CurrentScore})");
                }
                else
                {
                    Debug.LogWarning("[MergeSystem] ScoreManager null! Inspector'da Score Manager alanını kontrol edin. Skor eklenemedi.");
                }

                OnMergeCompleted?.Invoke(nextType, spawnPosition);

                Debug.Log($"[MergeSystem] ✅ MERGE TAMAMLANDI: {currentType.displayName} -> {nextType.displayName} at {spawnPosition} (+{nextType.scoreValue} puan)");
            }
            else
            {
                Debug.LogError($"[MergeSystem] ❌ Yeni meyve spawn edilemedi: {nextType.displayName}. PoolManager null mu?");
            }
        }

        public void ClearQueue()
        {
            mergeQueue.Clear();
            mergeTimer = 0f;

            if (debugMode)
            {
                Debug.Log("[MergeSystem] Merge queue temizlendi.");
            }
        }
    }
}
