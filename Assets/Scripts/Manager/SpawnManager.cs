using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Systems;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> spawnPoints = new();
    [SerializeField] private float minimumSpawnSeparation = 1f;

    public bool TryPlaceAtSpawn(Transform target, TeamSide side)
    {
        if (target == null)
        {
            Debug.LogWarning("[SpawnManager] Cannot place a null target.");
            return false;
        }

        SpawnPoint spawnPoint = GetSpawnPoint(side);

        if (spawnPoint == null)
            return false;

        target.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
        return true;
    }

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
}
