// BombSiteDefuseTests.cs
// Tests for: BombSiteDefuseInteraction (Assets/Scripts/Environment/BombSite/BombSiteDefuseInteraction.cs)
//
// What these tests cover (SCRUM-70 success criteria #4):
//   - A Defender can NEVER start a defuse (team side check)
//   - An eliminated Attacker cannot start a defuse (eligibility check)
//   - An Attacker outside interaction range cannot start a defuse (distance check)
//   - An eligible Attacker inside range CAN start a defuse (happy path)
//
// Component dependency chain:
//   BombSiteDefuseInteraction  requires  BombSite  which requires  BoxCollider2D
//   RoundParticipant reads PlayerHealth for IsEliminated — PlayerHealth must be on the same GO
//
// Pattern: CreateParticipant() is a shared factory so each test describes only
//          the variation that matters (team side, position, alive/eliminated).

using NUnit.Framework;
using UnityEngine;
using Core.Systems;

public class BombSiteDefuseTests
{
    private GameObject siteGO;
    private BombSiteDefuseInteraction defuseInteraction;

    [SetUp]
    public void SetUp()
    {
        // Build the bomb site GameObject with all required components.
        // Order matters: BoxCollider2D → BombSite → BombSiteDefuseInteraction
        siteGO = new GameObject("TestBombSite");
        siteGO.AddComponent<BoxCollider2D>();
        siteGO.AddComponent<BombSite>();
        defuseInteraction = siteGO.AddComponent<BombSiteDefuseInteraction>();
        // siteGO is at (0,0,0) — participants created at origin are in range by default.
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(siteGO);
    }

    // Factory method: creates a RoundParticipant with the given team side.
    // Also adds PlayerHealth, which RoundParticipant needs to evaluate IsEliminated.
    // RoundParticipant defaults to TeamSide.Attacker, so SetTeamSide() is only
    // meaningful when Defender is needed — but it is called in all cases for clarity.
    private RoundParticipant CreateParticipant(TeamSide side)
    {
        var go          = new GameObject("Participant");
        go.AddComponent<PlayerHealth>();
        var participant = go.AddComponent<RoundParticipant>();

        // SetTeamSide() is a public method on RoundParticipant — no reflection needed.
        participant.SetTeamSide(side);

        return participant;
    }

    // ── Eligibility checks ────────────────────────────────────────────────────

    [Test]
    public void TryStartDefuse_AsDefender_ReturnsFalse()
    {
        RoundParticipant defender = CreateParticipant(TeamSide.Defender);

        bool result = defuseInteraction.TryStartDefuse(defender);

        Assert.IsFalse(result,
            "Defenders must never be allowed to defuse — only Attackers can.");

        Object.Destroy(defender.gameObject);
    }

    [Test]
    public void TryStartDefuse_AsEliminatedAttacker_ReturnsFalse()
    {
        RoundParticipant attacker = CreateParticipant(TeamSide.Attacker);

        // Eliminate the attacker. PlayerHealth.OnDeath() sets the GO inactive.
        attacker.GetComponent<PlayerHealth>().TakeDamage(100f);

        bool result = defuseInteraction.TryStartDefuse(attacker);

        Assert.IsFalse(result,
            "An eliminated Attacker is no longer eligible and must not be able to defuse.");

        // GameObject is inactive after elimination — use DestroyImmediate to clean up.
        Object.DestroyImmediate(attacker.gameObject);
    }

    [Test]
    public void TryStartDefuse_AsAttacker_WhenOutsideInteractionRange_ReturnsFalse()
    {
        RoundParticipant attacker = CreateParticipant(TeamSide.Attacker);

        // Move the participant far outside the default interaction range (2 units).
        attacker.transform.position = new Vector3(100f, 100f, 0f);

        bool result = defuseInteraction.TryStartDefuse(attacker);

        Assert.IsFalse(result,
            "An Attacker who is too far from the bomb site must not be able to defuse.");

        Object.Destroy(attacker.gameObject);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Test]
    public void TryStartDefuse_AsEligibleAttacker_WhenInRange_ReturnsTrue()
    {
        // Participant is at (0,0,0) — same position as the site — so in range.
        RoundParticipant attacker = CreateParticipant(TeamSide.Attacker);

        bool result = defuseInteraction.TryStartDefuse(attacker);

        Assert.IsTrue(result,
            "A living Attacker within interaction range must be able to start a defuse.");

        Object.Destroy(attacker.gameObject);
    }
}
