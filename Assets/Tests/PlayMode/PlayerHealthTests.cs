// PlayerHealthTests.cs
// Tests for: PlayerHealth component (Assets/Scripts/Player/PlayerHealth.cs)
//
// What these tests cover (SCRUM-70 success criteria #2):
//   - Damage correctly reduces health
//   - Health cannot go below zero
//   - A player who takes lethal damage is marked as Eliminated
//   - An eliminated player ignores further damage
//   - The onHealthChanged event fires when damage is taken
//   - Parametrised damage values all produce the correct result
//
// Pattern: [SetUp] creates a fresh player before every test.
//          [TearDown] destroys it afterwards so tests do not interfere with each other.
//          [TestCase] runs the same test body with different input values automatically.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Core.Systems;

public class PlayerHealthTests
{
    private GameObject playerGO;
    private PlayerHealth playerHealth;

    // Runs before EVERY test in this class.
    // Creates a clean player so each test starts from the same known state.
    [SetUp]
    public void SetUp()
    {
        playerGO    = new GameObject("TestPlayer");
        playerHealth = playerGO.AddComponent<PlayerHealth>();
        // Awake() is called immediately by AddComponent in Play Mode.
        // After Awake: currentHealth = maxHealth = 100f, State = Active.
    }

    // Runs after EVERY test — destroys the player to prevent test pollution.
    [TearDown]
    public void TearDown()
    {
        Object.Destroy(playerGO);
    }

    // ── Basic damage ──────────────────────────────────────────────────────────

    [Test]
    public void TakeDamage_ReducesHealthByTheCorrectAmount()
    {
        playerHealth.TakeDamage(25f);

        Assert.AreEqual(75f, playerHealth.CurrentHealth);
    }

    [Test]
    public void TakeDamage_CannotReduceHealthBelowZero()
    {
        // Dealing more damage than remaining health must clamp to zero.
        playerHealth.TakeDamage(999f);

        Assert.AreEqual(0f, playerHealth.CurrentHealth);
    }

    // ── Elimination ───────────────────────────────────────────────────────────

    [Test]
    public void TakeDamage_WhenHealthReachesZero_SetsStateToEliminated()
    {
        playerHealth.TakeDamage(100f);

        Assert.AreEqual(PlayerState.Eliminated, playerHealth.State);
    }

    [Test]
    public void TakeDamage_WhenAlreadyEliminated_IsIgnored()
    {
        playerHealth.TakeDamage(100f);   // eliminate the player
        playerHealth.TakeDamage(50f);    // this second call must be silently ignored

        // Health was already at zero — must stay at zero, not go to -50.
        Assert.AreEqual(0f, playerHealth.CurrentHealth);
    }

    // ── Event ─────────────────────────────────────────────────────────────────

    [Test]
    public void TakeDamage_FiresOnHealthChangedEvent()
    {
        bool eventFired = false;

        // Subscribe a lambda that sets our flag when the event fires.
        playerHealth.onHealthChanged += (current, max) => eventFired = true;

        playerHealth.TakeDamage(10f);

        Assert.IsTrue(eventFired,
            "onHealthChanged must fire so the HUD and other listeners can update.");
    }

    // ── Parametrised — one test body, many input sets ─────────────────────────
    // NUnit runs this test once for each [TestCase] with the given arguments.

    [TestCase(25f,  75f)]
    [TestCase(50f,  50f)]
    [TestCase(100f,  0f)]
    public void TakeDamage_VariousAmounts_ProduceCorrectRemainingHealth(
        float damage, float expectedHealth)
    {
        playerHealth.TakeDamage(damage);

        Assert.AreEqual(expectedHealth, playerHealth.CurrentHealth,
            $"Expected {expectedHealth} HP after {damage} damage.");
    }
}
