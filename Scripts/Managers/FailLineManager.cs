using UnityEngine;
using System.Collections.Generic;
using FruitMerge.Core;

namespace FruitMerge.Managers
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FailLineManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float graceTime = 1.0f;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private Dictionary<Fruit, float> fruitsInZone = new Dictionary<Fruit, float>();

        public System.Action OnGameOver;

        private BoxCollider2D trigger;

        private void Awake()
        {
            trigger = GetComponent<BoxCollider2D>();

            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
            else
            {
                Debug.LogError("[FailLineManager] BoxCollider2D bulunamadı!");
            }

            gameObject.layer = LayerMask.NameToLayer("FailLine");
        }

        private void Update()
        {
            List<Fruit> toRemove = new List<Fruit>();

            var fruitsCopy = new Dictionary<Fruit, float>(fruitsInZone);

            foreach (var kvp in fruitsCopy)
            {
                Fruit fruit = kvp.Key;
                
                if (fruit == null || !fruit.gameObject.activeInHierarchy)
                {
                    toRemove.Add(fruit);
                    continue;
                }

                float timeInZone = kvp.Value + Time.deltaTime;

                if (fruitsInZone.ContainsKey(fruit))
                {
                    fruitsInZone[fruit] = timeInZone;
                }

                if (timeInZone >= graceTime)
                {
                    TriggerGameOver(fruit);
                    return;
                }
            }

            foreach (var fruit in toRemove)
            {
                if (fruitsInZone.ContainsKey(fruit))
                {
                    fruitsInZone.Remove(fruit);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Fruit fruit = other.GetComponent<Fruit>();

            if (fruit != null && !fruit.IsLockedForMerge)
            {
                if (!fruitsInZone.ContainsKey(fruit))
                {
                    fruitsInZone[fruit] = 0f;

                    if (debugMode)
                    {
                        Debug.Log($"[FailLineManager] {fruit.FruitType.displayName} fail line'a girdi. Grace timer başladı.");
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Fruit fruit = other.GetComponent<Fruit>();

            if (fruit != null && fruitsInZone.ContainsKey(fruit))
            {
                fruitsInZone.Remove(fruit);

                if (debugMode)
                {
                    Debug.Log($"[FailLineManager] {fruit.FruitType.displayName} fail line'dan çıktı. Timer resetlendi.");
                }
            }
        }

        private void TriggerGameOver(Fruit fruit)
        {
            if (debugMode)
            {
                Debug.Log($"[FailLineManager] GAME OVER! {fruit.FruitType.displayName} çok uzun süre fail line'da kaldı.");
            }

            OnGameOver?.Invoke();

            fruitsInZone.Clear();
        }

        public void ResetFailLine()
        {
            fruitsInZone.Clear();

            if (debugMode)
            {
                Debug.Log("[FailLineManager] Fail line resetlendi.");
            }
        }

        private void OnDrawGizmos()
        {
            if (trigger == null) return;

            Gizmos.color = fruitsInZone.Count > 0 ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(transform.position, trigger.size);
        }
    }
}
