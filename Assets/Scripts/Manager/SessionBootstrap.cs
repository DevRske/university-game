using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Systems;

public class SessionBootstrap : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private TeamSide playerTeamSide = TeamSide.Attacker;

    [Header("Bots")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private TeamSide enemyTeamSide = TeamSide.Defender;
    [SerializeField] private List<Transform> botSpawnPoints = new();

    [Header("Hierarchy")]
    [SerializeField] private Transform spawnedActorsParent;

    private readonly List<RoundParticipant> participants = new();
    private bool hasBootstrapped;
    private bool setupFailed;

    private IEnumerator Start()
    {
        if (hasBootstrapped)
            yield break;

        hasBootstrapped = true;

        if (!HasRequiredReferences())
            yield break;

        GameObject player = SpawnPlayer();

        if (player == null)
            yield break;

        SpawnBots(player.transform);

        yield return null;

        if (!AreParticipantsReady())
            yield break;

        roundManager.StartRound();
    }

    private bool HasRequiredReferences()
    {
        if (roundManager == null)
        {
            Debug.LogError("[SessionBootstrap] RoundManager is not assigned.", this);
            return false;
        }

        if (spawnManager == null)
        {
            Debug.LogError("[SessionBootstrap] SpawnManager is not assigned.", this);
            return false;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[SessionBootstrap] Player prefab is not assigned.", this);
            return false;
        }

        if (enemyPrefab == null && botSpawnPoints.Count > 0)
        {
            Debug.LogError("[SessionBootstrap] Enemy prefab is not assigned but bot spawn points are configured.", this);
            return false;
        }

        return true;
    }

    private GameObject SpawnPlayer()
    {
        GameObject player = Instantiate(playerPrefab, spawnedActorsParent);
        player.name = playerPrefab.name;

        if (!spawnManager.TryPlaceAtSpawn(player.transform, playerTeamSide))
        {
            Debug.LogError($"[SessionBootstrap] Could not place player at a {playerTeamSide} spawn.", this);
            Destroy(player);
            return null;
        }

        if (!TryRegisterParticipant(player, playerTeamSide))
        {
            Destroy(player);
            return null;
        }

        if (cameraFollow != null)
            cameraFollow.SetTarget(player.transform);

        return player;
    }

    private void SpawnBots(Transform playerTarget)
    {
        for (int i = 0; i < botSpawnPoints.Count; i++)
        {
            Transform spawnPoint = botSpawnPoints[i];

            if (spawnPoint == null)
            {
                Debug.LogError($"[SessionBootstrap] Bot spawn point {i} is not assigned.", this);
                setupFailed = true;
                continue;
            }

            Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, spawnedActorsParent);
            enemy.name = $"{enemyPrefab.name} {i + 1}";
            enemy.SetTarget(playerTarget);

            if (!TryRegisterParticipant(enemy.gameObject, enemyTeamSide))
            {
                setupFailed = true;
                Destroy(enemy.gameObject);
            }
        }
    }

    private bool TryRegisterParticipant(GameObject actor, TeamSide side)
    {
        if (actor == null)
            return false;

        RoundParticipant participant = actor.GetComponent<RoundParticipant>();

        if (participant == null)
        {
            Debug.LogError($"[SessionBootstrap] {actor.name} is missing a RoundParticipant component.", actor);
            return false;
        }

        participant.SetTeamSide(side);
        participants.Add(participant);
        return true;
    }

    private bool AreParticipantsReady()
    {
        if (setupFailed)
            return false;

        for (int i = 0; i < participants.Count; i++)
        {
            RoundParticipant participant = participants[i];

            if (participant == null || !participant.gameObject.activeInHierarchy)
            {
                Debug.LogError("[SessionBootstrap] A participant was not ready before round start.", this);
                return false;
            }
        }

        return participants.Count > 0;
    }
}
