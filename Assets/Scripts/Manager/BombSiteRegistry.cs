using UnityEngine;

public class BombSiteRegistry : MonoBehaviour
{
    public static BombSiteRegistry Instance { get; private set; }

    [SerializeField] private BombSite activeBombSite;

    public BombSite ActiveBombSite => activeBombSite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public BombSite GetActiveSite()
    {
        if (activeBombSite == null)
            Debug.LogWarning("[BombSiteRegistry] No active bomb site assigned.");

        return activeBombSite;
    }
}