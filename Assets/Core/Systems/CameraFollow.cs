using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Map Bounds")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    private float zOffset;
    private Camera cam;

    private void Start()
    {
        zOffset = transform.position.z;
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.orthographicSize * cam.aspect;

        float clampedX = Mathf.Clamp(target.position.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, minY + halfHeight, maxY - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, zOffset);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}