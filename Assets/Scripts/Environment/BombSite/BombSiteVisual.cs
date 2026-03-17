using UnityEngine;

[RequireComponent(typeof(BombSite))]
public class BombSiteVisual : MonoBehaviour
{
    [SerializeField] private Color fillColor = new Color(1f, 0.8f, 0f, 0.25f);
    [SerializeField] private Color borderColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Vector2 size = new Vector2(3f, 3f);

    private void OnDrawGizmos()
    {
        // Fill
        Gizmos.color = fillColor;
        Gizmos.DrawCube(transform.position, new Vector3(size.x, size.y, 0f));

        // Border
        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
    }
}