using System;
using UnityEngine;

public class AmmoSystem : MonoBehaviour
{
    [Header("Magazine Settings")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int maxReserveAmmo = 120;

    [Header("Current Ammo (Debug)")]
    [SerializeField] private int currentMagazine;
    [SerializeField] private int currentReserveAmmo;

    public event Action<int, int> onAmmoChanged;

    private void Awake()
    {
        currentMagazine = magazineSize;
        currentReserveAmmo = maxReserveAmmo;
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

    public int GetCurrentMagazine() => currentMagazine;

    public int GetCurrentReserveAmmo() => currentReserveAmmo;
}