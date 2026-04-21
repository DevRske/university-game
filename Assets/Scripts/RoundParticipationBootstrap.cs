using UnityEngine;

public class RoundParticipationBootstrap : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;

    private void Start()
    {
        RoundParticipant[] participants = FindObjectsByType<RoundParticipant>(FindObjectsSortMode.None);

        foreach (RoundParticipant participant in participants)
        {
            spawnManager.SpawnParticipant(participant);
        }
    }
}