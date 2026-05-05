// RoundManagerTests.cs
// Tests for: RoundManager component (Assets/Scripts/Manager/RoundManager.cs)
//
// What these tests cover (SCRUM-70 success criteria #3):
//   - StartRound transitions state to Active
//   - Calling StartRound twice does not break state
//   - EndRound sets the correct Outcome and state
//   - Guard conditions (before start, double end, None outcome) are enforced
//   - OnRoundEnded event fires with the correct outcome
//   - The timer, when it expires with no active defuse, ends the round for Defenders
//
// Pattern: reflection is used to set the private "roundDuration" field so the
//          timer test completes in 0.5 s instead of the default 120 s.
//          [UnityTest] + IEnumerator is used for the async timer test only.
//          All synchronous state-machine tests use plain [Test].

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core.Systems;

public class RoundManagerTests
{
    private GameObject managerGO;
    private RoundManager roundManager;

    [SetUp]
    public void SetUp()
    {
        managerGO    = new GameObject("TestRoundManager");
        roundManager = managerGO.AddComponent<RoundManager>();
        // Awake() runs: TimeRemaining = roundDuration (120f), State = Initialising.
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(managerGO);
    }

    // Helper: write to a private field via reflection so tests can control
    // internal values without changing the production class.
    private void SetPrivateField(string fieldName, object value)
    {
        typeof(RoundManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(roundManager, value);
    }

    // ── State transitions ─────────────────────────────────────────────────────

    [Test]
    public void StartRound_SetsStateToActive()
    {
        roundManager.StartRound();

        Assert.AreEqual(RoundState.Active, roundManager.State);
    }

    [Test]
    public void StartRound_WhenAlreadyActive_IsIgnored()
    {
        roundManager.StartRound();
        roundManager.StartRound(); // second call must be silently ignored

        // State must still be Active — not broken or reset.
        Assert.AreEqual(RoundState.Active, roundManager.State);
    }

    [Test]
    public void EndRound_SetsStateToEnded()
    {
        roundManager.StartRound();
        roundManager.EndRound(RoundOutcome.DefendersWin);

        Assert.AreEqual(RoundState.Ended, roundManager.State);
    }

    // ── Outcome ───────────────────────────────────────────────────────────────

    [Test]
    public void EndRound_WithDefendersWin_RecordsCorrectOutcome()
    {
        roundManager.StartRound();
        roundManager.EndRound(RoundOutcome.DefendersWin);

        Assert.AreEqual(RoundOutcome.DefendersWin, roundManager.Outcome);
    }

    [Test]
    public void EndRound_WithAttackersWin_RecordsCorrectOutcome()
    {
        roundManager.StartRound();
        roundManager.EndRound(RoundOutcome.AttackersWin);

        Assert.AreEqual(RoundOutcome.AttackersWin, roundManager.Outcome);
    }

    // ── Guard conditions ──────────────────────────────────────────────────────

    [Test]
    public void EndRound_CalledBeforeStartRound_IsIgnored()
    {
        // EndRound before StartRound is a misuse — state must stay Initialising.
        roundManager.EndRound(RoundOutcome.DefendersWin);

        Assert.AreEqual(RoundState.Initialising, roundManager.State);
    }

    [Test]
    public void EndRound_WhenAlreadyEnded_DoesNotOverwriteOutcome()
    {
        roundManager.StartRound();
        roundManager.EndRound(RoundOutcome.DefendersWin);
        roundManager.EndRound(RoundOutcome.AttackersWin); // must be ignored

        // The first outcome must be preserved.
        Assert.AreEqual(RoundOutcome.DefendersWin, roundManager.Outcome);
    }

    [Test]
    public void EndRound_WithNoneOutcome_IsIgnored()
    {
        roundManager.StartRound();
        roundManager.EndRound(RoundOutcome.None); // RoundOutcome.None is never a valid end state

        Assert.AreNotEqual(RoundState.Ended, roundManager.State);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Test]
    public void EndRound_FiresOnRoundEndedEvent_WithCorrectOutcome()
    {
        RoundOutcome captured = RoundOutcome.None;
        roundManager.OnRoundEnded += outcome => captured = outcome;

        roundManager.StartRound();
        roundManager.EndRound(RoundOutcome.AttackersWin);

        Assert.AreEqual(RoundOutcome.AttackersWin, captured,
            "OnRoundEnded must deliver the correct outcome to subscribers.");
    }

    // ── Timer (async — requires [UnityTest] + IEnumerator) ───────────────────
    // This test must yield real frames because RoundManager uses
    // WaitForEndOfFrame() internally to resolve timer expiry safely.

    [UnityTest]
    public IEnumerator Timer_WhenExpired_WithNoActiveDefuse_EndsRoundDefendersWin()
    {
        // Shorten the duration so the test finishes in < 1 s.
        SetPrivateField("roundDuration", 0.1f);

        roundManager.StartRound();

        // Wait long enough for the timer to expire AND for the end-of-frame
        // coroutine inside RoundManager to fire and call EndRound.
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(RoundState.Ended,            roundManager.State,   "State must be Ended after timer expires.");
        Assert.AreEqual(RoundOutcome.DefendersWin, roundManager.Outcome, "Defenders win when timer runs out with no defuse.");
    }
}
