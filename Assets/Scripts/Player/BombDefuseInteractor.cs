using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(RoundParticipant))]
public class BombDefuseInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key defuseKey = Key.E;

    [Header("References")]
    [SerializeField] private BombSiteRegistry bombSiteRegistry;

    private RoundParticipant roundParticipant;
    private BombSiteDefuseInteraction currentSite;
    private BombSiteDefuseInteraction[] cachedSites = System.Array.Empty<BombSiteDefuseInteraction>();

    private void Awake()
    {
        roundParticipant = GetComponent<RoundParticipant>();

        if (bombSiteRegistry == null)
            bombSiteRegistry = BombSiteRegistry.Instance;

        cachedSites = Object.FindObjectsByType<BombSiteDefuseInteraction>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void Update()
    {
        bool isHoldingDefuse = Keyboard.current != null && Keyboard.current[defuseKey].isPressed;

        BombSiteDefuseInteraction targetSite = ResolveTargetSite();

        if (currentSite != null && currentSite != targetSite)
        {
            currentSite.CancelDefuse(roundParticipant);
            currentSite = null;
        }

        if (!isHoldingDefuse)
        {
            if (currentSite != null)
            {
                currentSite.CancelDefuse(roundParticipant);
                currentSite = null;
            }

            return;
        }

        if (targetSite == null)
            return;

        if (!targetSite.TryStartDefuse(roundParticipant))
            return;

        currentSite = targetSite;
        currentSite.TickDefuse(roundParticipant, Time.deltaTime);

        if (currentSite.IsDefuseCompleted)
            currentSite = null;
    }

    private BombSiteDefuseInteraction ResolveTargetSite()
    {
        BombSiteDefuseInteraction activeSiteInteraction = ResolveActiveSiteFromRegistry();

        if (activeSiteInteraction != null)
            return activeSiteInteraction;

        return ResolveClosestSiteFromCache();
    }

    private BombSiteDefuseInteraction ResolveActiveSiteFromRegistry()
    {
        if (bombSiteRegistry == null)
            return null;

        BombSite activeSite = bombSiteRegistry.GetActiveSite();

        if (activeSite == null)
            return null;

        BombSiteDefuseInteraction siteInteraction = activeSite.GetComponent<BombSiteDefuseInteraction>();
        return siteInteraction != null && siteInteraction.IsParticipantInRange(roundParticipant)
            ? siteInteraction
            : null;
    }

    private BombSiteDefuseInteraction ResolveClosestSiteFromCache()
    {
        BombSiteDefuseInteraction closestSite = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < cachedSites.Length; i++)
        {
            BombSiteDefuseInteraction site = cachedSites[i];

            if (site == null || !site.IsParticipantInRange(roundParticipant))
                continue;

            float distanceSqr = (site.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestSite = site;
            }
        }

        return closestSite;
    }
}
