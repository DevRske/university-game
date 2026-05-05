using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class RoundTimerHud : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private float bindRetryInterval = 0.5f;

    private const float MinimumBindRetryInterval = 0.1f;

    private RoundManager boundRoundManager;
    private Coroutine bindRetryCoroutine;

    private void OnEnable()
    {
        if (timerText == null)
        {
            Debug.LogWarning("[RoundTimerHud] Timer text reference is missing. Disabling HUD updater.", this);
            enabled = false;
            return;
        }

        BindToResolvedRoundManager();

        if (boundRoundManager == null)
        {
            StartBindRetry();
        }
    }

    private void OnDisable()
    {
        StopBindRetry();
        UnbindCurrentRoundManager();
    }

    private void BindToResolvedRoundManager()
    {
        RoundManager resolvedRoundManager = ResolveRoundManager();

        if (resolvedRoundManager == boundRoundManager)
        {
            if (resolvedRoundManager != null)
            {
                UpdateTimerText(resolvedRoundManager.TimeRemaining);
            }

            return;
        }

        UnbindCurrentRoundManager();

        if (resolvedRoundManager == null)
        {
            return;
        }

        boundRoundManager = resolvedRoundManager;
        boundRoundManager.OnTimerTick += HandleTimerTick;
        UpdateTimerText(boundRoundManager.TimeRemaining);
    }

    private RoundManager ResolveRoundManager()
    {
        if (roundManager != null)
        {
            return roundManager;
        }

        return Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);
    }

    private void HandleTimerTick(float timeRemaining)
    {
        UpdateTimerText(timeRemaining);
    }

    private void UpdateTimerText(float timeRemaining)
    {
        float clampedTime = Mathf.Max(timeRemaining, 0f);
        int minutes = Mathf.FloorToInt(clampedTime / 60f);
        int seconds = Mathf.FloorToInt(clampedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UnbindCurrentRoundManager()
    {
        if (boundRoundManager == null)
        {
            return;
        }

        boundRoundManager.OnTimerTick -= HandleTimerTick;
        boundRoundManager = null;
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

        while (boundRoundManager == null)
        {
            yield return wait;
            BindToResolvedRoundManager();
        }

        bindRetryCoroutine = null;
    }
}
