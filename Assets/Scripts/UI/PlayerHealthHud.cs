using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealthHud : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private float bindRetryInterval = 0.5f;

    private const float MinimumBindRetryInterval = 0.1f;

    private PlayerHealth explicitPlayerHealth;
    private PlayerHealth boundPlayerHealth;
    private Coroutine bindRetryCoroutine;

    private void OnEnable()
    {
        if (healthText == null)
        {
            Debug.LogWarning("[PlayerHealthHud] Health text reference is missing. Disabling HUD updater.", this);
            enabled = false;
            return;
        }

        BindToResolvedPlayerHealth();

        if (boundPlayerHealth == null)
        {
            StartBindRetry();
        }
    }

    private void OnDisable()
    {
        StopBindRetry();
        UnbindCurrentPlayerHealth();
    }

    public void SetPlayerHealth(PlayerHealth playerHealth)
    {
        explicitPlayerHealth = playerHealth;

        if (!isActiveAndEnabled)
        {
            return;
        }

        StopBindRetry();
        BindToResolvedPlayerHealth();

        if (boundPlayerHealth == null)
        {
            StartBindRetry();
        }
    }

    private void BindToResolvedPlayerHealth()
    {
        PlayerHealth resolvedPlayerHealth = ResolvePlayerHealth();

        if (resolvedPlayerHealth == boundPlayerHealth)
        {
            if (resolvedPlayerHealth != null)
            {
                UpdateHealthText(resolvedPlayerHealth.CurrentHealth);
            }

            return;
        }

        UnbindCurrentPlayerHealth();

        if (resolvedPlayerHealth == null)
        {
            return;
        }

        boundPlayerHealth = resolvedPlayerHealth;
        boundPlayerHealth.onHealthChanged += HandleHealthChanged;
        UpdateHealthText(boundPlayerHealth.CurrentHealth);
    }

    private PlayerHealth ResolvePlayerHealth()
    {
        if (explicitPlayerHealth != null)
        {
            return explicitPlayerHealth;
        }

        GameObject playerRoot = PlayerHudTargetResolver.ResolveLocalPlayerRoot();
        return playerRoot != null ? playerRoot.GetComponent<PlayerHealth>() : null;
    }

    private void HandleHealthChanged(float currentHealth, float _)
    {
        UpdateHealthText(currentHealth);
    }

    private void UpdateHealthText(float currentHealth)
    {
        healthText.text = Mathf.RoundToInt(currentHealth).ToString();
    }

    private void UnbindCurrentPlayerHealth()
    {
        if (boundPlayerHealth == null)
        {
            return;
        }

        boundPlayerHealth.onHealthChanged -= HandleHealthChanged;
        boundPlayerHealth = null;
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

        while (boundPlayerHealth == null)
        {
            yield return wait;
            BindToResolvedPlayerHealth();
        }

        bindRetryCoroutine = null;
    }
}
