using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BombSiteDefuseInteraction))]
public class BombSiteDefuseRoundReporter : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    private BombSiteDefuseInteraction defuseInteraction;

    private void Awake()
    {
        defuseInteraction = GetComponent<BombSiteDefuseInteraction>();

        if (roundManager == null)
            roundManager = Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);
    }

    private void OnEnable()
    {
        if (defuseInteraction == null)
            return;

        defuseInteraction.OnDefuseStarted += HandleDefuseStarted;
        defuseInteraction.OnDefuseCancelled += HandleDefuseCancelled;
    }

    private void OnDisable()
    {
        if (defuseInteraction == null)
            return;

        defuseInteraction.OnDefuseStarted -= HandleDefuseStarted;
        defuseInteraction.OnDefuseCancelled -= HandleDefuseCancelled;
    }

    private void HandleDefuseStarted(RoundParticipant _)
    {
        if (roundManager != null)
            roundManager.NotifyDefuseStarted();
    }

    private void HandleDefuseCancelled(RoundParticipant _)
    {
        if (roundManager != null)
            roundManager.NotifyDefuseCancelled();
    }
}
