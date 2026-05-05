using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class RoundRestartController : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private RoundEliminationTracker eliminationTracker;
    [SerializeField] private float restartDelaySeconds = 5f;

    private readonly List<RoundParticipant> cachedParticipants = new();
    private Coroutine restartCoroutine;

    private void Awake()
    {
        if (roundManager == null)
            roundManager = Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);

        if (spawnManager == null)
            spawnManager = Object.FindFirstObjectByType<SpawnManager>(FindObjectsInactive.Exclude);

        if (eliminationTracker == null)
            eliminationTracker = Object.FindFirstObjectByType<RoundEliminationTracker>(FindObjectsInactive.Exclude);

        CacheParticipants();
    }

    private void OnEnable()
    {
        if (roundManager != null)
            roundManager.OnRoundEnded += HandleRoundEnded;
    }

    private void OnDisable()
    {
        if (roundManager != null)
            roundManager.OnRoundEnded -= HandleRoundEnded;

        if (restartCoroutine != null)
        {
            StopCoroutine(restartCoroutine);
            restartCoroutine = null;
        }
    }

    public void RestartRound()
    {
        if (roundManager == null || spawnManager == null)
            return;

        if (!roundManager.CanRestart)
            return;

        CacheParticipants();
        RespawnParticipants();

        eliminationTracker?.RefreshParticipants();
        roundManager.RestartRound();
    }

    private void HandleRoundEnded(Core.Systems.RoundOutcome _)
    {
        if (restartCoroutine != null)
            StopCoroutine(restartCoroutine);

        restartCoroutine = StartCoroutine(RestartAfterDelay());
    }

    private System.Collections.IEnumerator RestartAfterDelay()
    {
        float delay = Mathf.Max(0f, restartDelaySeconds);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        RestartRound();
        restartCoroutine = null;
    }

    private void CacheParticipants()
    {
        cachedParticipants.Clear();
        cachedParticipants.AddRange(Object.FindObjectsByType<RoundParticipant>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void RespawnParticipants()
    {
        foreach (RoundParticipant participant in cachedParticipants.Where(p => p != null))
        {
            GameObject participantObject = participant.gameObject;
            if (!participantObject.activeSelf)
                participantObject.SetActive(true);

            PlayerHealth health = participant.GetComponent<PlayerHealth>();
            if (health != null)
                health.ResetHealth();
        }

        spawnManager.RespawnParticipants(cachedParticipants);
    }
}
