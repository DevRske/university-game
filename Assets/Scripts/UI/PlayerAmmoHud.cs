using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAmmoHud : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private float bindRetryInterval = 0.5f;

    private const float MinimumBindRetryInterval = 0.1f;

    private AmmoSystem boundAmmoSystem;
    private Coroutine bindRetryCoroutine;

    private void OnEnable()
    {
        if (ammoText == null)
        {
            Debug.LogWarning("[PlayerAmmoHud] Ammo text reference is missing. Disabling HUD updater.", this);
            enabled = false;
            return;
        }

        BindToResolvedAmmoSystem();

        if (boundAmmoSystem == null)
        {
            StartBindRetry();
        }
    }

    private void OnDisable()
    {
        StopBindRetry();
        UnbindCurrentAmmoSystem();
    }

    private void BindToResolvedAmmoSystem()
    {
        GameObject playerRoot = PlayerHudTargetResolver.ResolveLocalPlayerRoot();
        AmmoSystem resolvedAmmoSystem = playerRoot != null ? playerRoot.GetComponent<AmmoSystem>() : null;

        if (resolvedAmmoSystem == boundAmmoSystem)
        {
            if (resolvedAmmoSystem != null)
            {
                UpdateAmmoText(resolvedAmmoSystem.GetCurrentMagazine(), resolvedAmmoSystem.GetCurrentReserveAmmo());
            }

            return;
        }

        UnbindCurrentAmmoSystem();

        if (resolvedAmmoSystem == null)
        {
            return;
        }

        boundAmmoSystem = resolvedAmmoSystem;
        boundAmmoSystem.onAmmoChanged += HandleAmmoChanged;
        UpdateAmmoText(boundAmmoSystem.GetCurrentMagazine(), boundAmmoSystem.GetCurrentReserveAmmo());
    }

    private void HandleAmmoChanged(int currentMagazine, int currentReserveAmmo)
    {
        UpdateAmmoText(currentMagazine, currentReserveAmmo);
    }

    private void UpdateAmmoText(int currentMagazine, int currentReserveAmmo)
    {
        ammoText.text = $"{currentMagazine}/{currentReserveAmmo}";
    }

    private void UnbindCurrentAmmoSystem()
    {
        if (boundAmmoSystem == null)
        {
            return;
        }

        boundAmmoSystem.onAmmoChanged -= HandleAmmoChanged;
        boundAmmoSystem = null;
    }

    private void StartBindRetry()
    {
        if (bindRetryCoroutine != null)
        {
            return;
        }

        bindRetryCoroutine = StartCoroutine(RetryBindUntilResolved());
    }

    private void StopBindRetry()
    {
        if (bindRetryCoroutine == null)
        {
            return;
        }

        StopCoroutine(bindRetryCoroutine);
        bindRetryCoroutine = null;
    }

    private IEnumerator RetryBindUntilResolved()
    {
        float retryDelay = Mathf.Max(bindRetryInterval, MinimumBindRetryInterval);
        WaitForSeconds wait = new WaitForSeconds(retryDelay);

        while (boundAmmoSystem == null)
        {
            yield return wait;
            BindToResolvedAmmoSystem();
        }

        bindRetryCoroutine = null;
    }
}
