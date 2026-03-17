using UnityEngine;
using Core.Systems;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private TeamSide teamSide;
    public TeamSide TeamSide => teamSide;

    private void OnDrawGizmos()
    {
        Gizmos.color = teamSide == TeamSide.Attacker ? Color.red : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.4f);
        Gizmos.DrawRay(transform.position, transform.forward * 1f);
    }
}