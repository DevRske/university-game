using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Systems;

[DisallowMultipleComponent]
public class RoundEliminationTracker : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    private readonly List<RoundParticipant> participants = new();
    private readonly Dictionary<PlayerHealth, Action<float, float>> healthSubscriptions = new();
    private readonly HashSet<RoundParticipant> eliminatedParticipants = new();

    private void Awake()
    {
        if (roundManager == null)
            roundManager = UnityEngine.Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);
    }

    private void OnEnable()
    {
        if (roundManager != null)
            roundManager.OnRoundStateChanged += HandleRoundStateChanged;

        RefreshParticipants();
    }

    private void OnDisable()
    {
        if (roundManager != null)
            roundManager.OnRoundStateChanged -= HandleRoundStateChanged;

        UnsubscribeFromHealthEvents();
        participants.Clear();
        eliminatedParticipants.Clear();
    }

    private void HandleRoundStateChanged(RoundState state)
    {
        if (state == RoundState.Active || state == RoundState.Overtime)
            RefreshParticipants();
    }

    private void RefreshParticipants()
    {
        UnsubscribeFromHealthEvents();
        CacheParticipants();
        SubscribeToHealthEvents();
        EvaluateTeamsAfterElimination();
    }

    private void CacheParticipants()
    {
        participants.Clear();
        eliminatedParticipants.Clear();

        RoundParticipant[] foundParticipants = UnityEngine.Object.FindObjectsByType<RoundParticipant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (RoundParticipant participant in foundParticipants)
        {
            if (participant == null)
                continue;

            participants.Add(participant);

            if (participant.IsEliminated)
                eliminatedParticipants.Add(participant);
        }
    }

    private void SubscribeToHealthEvents()
    {
        healthSubscriptions.Clear();

        foreach (RoundParticipant participant in participants)
        {
            PlayerHealth health = participant != null ? participant.GetComponentInChildren<PlayerHealth>() : null;

            if (health == null)
            {
                Debug.LogWarning("[RoundEliminationTracker] RoundParticipant is missing a PlayerHealth component.");
                continue;
            }

            Action<float, float> handler = (current, _) => HandleHealthChanged(participant, current);
            healthSubscriptions[health] = handler;
            health.onHealthChanged += handler;
        }
    }

    private void UnsubscribeFromHealthEvents()
    {
        foreach (KeyValuePair<PlayerHealth, Action<float, float>> subscription in healthSubscriptions)
        {
            if (subscription.Key == null)
                continue;

            subscription.Key.onHealthChanged -= subscription.Value;
        }

        healthSubscriptions.Clear();
    }

    private void HandleHealthChanged(RoundParticipant participant, float currentHealth)
    {
        if (participant == null || currentHealth > 0f)
            return;

        Debug.Log($"[RoundEliminationTracker] {participant.name} eliminated. Checking teams.");

        if (!eliminatedParticipants.Add(participant))
            return;

        EvaluateTeamsAfterElimination();
    }

    private void EvaluateTeamsAfterElimination()
    {
        if (roundManager == null || roundManager.State == RoundState.Initialising || roundManager.State == RoundState.Ended)
            return;

        int attackersAlive = 0;
        int defendersAlive = 0;

        foreach (RoundParticipant participant in participants)
        {
            if (participant == null || participant.IsEliminated || eliminatedParticipants.Contains(participant))
                continue;

            if (participant.TeamSide == TeamSide.Attacker)
                attackersAlive++;
            else
                defendersAlive++;
        }

        if (defendersAlive == 0)
        {
            Debug.Log("[RoundEliminationTracker] All defenders eliminated. Attackers win.");
            roundManager.EndRound(RoundOutcome.AttackersWin);
            return;
        }

        if (attackersAlive == 0)
        {
            Debug.Log("[RoundEliminationTracker] All attackers eliminated. Defenders win.");
            roundManager.EndRound(RoundOutcome.DefendersWin);
        }
    }
}
