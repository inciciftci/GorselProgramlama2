using FruitMerge.Core;
using FruitMerge.Data;
using UnityEngine;

namespace FruitMerge.Managers
{
    public class FruitSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private SpawnQueueManager spawnQueueManager;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnY = 8f;
        [SerializeField] private float minX = -6f;
        [SerializeField] private float maxX = 6f;
        [SerializeField] private float previewOffsetY = 0.5f;

        [Header("Input Settings")]
        [SerializeField] private bool allowInput = true;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        private Fruit previewFruit;
        private bool isDragging = false;
        private bool hasDropped = false;

        private Camera mainCamera;

        public bool AllowInput
        {
            get => allowInput;
            set => allowInput = value;
        }

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (!allowInput) return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                hasDropped = false;
            }

            if (Input.GetMouseButton(0) && isDragging && !hasDropped)
            {
                MovePreviewToMousePosition();
            }

            if (Input.GetMouseButtonUp(0) && isDragging && !hasDropped)
            {
                DropFruit();
                isDragging = false;
                hasDropped = true;
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    hasDropped = false;
                }
                else if (touch.phase == TouchPhase.Moved && isDragging && !hasDropped)
                {
                    MovePreviPreviewToTouchPosition(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended && isDragging && !hasDropped)
                {
                    DropFruit();
                    isDragging = false;
                    hasDropped = true;
                }
            }
        }

        private void SpawnPreviewFruit()
        {
            if (spawnQueueManager == null || poolManager == null)
            {
                Debug.LogError("[FruitSpawner] SpawnQueueManager veya PoolManager null!");
                return;
            }

            FruitTypeSO currentType = spawnQueueManager.CurrentFruit;
            if (currentType == null)
            {
                Debug.LogError("[FruitSpawner] CurrentFruit null!");
                return;
            }

            Vector2 spawnPosition = new Vector2(0, spawnY + previewOffsetY);
            previewFruit = poolManager.Get(currentType, spawnPosition, isKinematic: true);

            if (previewFruit != null)
            {
                previewFruit.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
                previewFruit.Rigidbody.simulated = false;

                if (debugMode)
                {
                    Debug.Log($"[FruitSpawner] Preview spawn: {currentType.displayName}");
                }
            }
        }

        private void MovePreviewToMousePosition()
        {
            if (previewFruit == null || mainCamera == null) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            float clampedX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);

            previewFruit.transform.position = new Vector3(clampedX, spawnY + previewOffsetY, 0f);
        }

        private void MovePreviPreviewToTouchPosition(Vector2 touchPosition)
        {
            if (previewFruit == null || mainCamera == null) return;

            Vector3 touchWorldPos = mainCamera.ScreenToWorldPoint(touchPosition);
            float clampedX = Mathf.Clamp(touchWorldPos.x, minX, maxX);

            previewFruit.transform.position = new Vector3(clampedX, spawnY + previewOffsetY, 0f);
        }

        private void DropFruit()
        {
            if (previewFruit == null) return;

            Vector3 currentPos = previewFruit.transform.position;
            float randomOffsetX = Random.Range(-0.02f, 0.02f);
            
            previewFruit.transform.position = new Vector3(currentPos.x + randomOffsetX, currentPos.y, 0f);

            previewFruit.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            previewFruit.Rigidbody.simulated = true;
            previewFruit.Rigidbody.velocity = Vector2.zero;
            previewFruit.Rigidbody.angularVelocity = 0f;
            
            previewFruit.Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            previewFruit.Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            if (previewFruit.Rigidbody.mass > 0.5f)
            {
                previewFruit.Rigidbody.mass = 0.5f;
            }

            if (debugMode)
            {
                Debug.Log($"[FruitSpawner] Meyve düşürüldü: {previewFruit.FruitType.displayName} at {previewFruit.transform.position}");
            }

            spawnQueueManager.AdvanceQueue();
            previewFruit = null;

            Invoke(nameof(SpawnPreviewFruit), 0.3f);
        }

        public void ResetSpawner()
        {
            if (previewFruit != null)
            {
                poolManager.Return(previewFruit);
                previewFruit = null;
            }

            isDragging = false;
            hasDropped = false;

            CancelInvoke(nameof(SpawnPreviewFruit));

            if (debugMode)
            {
                Debug.Log("[FruitSpawner] Spawner resetlendi.");
            }
        }

        public void StartSpawning()
        {
            ResetSpawner();
            SpawnPreviewFruit();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(minX, spawnY, 0), new Vector3(maxX, spawnY, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(minX, spawnY, 0), 0.3f);
            Gizmos.DrawWireSphere(new Vector3(maxX, spawnY, 0), 0.3f);
        }
    }
}
