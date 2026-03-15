using UnityEngine;

public class WallHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private WallType wallType;

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageThreshold = 50f;

    public event System.Action OnDamaged;
    public event System.Action OnDestroyed;

    private float currentHealth;
    private Collider2D wallCollider;

    private void Awake()
    {
        currentHealth = maxHealth;
        wallCollider = GetComponent<Collider2D>();
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

        if (!CanTakeBulletDamage())
        {
            return; // Ignore damage if the wall cannot take bullet damage
        }
        currentHealth -= amount;
        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            wallCollider.enabled = false;
            OnDestroyed?.Invoke();
        }
    }
    private bool CanTakeBulletDamage()
    {
        switch (wallType)
        {
            case WallType.PaperSoft:
                return true; // Can take damage from bullets
            case WallType.StructuralSoft:
                return false; // Cannot take damage from bullets
            case WallType.HardWall:
                return false; // Cannot take damage from bullets
            case WallType.Reinforced:
                return false; // Cannot take damage from bullets
            default:
                return false;
        }
    }

    public float CurrentHealth => currentHealth;
    public float DamageThreshold => damageThreshold;
}
