using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BombSite : MonoBehaviour
{
    [SerializeField] private string siteID = "A";

    public string SiteID => siteID;

    public void OnBombPlanted()
    {
        Debug.Log($"[BombSite] Bomb planted at site {siteID}");
    }

    public void OnBombDefused()
    {
        Debug.Log($"[BombSite] Bomb defused at site {siteID}");
    }
}