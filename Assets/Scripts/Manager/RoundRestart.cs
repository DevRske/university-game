using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Systems;

[DisallowMultipleComponent]
public class RoundRestart : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private float restartDelay = 5f;

    private Coroutine restartCoroutine;

    private void Awake()
    {
        if (roundManager == null)
            roundManager = UnityEngine.Object.FindFirstObjectByType<RoundManager>(FindObjectsInactive.Exclude);
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
    }

    private void HandleRoundEnded(RoundOutcome _)
    {
        if (restartCoroutine != null)
            return;

        restartCoroutine = StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
