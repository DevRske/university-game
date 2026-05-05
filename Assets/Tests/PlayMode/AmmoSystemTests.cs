// AmmoSystemTests.cs
// Tests for: AmmoSystem component (Assets/Scripts/Weapon/AmmoCounting.cs)
//
// What these tests cover (SCRUM-70 success criteria #6):
//   - CanFire() returns true when the magazine has rounds
//   - CanFire() returns false when the magazine is empty
//   - ConsumeAmmo() decrements the magazine by exactly one
//   - ConsumeAmmo() fires the onAmmoChanged event so the HUD stays in sync
//   - TryReload() fills the magazine from reserve when magazine is not full
//   - TryReload() does nothing when the magazine is already full
//   - TryReload() does nothing when there is no reserve ammo left
//
// Pattern: reflection writes to private currentMagazine / currentReserveAmmo
//          to set up specific pre-conditions without adding test-only methods
//          to the production class.
//
// Default inspector values used throughout (matches AmmoSystem serialized fields):
//   magazineSize    = 30
//   maxReserveAmmo  = 120

using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class AmmoSystemTests
{
    // Mirror the default inspector values so test numbers are self-documenting.
    private const int DefaultMagazine = 30;
    private const int DefaultReserve  = 120;

    private GameObject playerGO;
    private AmmoSystem ammoSystem;

    [SetUp]
    public void SetUp()
    {
        playerGO  = new GameObject("TestPlayer");
        ammoSystem = playerGO.AddComponent<AmmoSystem>();
        // Awake() runs: currentMagazine = 30, currentReserveAmmo = 120.
        // playerHealth is null (no PlayerHealth on this GO) which is fine —
        // AmmoSystem null-checks it before using it.
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(playerGO);
    }

    // Helper: write directly to a private backing field via reflection.
    private void SetField(string fieldName, int value)
    {
        typeof(AmmoSystem)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(ammoSystem, value);
    }

    // ── CanFire ───────────────────────────────────────────────────────────────

    [Test]
    public void CanFire_WhenMagazineIsLoaded_ReturnsTrue()
    {
        // Magazine starts full — shooting should be allowed.
        Assert.IsTrue(ammoSystem.CanFire());
    }

    [Test]
    public void CanFire_WhenMagazineIsEmpty_ReturnsFalse()
    {
        SetField("currentMagazine", 0);

        Assert.IsFalse(ammoSystem.CanFire(),
            "An empty magazine must not allow the player to fire.");
    }

    // ── ConsumeAmmo ───────────────────────────────────────────────────────────

    [Test]
    public void ConsumeAmmo_ReducesMagazineByExactlyOne()
    {
        ammoSystem.ConsumeAmmo();

        Assert.AreEqual(DefaultMagazine - 1, ammoSystem.GetCurrentMagazine());
    }

    [Test]
    public void ConsumeAmmo_FiresOnAmmoChangedEvent()
    {
        bool eventFired = false;
        ammoSystem.onAmmoChanged += (mag, reserve) => eventFired = true;

        ammoSystem.ConsumeAmmo();

        Assert.IsTrue(eventFired,
            "onAmmoChanged must fire after each shot so the HUD reflects the new count.");
    }

    // ── TryReload ─────────────────────────────────────────────────────────────

    [Test]
    public void TryReload_WhenMagazineIsNotFull_FillsMagazineFromReserve()
    {
        SetField("currentMagazine", 0); // magazine is empty

        ammoSystem.TryReload();

        Assert.AreEqual(DefaultMagazine, ammoSystem.GetCurrentMagazine(),
            "Reload must restore the magazine to its full capacity.");
        Assert.AreEqual(DefaultReserve - DefaultMagazine, ammoSystem.GetCurrentReserveAmmo(),
            "Reserve must decrease by the number of rounds loaded into the magazine.");
    }

    [Test]
    public void TryReload_WhenMagazineIsAlreadyFull_DoesNotConsumeReserve()
    {
        // Magazine starts full (30 / 30) — reload must be silently skipped.
        int reserveBefore = ammoSystem.GetCurrentReserveAmmo();

        ammoSystem.TryReload();

        Assert.AreEqual(reserveBefore, ammoSystem.GetCurrentReserveAmmo(),
            "Reserve must not decrease when the magazine is already full.");
    }

    [Test]
    public void TryReload_WhenReserveIsEmpty_LeavesEmptyMagazineUnchanged()
    {
        SetField("currentMagazine",    0);
        SetField("currentReserveAmmo", 0);

        ammoSystem.TryReload();

        Assert.AreEqual(0, ammoSystem.GetCurrentMagazine(),
            "Magazine must remain at zero when there is no reserve ammo to draw from.");
    }
}
