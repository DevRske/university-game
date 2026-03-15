using UnityEngine;

/// <summary>
/// Renders a vision cone in front of the player using 2D raycasting.
///
/// SETUP — add a child GameObject to the Player prefab named "VisionCone":
///   1. Attach this script to the VisionCone child.
///   2. Assign the VisionCone material (Vision/StencilWrite shader) to the child's MeshRenderer.
///   3. Assign the DarkOverlay material (Vision/DarkOverlay shader) to _darkOverlayMaterial.
///   4. Set _occlusionLayer to the VisionOccluder layer.
///
/// The VisionCone child co-rotates with the player root (which PlayerRotation.cs aims at the
/// mouse), so the cone automatically tracks the player's aim direction.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlayerVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float _viewRadius = 10f;
    [SerializeField][Range(1f, 360f)] private float _viewAngle = 90f;
    [SerializeField] private int _rayCount = 50;

    [Header("Layer Configuration")]
    [Tooltip("Set to the VisionOccluder layer. Only colliders on this layer will block vision.")]
    [SerializeField] private LayerMask _occlusionLayer;

    [Header("Dark Overlay")]
    [Tooltip("Material using the Vision/DarkOverlay shader. Covers everything outside the vision cone.")]
    [SerializeField] private Material _darkOverlayMaterial;

    private MeshFilter _visionMeshFilter;
    private Mesh _visionMesh;
    private Transform _overlayTransform;

    // Large enough to cover any camera view in the test map.
    private const float OverlayHalfSize = 100f;

    private void Awake()
    {
        _visionMeshFilter = GetComponent<MeshFilter>();

        MeshRenderer visionRenderer = GetComponent<MeshRenderer>();
        visionRenderer.sortingOrder = 99;

        _visionMesh = new Mesh { name = "VisionConeMesh" };
        _visionMeshFilter.mesh = _visionMesh;

        CreateDarkOverlay();
    }

    private void LateUpdate()
    {
        // Keep the overlay centred on our position so it always covers the screen.
        if (_overlayTransform != null)
            _overlayTransform.position = transform.position;

        DrawVisionMesh();
    }

    /// <summary>
    /// Creates the full-screen dark overlay as a world-space root object so it never inherits
    /// the player's rotation — the quad must always be axis-aligned to cover the view.
    /// </summary>
    private void CreateDarkOverlay()
    {
        if (_darkOverlayMaterial == null)
        {
            Debug.LogWarning("[PlayerVision] Dark overlay material not assigned. The fog of war will not render.", this);
            return;
        }

        GameObject overlayObj = new GameObject("VisionDarkOverlay");
        _overlayTransform = overlayObj.transform;
        _overlayTransform.position = transform.position;

        MeshFilter overlayFilter = overlayObj.AddComponent<MeshFilter>();
        MeshRenderer overlayRenderer = overlayObj.AddComponent<MeshRenderer>();
        overlayRenderer.material = _darkOverlayMaterial;
        overlayRenderer.sortingOrder = 100;

        float s = OverlayHalfSize;
        Mesh overlayMesh = new Mesh { name = "DarkOverlayMesh" };
        overlayMesh.vertices = new Vector3[]
        {
            new Vector3(-s, -s, 0f),
            new Vector3( s, -s, 0f),
            new Vector3( s,  s, 0f),
            new Vector3(-s,  s, 0f),
        };
        overlayMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        overlayMesh.RecalculateNormals();
        overlayFilter.mesh = overlayMesh;
    }

    /// <summary>
    /// Casts rays across the view angle arc each frame and rebuilds the vision cone mesh
    /// from the hit points (or max-range endpoints when no wall is hit).
    /// </summary>
    private void DrawVisionMesh()
    {
        float halfAngle = _viewAngle / 2f;
        float angleStep = _viewAngle / _rayCount;

        // +2 vertices: origin (index 0) plus one endpoint per ray boundary (_rayCount + 1)
        Vector3[] vertices = new Vector3[_rayCount + 2];
        int[] triangles = new int[_rayCount * 3];

        vertices[0] = Vector3.zero; // Local-space origin (the player's eye point)

        for (int i = 0; i <= _rayCount; i++)
        {
            // Rotate around local Z from the left edge to the right edge of the cone.
            // Local Vector2.up == the player's aim direction (parent root is aim-rotated by PlayerRotation).
            float localAngleDeg = -halfAngle + angleStep * i;
            Vector2 localDir = Quaternion.Euler(0f, 0f, localAngleDeg) * Vector2.up;
            Vector2 worldDir = transform.TransformDirection(localDir);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDir, _viewRadius, _occlusionLayer);

            Vector3 worldPoint = hit.collider != null
                ? (Vector3)hit.point
                : transform.position + (Vector3)worldDir * _viewRadius;

            // Store in local space so the mesh co-rotates correctly with the player.
            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);
        }

        for (int i = 0; i < _rayCount; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        _visionMesh.Clear();
        _visionMesh.vertices = vertices;
        _visionMesh.triangles = triangles;
        _visionMesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        if (_overlayTransform != null)
            Destroy(_overlayTransform.gameObject);
    }
}
