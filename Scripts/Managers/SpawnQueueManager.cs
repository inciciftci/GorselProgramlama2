using UnityEngine;
using FruitMerge.Data;

namespace FruitMerge.Managers
{
    public class SpawnQueueManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FruitDatabaseSO fruitDatabase;

        [Header("Debug (Read-Only)")]
        [SerializeField, ReadOnly] private FruitTypeSO currentFruit;
        [SerializeField, ReadOnly] private FruitTypeSO nextFruit;

        public System.Action<FruitTypeSO> OnCurrentChanged;
        public System.Action<FruitTypeSO> OnNextChanged;

        public FruitTypeSO CurrentFruit => currentFruit;
        public FruitTypeSO NextFruit => nextFruit;

        private void Awake()
        {
            if (fruitDatabase != null)
            {
                fruitDatabase.Initialize();
            }
        }

        public void InitializeQueue()
        {
            if (fruitDatabase == null)
            {
                Debug.LogError("[SpawnQueueManager] FruitDatabase atanmamış!");
                return;
            }

            fruitDatabase.Initialize();

            if (fruitDatabase.GetAllTypes() == null || fruitDatabase.GetAllTypes().Count == 0)
            {
                Debug.LogError("[SpawnQueueManager] FruitDatabase Types listesi boş!");
                return;
            }

            currentFruit = fruitDatabase.GetRandomStartType();
            nextFruit = fruitDatabase.GetRandomStartType();

            if (currentFruit == null || nextFruit == null)
            {
                Debug.LogError($"[SpawnQueueManager] GetRandomStartType null döndü! Current: {currentFruit}, Next: {nextFruit}");
                Debug.LogError($"[SpawnQueueManager] FruitDatabase Types count: {fruitDatabase.GetAllTypes().Count}, Max Spawn ID: {fruitDatabase.GetMaxSpawnTypeID()}");

                var allTypes = fruitDatabase.GetAllTypes();
                foreach (var type in allTypes)
                {
                    if (type != null && type.id <= fruitDatabase.GetMaxSpawnTypeID())
                    {
                        if (currentFruit == null) currentFruit = type;
                        else if (nextFruit == null) nextFruit = type;
                        if (currentFruit != null && nextFruit != null) break;
                    }
                }
            }

            OnCurrentChanged?.Invoke(currentFruit);
            OnNextChanged?.Invoke(nextFruit);

            Debug.Log($"[SpawnQueueManager] Queue başlatıldı. Current: {currentFruit?.displayName ?? "NULL"}, Next: {nextFruit?.displayName ?? "NULL"}");
        }

        public void AdvanceQueue()
        {
            if (fruitDatabase == null)
            {
                Debug.LogError("[SpawnQueueManager] AdvanceQueue: FruitDatabase null!");
                return;
            }

            fruitDatabase.Initialize();

            if (nextFruit == null)
            {
                Debug.LogWarning("[SpawnQueueManager] NextFruit null! Yeni rastgele tip alınıyor...");
                nextFruit = fruitDatabase.GetRandomStartType();

                if (nextFruit == null)
                {
                    Debug.LogError("[SpawnQueueManager] GetRandomStartType null döndü! Fallback kullanılıyor...");
                    var allTypes = fruitDatabase.GetAllTypes();
                    foreach (var type in allTypes)
                    {
                        if (type != null && type.id <= fruitDatabase.GetMaxSpawnTypeID())
                        {
                            nextFruit = type;
                            break;
                        }
                    }
                }
            }

            currentFruit = nextFruit;
            nextFruit = fruitDatabase.GetRandomStartType();

            if (nextFruit == null)
            {
                Debug.LogWarning("[SpawnQueueManager] Yeni NextFruit null! Fallback kullanılıyor...");
                var allTypes = fruitDatabase.GetAllTypes();
                foreach (var type in allTypes)
                {
                    if (type != null && type.id <= fruitDatabase.GetMaxSpawnTypeID())
                    {
                        nextFruit = type;
                        break;
                    }
                }
            }

            if (currentFruit == null || nextFruit == null)
            {
                Debug.LogError($"[SpawnQueueManager] AdvanceQueue: Null değer! Current: {currentFruit}, Next: {nextFruit}");
                Debug.LogError($"[SpawnQueueManager] FruitDatabase Types count: {fruitDatabase.GetAllTypes().Count}, Max Spawn ID: {fruitDatabase.GetMaxSpawnTypeID()}");
            }

            OnCurrentChanged?.Invoke(currentFruit);
            OnNextChanged?.Invoke(nextFruit);

            Debug.Log($"[SpawnQueueManager] Queue ilerledi. Current: {currentFruit?.displayName ?? "NULL"}, Next: {nextFruit?.displayName ?? "NULL"}");
        }

        public void ResetQueue()
        {
            currentFruit = null;
            nextFruit = null;

            OnCurrentChanged?.Invoke(null);
            OnNextChanged?.Invoke(null);

            Debug.Log("[SpawnQueueManager] Queue resetlendi.");
        }

#if UNITY_EDITOR
        [System.Serializable]
        public class ReadOnlyAttribute : PropertyAttribute { }

        [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
        public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
        {
            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                GUI.enabled = false;
                UnityEditor.EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }
        }
#endif
    }
}
