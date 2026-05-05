using System.Collections.Generic;
using UnityEngine;
using Core.Systems;

[DisallowMultipleComponent]
public class RoundEliminationTracker : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;

    private readonly List<RoundParticipant> participants = new();
    private readonly HashSet<RoundParticipant> attackersAlive = new();
    private readonly HashSet<RoundParticipant> defendersAlive = new();
    private readonly Dictionary<RoundParticipant, System.Action> eliminationHandlers = new();

    private void Awake()
    {
        if (roundManager == null)
            roundManager = Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);

        participants.AddRange(Object.FindObjectsByType<RoundParticipant>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void OnEnable()
    {
        if (roundManager != null)
            roundManager.OnRoundEnded += HandleRoundEnded;

        RegisterParticipants();
        SeedAliveParticipants();
    }

    private void OnDisable()
    {
        if (roundManager != null)
            roundManager.OnRoundEnded -= HandleRoundEnded;

        UnsubscribeFromParticipants();
    }

    private void RegisterParticipants()
    {
        eliminationHandlers.Clear();

        foreach (RoundParticipant participant in participants)
        {
            if (participant == null)
                continue;

            PlayerHealth health = participant.GetComponent<PlayerHealth>();
            if (health == null)
                continue;

            System.Action handler = () => HandleParticipantEliminated(participant);
            eliminationHandlers[participant] = handler;
            health.onEliminated += handler;
        }
    }

    private void UnsubscribeFromParticipants()
    {
        foreach (RoundParticipant participant in participants)
        {
            if (participant == null)
                continue;

            PlayerHealth health = participant.GetComponent<PlayerHealth>();
            if (health == null)
                continue;

            if (!eliminationHandlers.TryGetValue(participant, out System.Action handler))
                continue;

            health.onEliminated -= handler;
        }

        eliminationHandlers.Clear();
    }

    private void SeedAliveParticipants()
    {
        attackersAlive.Clear();
        defendersAlive.Clear();

        foreach (RoundParticipant participant in participants)
        {
            if (participant == null)
                continue;

            if (participant.IsEliminated)
                continue;

            if (participant.TeamSide == TeamSide.Attacker)
                attackersAlive.Add(participant);
            else
                defendersAlive.Add(participant);
        }
    }

    private void HandleParticipantEliminated(RoundParticipant participant)
    {
        if (roundManager == null || roundManager.State == RoundState.Ended)
            return;

        if (participant == null)
            return;

        if (participant.TeamSide == TeamSide.Attacker)
            attackersAlive.Remove(participant);
        else
            defendersAlive.Remove(participant);

        EvaluateTeams();
    }

    private void EvaluateTeams()
    {
        if (defendersAlive.Count == 0)
            roundManager.EndRound(RoundOutcome.AttackersWin);
        else if (attackersAlive.Count == 0)
            roundManager.EndRound(RoundOutcome.DefendersWin);
    }

    private void HandleRoundEnded(RoundOutcome _)
    {
        attackersAlive.Clear();
        defendersAlive.Clear();
    }

    public void RefreshParticipants()
    {
        participants.Clear();
        participants.AddRange(Object.FindObjectsByType<RoundParticipant>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        UnsubscribeFromParticipants();
        RegisterParticipants();
        SeedAliveParticipants();
    }
}
