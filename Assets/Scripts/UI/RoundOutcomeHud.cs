using System.Collections;
using TMPro;
using UnityEngine;
using Core.Systems;

[DisallowMultipleComponent]
public class RoundOutcomeHud : MonoBehaviour
{
    [SerializeField] private TMP_Text outcomeText;
    [SerializeField] private GameObject outcomePanel;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private float bindRetryInterval = 0.5f;

    private const float MinimumBindRetryInterval = 0.1f;

    private RoundManager boundRoundManager;
    private Coroutine bindRetryCoroutine;

    private void OnEnable()
    {
        if (outcomeText == null)
        {
            Debug.LogWarning("[RoundOutcomeHud] Outcome text reference is missing. Disabling HUD updater.", this);
            enabled = false;
            return;
        }

        if (outcomePanel == null)
        {
            outcomePanel = outcomeText.gameObject;
        }

        outcomePanel.SetActive(false);

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
            return;
        }

        UnbindCurrentRoundManager();

        if (resolvedRoundManager == null)
        {
            return;
        }

        boundRoundManager = resolvedRoundManager;
        boundRoundManager.OnRoundEnded += HandleRoundEnded;
    }

    private RoundManager ResolveRoundManager()
    {
        if (roundManager != null)
        {
            return roundManager;
        }

        return Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);
    }

    private void HandleRoundEnded(RoundOutcome outcome)
    {
        outcomeText.text = GetOutcomeMessage(outcome);
        outcomePanel.SetActive(true);
    }

    private string GetOutcomeMessage(RoundOutcome outcome)
    {
        switch (outcome)
        {
            case RoundOutcome.AttackersWin:
                return "Attackers Win";
            case RoundOutcome.DefendersWin:
                return "Defenders Win";
            default:
                return "Round Ended";
        }
    }

    private void UnbindCurrentRoundManager()
    {
        if (boundRoundManager == null)
        {
            return;
        }

        boundRoundManager.OnRoundEnded -= HandleRoundEnded;
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
