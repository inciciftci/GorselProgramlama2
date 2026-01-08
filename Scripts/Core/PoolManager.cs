using UnityEngine;
using System.Collections.Generic;
using FruitMerge.Data;

namespace FruitMerge.Core
{
    public class PoolManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int initialPoolSizePerType = 10;
        [SerializeField] private Transform poolParent;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private Dictionary<int, Queue<Fruit>> pools = new Dictionary<int, Queue<Fruit>>();
        private Dictionary<int, List<Fruit>> activeObjects = new Dictionary<int, List<Fruit>>();

        private void Awake()
        {
            if (poolParent == null)
            {
                poolParent = new GameObject("PoolParent").transform;
                poolParent.SetParent(transform);
            }
        }

        public void WarmupPool(FruitTypeSO fruitType, int count)
        {
            if (fruitType == null || fruitType.prefab == null)
            {
                Debug.LogError($"[PoolManager] WarmupPool: FruitType veya prefab null!");
                return;
            }

            if (!pools.ContainsKey(fruitType.id))
            {
                pools[fruitType.id] = new Queue<Fruit>();
                activeObjects[fruitType.id] = new List<Fruit>();
            }

            for (int i = 0; i < count; i++)
            {
                CreateNewInstance(fruitType);
            }

            if (debugMode)
            {
                Debug.Log($"[PoolManager] {fruitType.displayName} için {count} instance warmup yapıldı.");
            }
        }

        public Fruit Get(FruitTypeSO fruitType, Vector2 position, bool isKinematic = false)
        {
            if (fruitType == null)
            {
                Debug.LogError("[PoolManager] Get: FruitType null!");
                return null;
            }

            if (!pools.ContainsKey(fruitType.id))
            {
                WarmupPool(fruitType, initialPoolSizePerType);
            }

            Fruit fruit = null;

            if (pools[fruitType.id].Count > 0)
            {
                fruit = pools[fruitType.id].Dequeue();
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[PoolManager] {fruitType.displayName} pool'u boş! Runtime'da yeni instance oluşturuluyor.");
                }
                fruit = CreateNewInstance(fruitType);
            }

            if (fruit != null)
            {
                fruit.Initialize(fruitType, position, isKinematic);
                activeObjects[fruitType.id].Add(fruit);

                if (debugMode)
                {
                    Debug.Log($"[PoolManager] {fruitType.displayName} pool'dan alındı. Aktif: {activeObjects[fruitType.id].Count}, Pool: {pools[fruitType.id].Count}");
                }
            }

            return fruit;
        }

        public void Return(Fruit fruit)
        {
            if (fruit == null)
            {
                Debug.LogWarning("[PoolManager] Return: Fruit null!");
                return;
            }

            FruitTypeSO fruitType = fruit.FruitType;
            if (fruitType == null)
            {
                Debug.LogError("[PoolManager] Return: Fruit'un FruitType'ı null!");
                Destroy(fruit.gameObject);
                return;
            }

            if (activeObjects.ContainsKey(fruitType.id))
            {
                activeObjects[fruitType.id].Remove(fruit);
            }

            fruit.ResetForPool();
            fruit.transform.SetParent(poolParent);
            pools[fruitType.id].Enqueue(fruit);

            if (debugMode)
            {
                Debug.Log($"[PoolManager] {fruitType.displayName} pool'a döndürüldü. Aktif: {activeObjects[fruitType.id].Count}, Pool: {pools[fruitType.id].Count}");
            }
        }

        public void ReturnAll()
        {
            List<Fruit> allActive = new List<Fruit>();

            foreach (var list in activeObjects.Values)
            {
                allActive.AddRange(list);
            }

            foreach (var fruit in allActive)
            {
                Return(fruit);
            }

            if (debugMode)
            {
                Debug.Log($"[PoolManager] Tüm aktif objeler pool'a döndürüldü. Toplam: {allActive.Count}");
            }
        }

        private Fruit CreateNewInstance(FruitTypeSO fruitType)
        {
            GameObject instance = Instantiate(fruitType.prefab, poolParent);
            instance.SetActive(false);

            Fruit fruit = instance.GetComponent<Fruit>();
            if (fruit == null)
            {
                Debug.LogError($"[PoolManager] {fruitType.displayName} prefab'ında Fruit component yok!");
                Destroy(instance);
                return null;
            }

            instance.layer = LayerMask.NameToLayer("Fruit");

            if (!pools.ContainsKey(fruitType.id))
            {
                pools[fruitType.id] = new Queue<Fruit>();
                activeObjects[fruitType.id] = new List<Fruit>();
            }

            pools[fruitType.id].Enqueue(fruit);

            return fruit;
        }

        public int GetActiveCount(int fruitTypeID)
        {
            if (activeObjects.ContainsKey(fruitTypeID))
            {
                return activeObjects[fruitTypeID].Count;
            }
            return 0;
        }

        public int GetPoolCount(int fruitTypeID)
        {
            if (pools.ContainsKey(fruitTypeID))
            {
                return pools[fruitTypeID].Count;
            }
            return 0;
        }
    }
}
