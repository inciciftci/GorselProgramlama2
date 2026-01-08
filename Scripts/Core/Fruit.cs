using UnityEngine;
using FruitMerge.Data;

namespace FruitMerge.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Fruit : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxVelocity = 15f;

        [Header("Runtime State (Read-Only)")]
        [SerializeField, ReadOnly] private bool isLockedForMerge;
        [SerializeField, ReadOnly] private bool isInitialized;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private CircleCollider2D circleCol;
        private EdgeCollider2D edgeCol;
        private CapsuleCollider2D capsuleCol;
        private BoxCollider2D boxCol;
        private PolygonCollider2D polygonCol;
        private Collider2D activeCollider;

        private FruitTypeSO fruitType;

        public FruitTypeSO FruitType => fruitType;
        public bool IsLockedForMerge => isLockedForMerge;
        public bool IsInitialized => isInitialized;
        public Rigidbody2D Rigidbody => rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            circleCol = GetComponent<CircleCollider2D>();
            edgeCol = GetComponent<EdgeCollider2D>();
            capsuleCol = GetComponent<CapsuleCollider2D>();
            boxCol = GetComponent<BoxCollider2D>();
            polygonCol = GetComponent<PolygonCollider2D>();

            if (circleCol != null) activeCollider = circleCol;
            else if (edgeCol != null) activeCollider = edgeCol;
            else if (capsuleCol != null) activeCollider = capsuleCol;
            else if (boxCol != null) activeCollider = boxCol;
            else if (polygonCol != null) activeCollider = polygonCol;
        }

        public void Initialize(FruitTypeSO type, Vector2 position, bool isKinematic = false)
        {
            if (type == null)
            {
                Debug.LogError("[Fruit] Initialize: FruitType null!");
                return;
            }

            fruitType = type;
            transform.position = position;
            transform.rotation = Quaternion.identity;

            if (spriteRenderer != null && type.sprite != null)
            {
                spriteRenderer.sprite = type.sprite;

                float scaleMultiplier;
                if (type.colliderRadius >= 1.0f)
                {
                    scaleMultiplier = 0.11f / 0.3f;
                }
                else
                {
                    scaleMultiplier = type.colliderRadius / 0.3f;
                }
                transform.localScale = Vector3.one * scaleMultiplier;

                Vector3 pos = transform.position;
                pos.z = 0f;
                transform.position = pos;

                spriteRenderer.sortingOrder = Time.frameCount % 1000;
            }

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Bounds spriteBounds = spriteRenderer.sprite.bounds;
                Vector2 spriteCenter = spriteBounds.center;

                if (circleCol != null)
                {
                    SetupCircleCollider(circleCol, type, spriteBounds, spriteCenter);
                }
                else if (edgeCol != null)
                {
                    SetupEdgeCollider(edgeCol, type, spriteBounds, spriteCenter);
                }
                else if (capsuleCol != null)
                {
                    SetupCapsuleCollider(capsuleCol, type, spriteBounds, spriteCenter);
                }
                else if (boxCol != null)
                {
                    SetupBoxCollider(boxCol, type, spriteBounds, spriteCenter);
                }
                else if (polygonCol != null)
                {
                    SetupPolygonCollider(polygonCol, type, spriteBounds, spriteCenter);
                }
            }

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
                rb.simulated = true;

                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                if (type.optionalMaterial != null && rb.sharedMaterial == null)
                {
                    rb.sharedMaterial = type.optionalMaterial;
                }
            }

            isLockedForMerge = false;
            isInitialized = true;

            if (activeCollider != null) activeCollider.enabled = true;

            gameObject.SetActive(true);
        }

        public void LockForMerge()
        {
            if (isLockedForMerge)
            {
                Debug.LogWarning($"[Fruit] {fruitType.displayName} zaten kilitli!");
                return;
            }

            isLockedForMerge = true;

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }

            if (activeCollider != null)
            {
                activeCollider.enabled = false;
            }
        }

        public void ResetForPool()
        {
            isLockedForMerge = false;
            isInitialized = false;
            fruitType = null;

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
            }

            if (activeCollider != null)
            {
                activeCollider.enabled = true;
            }

            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (rb != null && !isLockedForMerge && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                if (rb.velocity.sqrMagnitude > maxVelocity * maxVelocity)
                {
                    rb.velocity = rb.velocity.normalized * maxVelocity;
                }

                if (transform.position.z != 0f)
                {
                    Vector3 pos = transform.position;
                    pos.z = 0f;
                    transform.position = pos;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ProcessCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            ProcessCollision(collision);
        }

        private void SetupCircleCollider(CircleCollider2D col, FruitTypeSO type, Bounds spriteBounds, Vector2 spriteCenter)
        {
            bool prefabHasRadius = col.radius > 0.01f;

            if (!prefabHasRadius)
            {
                float spriteWidth = spriteBounds.size.x * transform.localScale.x;
                float spriteHeight = spriteBounds.size.y * transform.localScale.y;

                float spriteMinSize = Mathf.Min(spriteWidth, spriteHeight);

                float baseRadius = spriteMinSize / 2f;

                if (type.colliderRadius > 0.01f)
                {
                    if (type.colliderRadius >= 1.0f)
                    {
                        baseRadius = spriteMinSize / 2f;
                    }
                    else
                    {
                        baseRadius = (spriteMinSize / 2f) * type.colliderRadius;
                    }
                }

                col.radius = baseRadius;
            }

            if (col.density <= 0.01f) col.density = 1f;
            col.usedByEffector = false;
            col.usedByComposite = false;
            col.isTrigger = false;

            if (col.offset == Vector2.zero)
            {
                col.offset = new Vector2(-spriteCenter.x, -spriteCenter.y);
            }

            if (type.optionalMaterial != null && col.sharedMaterial == null)
            {
                col.sharedMaterial = type.optionalMaterial;
            }
        }

        private void SetupEdgeCollider(EdgeCollider2D col, FruitTypeSO type, Bounds spriteBounds, Vector2 spriteCenter)
        {
            bool prefabHasPoints = col.pointCount > 0;

            if (col.edgeRadius <= 0.01f) col.edgeRadius = 0.01f;
            if (col.density <= 0.01f) col.density = 1f;
            col.usedByEffector = false;
            col.usedByComposite = false;
            col.isTrigger = false;

            if (!prefabHasPoints && spriteRenderer.sprite != null)
            {
                Vector2[] edgePoints = new Vector2[]
                {
                    new Vector2(-spriteBounds.extents.x, -spriteBounds.extents.y),
                    new Vector2(spriteBounds.extents.x, -spriteBounds.extents.y),
                    new Vector2(spriteBounds.extents.x, spriteBounds.extents.y),
                    new Vector2(-spriteBounds.extents.x, spriteBounds.extents.y),
                    new Vector2(-spriteBounds.extents.x, -spriteBounds.extents.y)
                };
                col.points = edgePoints;
            }

            if (col.offset == Vector2.zero)
            {
                col.offset = new Vector2(-spriteCenter.x, -spriteCenter.y);
            }

            if (type.optionalMaterial != null && col.sharedMaterial == null)
            {
                col.sharedMaterial = type.optionalMaterial;
            }
        }

        private void SetupCapsuleCollider(CapsuleCollider2D col, FruitTypeSO type, Bounds spriteBounds, Vector2 spriteCenter)
        {
            bool prefabHasSize = col.size.x > 0.01f && col.size.y > 0.01f;

            if (!prefabHasSize)
            {
                Vector2 size = new Vector2(spriteBounds.size.x * transform.localScale.x,
                                           spriteBounds.size.y * transform.localScale.y);

                size.x = Mathf.Max(size.x, 0.01f);
                size.y = Mathf.Max(size.y, 0.01f);

                col.size = size;

                col.direction = size.y >= size.x ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
            }

            if (col.density <= 0.01f) col.density = 1f;
            col.usedByEffector = false;
            col.usedByComposite = false;
            col.isTrigger = false;

            if (col.offset == Vector2.zero)
            {
                col.offset = new Vector2(-spriteCenter.x, -spriteCenter.y);
            }

            if (type.optionalMaterial != null && col.sharedMaterial == null)
            {
                col.sharedMaterial = type.optionalMaterial;
            }
        }

        private void SetupBoxCollider(BoxCollider2D col, FruitTypeSO type, Bounds spriteBounds, Vector2 spriteCenter)
        {
            bool prefabHasSize = col.size.x > 0.01f && col.size.y > 0.01f;

            if (!prefabHasSize)
            {
                Vector2 size = new Vector2(spriteBounds.size.x * transform.localScale.x,
                                           spriteBounds.size.y * transform.localScale.y);

                size.x = Mathf.Max(size.x, 0.01f);
                size.y = Mathf.Max(size.y, 0.01f);

                col.size = size;
            }

            if (col.density <= 0.01f) col.density = 1f;
            col.usedByEffector = false;
            col.usedByComposite = false;
            col.isTrigger = false;
            col.edgeRadius = 0f;

            if (col.offset == Vector2.zero)
            {
                col.offset = new Vector2(-spriteCenter.x, -spriteCenter.y);
            }

            if (type.optionalMaterial != null && col.sharedMaterial == null)
            {
                col.sharedMaterial = type.optionalMaterial;
            }
        }

        private void SetupPolygonCollider(PolygonCollider2D col, FruitTypeSO type, Bounds spriteBounds, Vector2 spriteCenter)
        {
            bool prefabHasPath = col.pathCount > 0 && col.GetTotalPointCount() > 0;

            if (col.density <= 0.01f) col.density = 1f;
            col.usedByEffector = false;
            col.usedByComposite = false;
            col.isTrigger = false;

            if (!prefabHasPath)
            {
                Sprite sprite = spriteRenderer.sprite;
                if (sprite != null)
                {
                    col.pathCount = 0;

                    Vector2[] points = sprite.vertices.Length > 0
                        ? System.Array.ConvertAll(sprite.vertices, v => (Vector2)v)
                        : GenerateBoxPoints(spriteBounds);

                    if (points.Length > 16)
                    {
                        points = OptimizePolygonPoints(points, 16);
                    }

                    col.SetPath(0, points);
                }
            }

            if (col.offset == Vector2.zero)
            {
                col.offset = new Vector2(-spriteCenter.x, -spriteCenter.y);
            }

            if (type.optionalMaterial != null && col.sharedMaterial == null)
            {
                col.sharedMaterial = type.optionalMaterial;
            }
        }

        private Vector2[] GenerateBoxPoints(Bounds bounds)
        {
            float halfWidth = bounds.size.x / 2f;
            float halfHeight = bounds.size.y / 2f;

            return new Vector2[]
            {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(halfWidth, -halfHeight),
                new Vector2(halfWidth, halfHeight),
                new Vector2(-halfWidth, halfHeight)
            };
        }

        private Vector2[] OptimizePolygonPoints(Vector2[] points, int maxPoints)
        {
            if (points.Length <= maxPoints)
                return points;

            int step = Mathf.Max(1, points.Length / maxPoints);
            int newLength = Mathf.Min(maxPoints, points.Length / step);
            Vector2[] optimized = new Vector2[newLength];

            for (int i = 0; i < newLength; i++)
            {
                optimized[i] = points[i * step];
            }

            return optimized;
        }

        private void ProcessCollision(Collision2D collision)
        {
            if (isLockedForMerge) return;

            Fruit otherFruit = collision.gameObject.GetComponent<Fruit>();
            if (otherFruit == null) return;

            if (otherFruit.IsLockedForMerge) return;

            if (fruitType == null || otherFruit.FruitType == null) return;

            if (fruitType.id != otherFruit.FruitType.id) return;

            if (collision.contactCount > 0)
            {
                if (MergeSystem.Instance == null)
                {
                    Debug.LogError("[Fruit] MergeSystem.Instance null! MergeSystem GameObject'i scene'de var mı?");
                    return;
                }

                Vector2 contactPoint = collision.contacts[0].point;

                Debug.Log($"[MERGE] {fruitType.displayName} (ID:{fruitType.id}) collided with {otherFruit.FruitType.displayName} (ID:{otherFruit.FruitType.id})");

                MergeSystem.Instance.OnFruitCollision(this, otherFruit, contactPoint);
            }
        }

        private void OnDrawGizmos()
        {
            if (isLockedForMerge)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.6f);
            }
        }

#if UNITY_EDITOR
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
