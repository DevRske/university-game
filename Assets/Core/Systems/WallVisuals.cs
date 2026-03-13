using UnityEngine;

[RequireComponent(typeof(WallHealth))]
[RequireComponent(typeof(SpriteRenderer))]

public class WallVisuals : MonoBehaviour
{
    [SerializeField] private Color intactColor = Color.white;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color destroyedColor = Color.black;

    private WallHealth wallHealth;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        wallHealth = GetComponent<WallHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        wallHealth.OnDamaged += HandleDamaged;
        wallHealth.OnDestroyed += HandleDestroyed;
    }
    private void OnDisable()
    {
        wallHealth.OnDamaged -= HandleDamaged;
        wallHealth.OnDestroyed -= HandleDestroyed;
    }

    private void HandleDamaged()
    {
        if (wallHealth.GetHealthPercentage() <= wallHealth.GetDamageThreshold())
        {
            spriteRenderer.color = damagedColor;
        }
    }

    private void HandleDestroyed()
    {
        spriteRenderer.color = destroyedColor;
    }
}
