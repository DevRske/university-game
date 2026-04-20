using UnityEngine;

/// <summary>
/// Refined camera follow component. Attach to the scene camera.
/// Follows an assigned target with optional map bounds clamping and configurable zoom.
/// Place under Assets/Scripts/ — not in Core/Systems/.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Zoom")]
    [SerializeField] private float orthographicSize = 5f;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 boundsMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 boundsMax = new Vector2(50f, 50f);

    private Camera _camera;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        if (_camera == null)
        {
            Debug.LogError("[CameraFollow] No Camera component found on this GameObject. " +
                           "Attach CameraFollow to the camera object.");
        }
    }

    private void Start()
    {
        ApplyZoom();

        if (target == null)
        {
            Debug.LogWarning("[CameraFollow] No follow target assigned. " +
                             "Assign one in the Inspector or call SetTarget() at runtime.");
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
            desiredPosition = ClampToBounds(desiredPosition);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Public API — change target at runtime
    // -------------------------------------------------------------------------

    /// <summary>
    /// Assigns a new follow target at runtime (e.g. when switching controlled player).
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target == null)
            Debug.LogWarning("[CameraFollow] SetTarget() called with null. Camera will hold its last position.");
    }

    /// <summary>
    /// Returns the current follow target.
    /// </summary>
    public Transform GetTarget() => target;

    // -------------------------------------------------------------------------
    // Zoom
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies the configured orthographic size to the camera.
    /// Call again if you change orthographicSize at runtime.
    /// </summary>
    public void ApplyZoom()
    {
        if (_camera != null && _camera.orthographic)
            _camera.orthographicSize = orthographicSize;
    }

    /// <summary>
    /// Changes orthographic zoom at runtime.
    /// </summary>
    public void SetZoom(float size)
    {
        orthographicSize = size;
        ApplyZoom();
    }

    // -------------------------------------------------------------------------
    // Bounds helpers
    // -------------------------------------------------------------------------

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (_camera == null) return position;

        // Account for how much world-space the camera viewport covers at this zoom level.
        float verticalExtent = _camera.orthographic ? _camera.orthographicSize : 0f;
        float horizontalExtent = verticalExtent * _camera.aspect;

        float clampedX = Mathf.Clamp(position.x, boundsMin.x + horizontalExtent, boundsMax.x - horizontalExtent);
        float clampedY = Mathf.Clamp(position.y, boundsMin.y + verticalExtent, boundsMax.y - verticalExtent);

        return new Vector3(clampedX, clampedY, position.z);
    }

#if UNITY_EDITOR
    // Draw bounds in Scene view for easy configuration.
    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((boundsMin.x + boundsMax.x) / 2f, (boundsMin.y + boundsMax.y) / 2f, 0f);
        Vector3 size = new Vector3(boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}