using System;
using System.Collections;
using UnityEngine;
using Core.Systems;

// Controls one playable round from start to finish.
// The lifecycle is: Initialising -> Active -> (Overtime if needed) -> Ended.
// Other systems should not drive state themselves, they should call the public methods here and subscribe to the events.
//
// How to start a round: call StartRound() when your scene is ready.
// How to react to round events: subscribe to OnRoundStateChanged or OnRoundEnded before calling StartRound.
// How to wire in a defuse system: call NotifyDefuseStarted() when a player begins defusing and NotifyDefuseCancelled() if they stop.
// How to end the round from outside: call EndRound(RoundOutcome.DefendersWin) if all attackers are eliminated, for example.
// How to handle a completed defuse: there is no NotifyDefuseCompleted method by design since defuse logic is a later story.
//   When you build that system, call EndRound(RoundOutcome.AttackersWin) from it directly once the defuse finishes.
public class RoundManager : MonoBehaviour
{
    [SerializeField] private float roundDuration = 120f;

    // Fires every time the state changes, useful for UI or other systems that need to react to lifecycle transitions.
    public event Action<RoundState> OnRoundStateChanged;

    // Fires once when the round ends and carries the final outcome.
    public event Action<RoundOutcome> OnRoundEnded;

    // Fires each frame while Active, carrying the current remaining seconds so UI can update without polling.
    public event Action<float> OnTimerTick;

    public RoundState State { get; private set; } = RoundState.Initialising;
    public RoundOutcome Outcome { get; private set; } = RoundOutcome.None;
    public float TimeRemaining { get; private set; }

    // Tracks whether a defuse is currently happening so we know whether to enter Overtime when the timer runs out.
    private bool _defuseActive;
    private bool _timerExpiryPending;
    private Coroutine _timerExpiryCoroutine;

    public bool CanRestart => State == RoundState.Ended;

    private void Awake()
    {
        TimeRemaining = roundDuration;
    }

    private void Update()
    {
        if (State != RoundState.Active) return;

        // Clamp before broadcasting so no subscriber ever receives a negative value.
        TimeRemaining = Mathf.Max(TimeRemaining - Time.deltaTime, 0f);
        OnTimerTick?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            QueueTimerExpiryResolution();
        }
    }

    // Call this to kick off the round. Nothing moves until this is called.
    public void StartRound()
    {
        if (State != RoundState.Initialising)
        {
            Debug.LogWarning($"[RoundManager] StartRound called in state {State}. Ignored.");
            return;
        }

        // Catch a misconfigured Inspector value before the timer causes an instant expiry.
        if (roundDuration <= 0f)
        {
            Debug.LogError("[RoundManager] roundDuration must be greater than zero. Round not started.");
            return;
        }

        TimeRemaining = roundDuration;
        _defuseActive = false;
        ClearPendingTimerExpiryResolution();
        Outcome = RoundOutcome.None;

        SetState(RoundState.Active);
        Debug.Log($"[RoundManager] Round started. Duration: {roundDuration}s");
    }

    public void RestartRound()
    {
        if (!CanRestart)
        {
            Debug.LogWarning($"[RoundManager] RestartRound called in state {State}. Ignored.");
            return;
        }

        TimeRemaining = roundDuration;
        _defuseActive = false;
        ClearPendingTimerExpiryResolution();
        Outcome = RoundOutcome.None;

        SetState(RoundState.Active);
        Debug.Log($"[RoundManager] Round restarted. Duration: {roundDuration}s");
    }

    // Call this from your defuse system when a player begins a defuse attempt.
    public void NotifyDefuseStarted()
    {
        if (State == RoundState.Ended)
        {
            Debug.LogWarning("[RoundManager] NotifyDefuseStarted called after round ended. Ignored.");
            return;
        }

        _defuseActive = true;
        Debug.Log("[RoundManager] Defuse started.");
    }

    // Call this when the defuse is interrupted, for example when the player leaves the zone.
    // If the timer already expired and we are sitting in Overtime, cancelling here resolves the round immediately for defenders.
    public void NotifyDefuseCancelled()
    {
        if (State == RoundState.Ended) return;

        // Only act if a defuse was actually in progress to avoid a stale call triggering an unintended end.
        if (!_defuseActive)
        {
            Debug.LogWarning("[RoundManager] NotifyDefuseCancelled called but no defuse was in progress. Ignored.");
            return;
        }

        _defuseActive = false;
        Debug.Log("[RoundManager] Defuse cancelled.");

        // If we are in Overtime it means the timer already hit zero and we were waiting on this defuse.
        // Cancelling it now means there is nothing left to save the attackers, so defenders win immediately.
        if (State == RoundState.Overtime)
        {
            Debug.Log("[RoundManager] Defuse cancelled during Overtime. Defenders win.");
            EndRound(RoundOutcome.DefendersWin);
        }
    }

    // Use this to force the round to end from any outside system, for example when all attackers are eliminated.
    // Calling this before StartRound is a misuse and will be rejected.
    public void EndRound(RoundOutcome outcome)
    {
        if (State == RoundState.Initialising)
        {
            Debug.LogWarning("[RoundManager] EndRound called before the round started. Ignored.");
            return;
        }

        if (State == RoundState.Ended)
        {
            Debug.LogWarning("[RoundManager] EndRound called but the round is already over. Ignored.");
            return;
        }

        if (outcome == RoundOutcome.None)
        {
            Debug.LogWarning("[RoundManager] EndRound called with RoundOutcome.None. Ignored.");
            return;
        }

        ClearPendingTimerExpiryResolution();
        _defuseActive = false;
        Outcome = outcome;
        SetState(RoundState.Ended);
        Debug.Log($"[RoundManager] Round ended. Outcome: {outcome}");
        OnRoundEnded?.Invoke(outcome);
    }

    private void QueueTimerExpiryResolution()
    {
        if (_timerExpiryPending) return;

        _timerExpiryPending = true;
        _timerExpiryCoroutine = StartCoroutine(ResolveTimerExpiryAtEndOfFrame());
    }

    private IEnumerator ResolveTimerExpiryAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        _timerExpiryCoroutine = null;

        if (!_timerExpiryPending || State != RoundState.Active)
        {
            yield break;
        }

        _timerExpiryPending = false;
        HandleTimerExpiry();
    }

    private void ClearPendingTimerExpiryResolution()
    {
        _timerExpiryPending = false;

        if (_timerExpiryCoroutine == null) return;

        StopCoroutine(_timerExpiryCoroutine);
        _timerExpiryCoroutine = null;
    }

    private void HandleTimerExpiry()
    {
        if (_defuseActive)
        {
            // The timer expired and a defuse is still active at the end of the frame.
            // We move to Overtime and wait for the defuse to either complete or get cancelled.
            SetState(RoundState.Overtime);
            Debug.Log("[RoundManager] Timer expired during an active defuse. Entering Overtime.");
        }
        else
        {
            // Nothing is happening to stop defenders from winning, so end the round now.
            Debug.Log("[RoundManager] Timer expired with no defuse in progress. Defenders win.");
            EndRound(RoundOutcome.DefendersWin);
        }
    }

    private void SetState(RoundState newState)
    {
        State = newState;
        OnRoundStateChanged?.Invoke(newState);
    }
}
