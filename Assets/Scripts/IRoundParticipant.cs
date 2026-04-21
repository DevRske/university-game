using Core.Systems;
using UnityEngine;

public interface IRoundParticipant
{
    TeamSide TeamSide { get; }
    bool IsAlive { get; }

    void ResetForRound();
    void Eliminate();
    void SpawnAt(SpawnPoint spawnPoint);
}