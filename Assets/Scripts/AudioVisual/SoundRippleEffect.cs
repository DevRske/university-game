using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class SoundRippleEffect : MonoBehaviour
{
    [Header("Ripple Settings")]
    [SerializeField] private float maxRadius = 3f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float fadeSpeed = 1.25f;
    [SerializeField] private float lineWidth = 0.04f;
    [SerializeField] private Color rippleColor = new Color(0.25f, 0.8f, 1f, 0.85f);
    [SerializeField] private int segmentCount = 64;
    [SerializeField] private int sortingOrder = 110;

    private LineRenderer lineRenderer;
    private float elapsedTime;

    private const int MinimumSegments = 8;
    private const float MinimumDuration = 0.01f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
    }

    private void OnEnable()
    {
        elapsedTime = 0f;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        ConfigureLineRenderer();
        DrawRipple(0f, rippleColor.a);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        float safeDuration = Mathf.Max(duration, MinimumDuration);
        float normalizedTime = Mathf.Clamp01(elapsedTime / safeDuration);
        float radius = Mathf.Lerp(0f, maxRadius, normalizedTime);
        float alpha = Mathf.Clamp01(rippleColor.a - elapsedTime * fadeSpeed);

        DrawRipple(radius, alpha);

        if (elapsedTime >= safeDuration)
            Destroy(gameObject);
    }

    private void ConfigureLineRenderer()
    {
        int safeSegmentCount = Mathf.Max(segmentCount, MinimumSegments);

        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = safeSegmentCount;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.sortingOrder = sortingOrder;
    }

    private void DrawRipple(float radius, float alpha)
    {
        int safeSegmentCount = Mathf.Max(segmentCount, MinimumSegments);
        Color currentColor = new Color(rippleColor.r, rippleColor.g, rippleColor.b, alpha);

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        if (lineRenderer.positionCount != safeSegmentCount)
            lineRenderer.positionCount = safeSegmentCount;

        for (int i = 0; i < safeSegmentCount; i++)
        {
            float angle = i / (float)safeSegmentCount * Mathf.PI * 2f;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            lineRenderer.SetPosition(i, point);
        }
    }
}
