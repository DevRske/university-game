using System;
using UnityEngine;
using Core.Systems;

[DisallowMultipleComponent]
[RequireComponent(typeof(BombSite))]
public class BombSiteDefuseInteraction : MonoBehaviour
{
    [Header("Defuse Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float defuseDurationSeconds = 5f;
    [SerializeField] private bool resetProgressWhenCancelled = true;

    [Header("Debug")]
    [SerializeField] private BombSiteDefuseState state = BombSiteDefuseState.Inactive;
    [SerializeField] private float currentProgressSeconds;

    private BombSite bombSite;
    private RoundParticipant currentDefuser;

    public event Action<RoundParticipant> OnDefuseStarted;
    public event Action<RoundParticipant> OnDefuseCancelled;
    public event Action<RoundParticipant> OnDefuseCompleted;
    public event Action<float> OnDefuseProgressChanged;

    public BombSiteDefuseState State => state;
    public bool IsDefuseActive => state == BombSiteDefuseState.Active;
    public bool IsDefuseCompleted => state == BombSiteDefuseState.Completed;
    public RoundParticipant CurrentDefuser => currentDefuser;
    public float CurrentProgressSeconds => currentProgressSeconds;
    public float CurrentProgressNormalized => defuseDurationSeconds <= 0f
        ? 1f
        : Mathf.Clamp01(currentProgressSeconds / defuseDurationSeconds);

    private void Awake()
    {
        bombSite = GetComponent<BombSite>();

        if (interactionRange < 0f)
            interactionRange = 0f;

        if (defuseDurationSeconds <= 0f)
            defuseDurationSeconds = 0.1f;
    }

    public bool IsParticipantInRange(RoundParticipant participant)
    {
        if (participant == null)
            return false;

        float sqrDistance = (participant.transform.position - transform.position).sqrMagnitude;
        return sqrDistance <= interactionRange * interactionRange;
    }

    public bool TryStartDefuse(RoundParticipant participant)
    {
        if (!CanParticipantDefuse(participant))
            return false;

        if (state == BombSiteDefuseState.Completed)
            return false;

        if (state == BombSiteDefuseState.Active)
            return currentDefuser == participant;

        currentDefuser = participant;
        state = BombSiteDefuseState.Active;
        OnDefuseStarted?.Invoke(participant);
        return true;
    }

    public void TickDefuse(RoundParticipant participant, float deltaTime)
    {
        if (state != BombSiteDefuseState.Active)
            return;

        if (participant == null || participant != currentDefuser)
            return;

        if (!CanParticipantDefuse(participant) || !IsParticipantInRange(participant))
        {
            CancelDefuse(participant);
            return;
        }

        currentProgressSeconds = Mathf.Min(currentProgressSeconds + Mathf.Max(deltaTime, 0f), defuseDurationSeconds);
        OnDefuseProgressChanged?.Invoke(CurrentProgressNormalized);

        if (currentProgressSeconds >= defuseDurationSeconds)
        {
            CompleteDefuse();
        }
    }

    public void CancelDefuse(RoundParticipant participant)
    {
        if (state != BombSiteDefuseState.Active)
            return;

        if (participant != null && participant != currentDefuser)
            return;

        RoundParticipant cancelledBy = currentDefuser;

        state = BombSiteDefuseState.Inactive;
        currentDefuser = null;

        if (resetProgressWhenCancelled)
        {
            currentProgressSeconds = 0f;
            OnDefuseProgressChanged?.Invoke(CurrentProgressNormalized);
        }

        OnDefuseCancelled?.Invoke(cancelledBy);
    }

    private void CompleteDefuse()
    {
        state = BombSiteDefuseState.Completed;

        RoundParticipant completedBy = currentDefuser;
        currentDefuser = null;

        bombSite.OnBombDefused();
        OnDefuseCompleted?.Invoke(completedBy);
    }

    private bool CanParticipantDefuse(RoundParticipant participant)
    {
        if (participant == null)
            return false;

        if (participant.TeamSide != TeamSide.Attacker)
            return false;

        if (!participant.IsEligibleForDefuse)
            return false;

        if (!IsParticipantInRange(participant))
            return false;

        return true;
    }
}
