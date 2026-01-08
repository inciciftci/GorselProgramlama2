using UnityEngine;

namespace FruitMerge.Data
{
    [CreateAssetMenu(fileName = "NewFruitType", menuName = "FruitMerge/FruitType")]
    public class FruitTypeSO : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Benzersiz meyve ID'si (0'dan başlar)")]
        public int id;

        [Tooltip("Görünen isim")]
        public string displayName;

        [Header("Visual")]
        [Tooltip("Meyve sprite'ı")]
        public Sprite sprite;

        [Tooltip("Meyve prefab'ı (Fruit component içermeli)")]
        public GameObject prefab;

        [Header("Merge Settings")]
        [Tooltip("Bu meyve merge olduğunda oluşacak bir sonraki tip (null = maksimum level)")]
        public FruitTypeSO nextType;

        [Header("Game Values")]
        [Tooltip("Bu meyve oluştuğunda verilecek skor")]
        public int scoreValue = 10;

        [Header("Physics")]
        [Tooltip("Collider radius (CircleCollider2D için)")]
        public float colliderRadius = 0.5f;

        [Tooltip("Özel physics material (opsiyonel)")]
        public PhysicsMaterial2D optionalMaterial;

        public bool HasNextType => nextType != null;

        public override string ToString()
        {
            return $"{displayName} (ID:{id})";
        }
    }
}
