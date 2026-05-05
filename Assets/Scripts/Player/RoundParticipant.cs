using UnityEngine;
using Core.Systems;

[DisallowMultipleComponent]
public class RoundParticipant : MonoBehaviour
{
    [SerializeField] private TeamSide teamSide = TeamSide.Attacker;

    private PlayerHealth playerHealth;

    public TeamSide TeamSide => teamSide;

    public bool IsEliminated => playerHealth != null && playerHealth.State == PlayerState.Eliminated;

    public bool IsEligibleForDefuse => gameObject.activeInHierarchy && !IsEliminated;

    public void SetTeamSide(TeamSide side)
    {
        teamSide = side;
    }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }
}
