using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Systems;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> spawnPoints = new();

    public SpawnPoint GetSpawnPoint(TeamSide side)
    {
        List<SpawnPoint> matching = spawnPoints
            .Where(sp => sp.TeamSide == side)
            .ToList();

        if (matching.Count == 0)
        {
            Debug.LogWarning($"[SpawnManager] No spawn points found for {side}");
            return null;
        }

        return matching[Random.Range(0, matching.Count)];
    }
    public void SpawnParticipant(IRoundParticipant participant)
    {
        SpawnPoint spawnPoint = GetSpawnPoint(participant.TeamSide);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"[SpawnManager] No spawn point found for {participant.TeamSide}");
            return;
        }

        participant.SpawnAt(spawnPoint);
    }
}