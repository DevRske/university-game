// WallHealthTests.cs
// Tests for: WallHealth component (Assets/Scripts/Environment/Wall/WallHealth.cs)
//
// What these tests cover (SCRUM-70 success criteria #5):
//   - PaperSoft walls take damage and lose health correctly
//   - PaperSoft walls fire OnDestroyed and stop responding when health hits zero
//   - StructuralSoft, HardWall and Reinforced walls are completely immune to bullets
//
// Pattern: a BoxCollider2D is added before WallHealth because WallHealth.Awake()
//          caches GetComponent<Collider2D>() and calls collider.enabled = false
//          on destruction — without a collider this throws NullReferenceException.
//          [TestCase] runs the immunity check for all three non-destructible types.

using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WallHealthTests
{
    private GameObject wallGO;
    private WallHealth wallHealth;

    [SetUp]
    public void SetUp()
    {
        wallGO = new GameObject("TestWall");

        // BoxCollider2D must exist before WallHealth.Awake() runs.
        // WallHealth caches it and disables it when the wall is destroyed.
        wallGO.AddComponent<BoxCollider2D>();

        wallHealth = wallGO.AddComponent<WallHealth>();
        // Awake() runs: currentHealth = maxHealth (100f).
        // wallType defaults to PaperSoft (first enum value = 0).
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(wallGO);
    }

    // Helper: change the private wallType field without modifying production code.
    private void SetWallType(WallType type)
    {
        typeof(WallHealth)
            .GetField("wallType", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(wallHealth, type);
    }

    // ── PaperSoft (the only destructible type) ────────────────────────────────

    [Test]
    public void TakeDamage_PaperSoft_ReducesHealth()
    {
        // wallType is PaperSoft by default — no reflection needed here.
        wallHealth.TakeDamage(40f);

        Assert.AreEqual(60f, wallHealth.CurrentHealth);
    }

    [Test]
    public void TakeDamage_PaperSoft_WhenHealthReachesZero_FiresOnDestroyedEvent()
    {
        bool destroyed = false;
        wallHealth.OnDestroyed += () => destroyed = true;

        wallHealth.TakeDamage(100f); // lethal hit

        Assert.IsTrue(destroyed,
            "OnDestroyed must fire when a PaperSoft wall is fully destroyed.");
    }

    [Test]
    public void TakeDamage_WhenAlreadyDestroyed_IsIgnored()
    {
        wallHealth.TakeDamage(100f); // destroy the wall
        wallHealth.TakeDamage(50f);  // subsequent damage must be ignored

        Assert.AreEqual(0f, wallHealth.CurrentHealth,
            "Health must not go below zero after destruction.");
    }

    // ── Non-destructible wall types ───────────────────────────────────────────
    // The same assertion is run once for each wall type listed below.

    [TestCase(WallType.StructuralSoft)]
    [TestCase(WallType.HardWall)]
    [TestCase(WallType.Reinforced)]
    public void TakeDamage_NonDestructibleWallType_DoesNotReduceHealth(WallType wallType)
    {
        SetWallType(wallType);

        wallHealth.TakeDamage(50f);

        Assert.AreEqual(100f, wallHealth.CurrentHealth,
            $"{wallType} walls are immune to bullets — health must remain at 100.");
    }
}
