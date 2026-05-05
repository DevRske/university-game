using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Systems;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> spawnPoints = new();
    [SerializeField] private float minimumSpawnSeparation = 1f;

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

    public void RespawnParticipants(IEnumerable<RoundParticipant> participants)
    {
        if (participants == null)
            return;

        List<RoundParticipant> participantsList = participants.Where(p => p != null).ToList();
        if (participantsList.Count == 0)
            return;

        List<SpawnPoint> availableSpawns = new(spawnPoints);

        foreach (RoundParticipant participant in participantsList)
        {
            SpawnPoint spawnPoint = GetSpawnPointForParticipant(participant, availableSpawns);
            if (spawnPoint == null)
                continue;

            participant.transform.position = spawnPoint.transform.position;
            participant.transform.rotation = spawnPoint.transform.rotation;
        }
    }

    private SpawnPoint GetSpawnPointForParticipant(RoundParticipant participant, List<SpawnPoint> availableSpawns)
    {
        if (participant == null)
            return null;

        List<SpawnPoint> matching = availableSpawns
            .Where(sp => sp != null && sp.TeamSide == participant.TeamSide)
            .ToList();

        if (matching.Count == 0)
        {
            matching = spawnPoints
                .Where(sp => sp != null && sp.TeamSide == participant.TeamSide)
                .ToList();
        }

        if (matching.Count == 0)
        {
            Debug.LogWarning($"[SpawnManager] No spawn points found for {participant.TeamSide}");
            return null;
        }

        SpawnPoint chosen = matching[Random.Range(0, matching.Count)];
        availableSpawns.Remove(chosen);

        if (minimumSpawnSeparation > 0f)
        {
            availableSpawns.RemoveAll(sp => sp != null && sp.TeamSide == participant.TeamSide &&
                                            Vector3.Distance(sp.transform.position, chosen.transform.position) < minimumSpawnSeparation);
        }

        return chosen;
    }
}