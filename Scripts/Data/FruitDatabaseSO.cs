using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FruitMerge.Data
{
    [CreateAssetMenu(fileName = "FruitDatabase", menuName = "FruitMerge/FruitDatabase")]
    public class FruitDatabaseSO : ScriptableObject
    {
        [Header("Fruit Types")]
        [Tooltip("Tüm meyve tipleri (ID sırasına göre)")]
        public List<FruitTypeSO> types = new List<FruitTypeSO>();

        [Header("Spawn Settings")]
        [Tooltip("Spawn edilebilecek maksimum meyve ID'si (0-tabanlı). Örnek: 2 = sadece 0,1,2 spawn olur")]
        [SerializeField] private int maxSpawnTypeID = 2;

        private Dictionary<int, FruitTypeSO> idLookup;

        public void Initialize()
        {
            idLookup = new Dictionary<int, FruitTypeSO>();
            foreach (var fruitType in types)
            {
                if (fruitType != null)
                {
                    idLookup[fruitType.id] = fruitType;
                }
            }

            if (types.Count == 0)
            {
                Debug.LogError("[FruitDatabase] Types listesi boş! ScriptableObject'leri ekleyin.");
            }

            for (int i = 0; i < types.Count; i++)
            {
                if (types[i] != null && types[i].id != i)
                {
                    Debug.LogWarning($"[FruitDatabase] {types[i].displayName} ID'si ({types[i].id}) index'i ({i}) ile uyuşmuyor!");
                }
            }
        }

        public FruitTypeSO GetTypeByID(int id)
        {
            if (idLookup == null || idLookup.Count == 0)
            {
                Initialize();
            }

            if (idLookup.TryGetValue(id, out FruitTypeSO fruitType))
            {
                return fruitType;
            }

            Debug.LogError($"[FruitDatabase] ID {id} için meyve tipi bulunamadı!");
            return null;
        }

        public FruitTypeSO GetNextType(FruitTypeSO currentType)
        {
            if (currentType == null)
            {
                Debug.LogError("[FruitDatabase] GetNextType: currentType null!");
                return null;
            }

            return currentType.nextType;
        }

        public FruitTypeSO GetRandomStartType()
        {
            if (types == null || types.Count == 0)
            {
                Debug.LogError("[FruitDatabase] Types listesi boş!");
                return null;
            }

            if (idLookup == null || idLookup.Count == 0)
            {
                Initialize();
            }

            List<FruitTypeSO> validTypes = new List<FruitTypeSO>();
            for (int i = 0; i <= maxSpawnTypeID && i < types.Count; i++)
            {
                if (types[i] != null)
                {
                    validTypes.Add(types[i]);
                }
            }

            if (validTypes.Count == 0)
            {
                Debug.LogError($"[FruitDatabase] GetRandomStartType: Geçerli tip bulunamadı! Types count: {types.Count}, Max Spawn ID: {maxSpawnTypeID}");
                foreach (var type in types)
                {
                    if (type != null)
                    {
                        Debug.LogWarning($"[FruitDatabase] Fallback: {type.displayName} döndürülüyor");
                        return type;
                    }
                }
                return null;
            }

            int randomIndex = Random.Range(0, validTypes.Count);
            FruitTypeSO result = validTypes[randomIndex];

            if (result == null)
            {
                Debug.LogError($"[FruitDatabase] GetRandomStartType: Seçilen tip null! Index: {randomIndex}, Valid types count: {validTypes.Count}");
            }
            else
            {
                Debug.Log($"[FruitDatabase] GetRandomStartType: {result.displayName} (ID:{result.id}) seçildi");
            }

            return result;
        }

        public FruitTypeSO GetRandomTypeInRange(int minID, int maxID)
        {
            minID = Mathf.Clamp(minID, 0, types.Count - 1);
            maxID = Mathf.Clamp(maxID, 0, types.Count - 1);

            int randomID = Random.Range(minID, maxID + 1);
            return GetTypeByID(randomID);
        }

        public List<FruitTypeSO> GetAllTypes()
        {
            return types;
        }

        public int GetMaxSpawnTypeID()
        {
            return maxSpawnTypeID;
        }

        private void OnValidate()
        {
            foreach (var fruit in types)
            {
                if (fruit != null && fruit.nextType != null)
                {
                    if (fruit.nextType.id <= fruit.id)
                    {
                        Debug.LogWarning($"[FruitDatabase] {fruit.displayName} nextType chain'i yanlış! nextType ID daha küçük veya eşit.");
                    }
                }
            }
        }
    }
}
