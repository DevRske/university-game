using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Core.Systems;

public class AmmoSystem : MonoBehaviour
{
    [Header("Magazine Settings")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int maxReserveAmmo = 120;

    [Header("Reload Settings")]
    [SerializeField] private Key reloadKey = Key.R;

    [Header("Current Ammo (Debug)")]
    [SerializeField] private int currentMagazine;
    [SerializeField] private int currentReserveAmmo;

    private PlayerHealth playerHealth;

    public event Action<int, int> onAmmoChanged;
    public event Action onReload;

    private void Awake()
    {
        currentMagazine = magazineSize;
        currentReserveAmmo = maxReserveAmmo;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (Keyboard.current[reloadKey].wasPressedThisFrame)
            TryReload();
    }

    public bool CanFire()
    {
        return currentMagazine > 0;
    }

    public void ConsumeAmmo()
    {
        if (currentMagazine > 0)
        {
            currentMagazine--;
            onAmmoChanged?.Invoke(currentMagazine, currentReserveAmmo);
        }
    }

    public void TryReload()
    {
        if (playerHealth != null && playerHealth.State == PlayerState.Eliminated)
            return;

        if (currentMagazine == magazineSize)
            return;

        if (currentReserveAmmo == 0)
            return;

        int ammoNeeded = magazineSize - currentMagazine;
        int ammoToTransfer = Mathf.Min(ammoNeeded, currentReserveAmmo);

        currentMagazine += ammoToTransfer;
        currentReserveAmmo -= ammoToTransfer;

        onAmmoChanged?.Invoke(currentMagazine, currentReserveAmmo);
        onReload?.Invoke();
    }

    public int GetCurrentMagazine() => currentMagazine;
    public int GetCurrentReserveAmmo() => currentReserveAmmo;
    public int GetMagazineSize() => magazineSize;
}