using Core.Systems;
using UnityEngine;

public class RoundParticipant : MonoBehaviour, IRoundParticipant
{
    [SerializeField] private TeamSide teamSide;

    private bool isAlive = true;

    public TeamSide TeamSide => teamSide;
    public bool IsAlive => isAlive;

    public void ResetForRound()
    {
        isAlive = true;
        gameObject.SetActive(true);
    }

    public void Eliminate()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }

    public void SpawnAt(SpawnPoint spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("[RoundParticipant] Spawn point is null.");
            return;
        }

        transform.position = spawnPoint.transform.position;
        transform.rotation = spawnPoint.transform.rotation;

        ResetForRound();
    }
}