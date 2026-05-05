using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Core.Systems;

// Attach this to every enemy prefab alongside PlayerHealth and RoundParticipant.
// The NavMeshAgent handles pathfinding through corridors automatically.
// The enemy always moves toward the player and shoots when close enough.
// Visual feedback mirrors PlayerShooting: LineRenderer for bullet trail,
// SpriteRenderer for muzzle flash. Set them up on the prefab the same way
// as they are on the player.
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(RoundParticipant))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Shooting")]
    [SerializeField] private float shootingRange = 6f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Visuals")]
    // Same setup as PlayerShooting — drag the LineRenderer component here.
    [SerializeField] private LineRenderer lineRenderer;
    // Same muzzle flash SpriteRenderer as the player — drag it here.
    [SerializeField] private SpriteRenderer muzzleFlash;
    [SerializeField] private SoundRippleEffect soundRipplePrefab;
    [SerializeField] private float lineDuration = 0.05f;

    [Header("Audio")]
    // Assign the AudioSource component and the same gunshot clip used by the player.
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gunshotClip;

    private NavMeshAgent agent;
    private PlayerHealth health;
    private float nextFireTime;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Awake()
    {
        // Disable visuals on awake — same timing as PlayerShooting.
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (muzzleFlash  != null) muzzleFlash.enabled  = false;
    }

    private void Start()
    {
        agent  = GetComponent<NavMeshAgent>();
        health = GetComponent<PlayerHealth>();

        // Required for 2D — stops the NavMeshAgent from rotating in 3D and
        // pushing the enemy off the Z = 0 plane.
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.speed          = moveSpeed;

        // Auto-find the player if no target was set manually in the Inspector.
        if (target == null)
        {
            PlayerShooting playerShooting = Object.FindFirstObjectByType<PlayerShooting>(FindObjectsInactive.Exclude);

            if (playerShooting != null)
                target = playerShooting.transform;
            else
                Debug.LogWarning($"[Enemy] {gameObject.name} could not find the player.");
        }
    }

    private void Update()
    {
        // Stop all behaviour once this enemy is eliminated.
        // PlayerHealth.OnDeath disables the GameObject, so this is a safety net
        // for the single frame between the kill shot and the disable.
        if (health.State == PlayerState.Eliminated) return;
        if (target == null) return;

        MoveTowardPlayer();
        FacePlayer();
        TryShoot();
    }

    private void MoveTowardPlayer()
    {
        // The NavMeshAgent recalculates the path every frame as the player moves,
        // so the enemy always finds a valid route through corridors.
        if (agent.isOnNavMesh)
            agent.SetDestination(target.position);
    }

    private void FacePlayer()
    {
        // Rotate the sprite toward the player independently of the NavMeshAgent.
        // Subtract 90 degrees to correct for sprites that face upward by default.
        Vector2 direction = (target.position - transform.position).normalized;

        if (direction == Vector2.zero) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void TryShoot()
    {
        // Use squared distance to avoid a sqrt call every frame.
        float distanceSqr = (target.position - transform.position).sqrMagnitude;

        if (distanceSqr > shootingRange * shootingRange) return;
        if (Time.time < nextFireTime) return;

        Shoot();
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireRate;

        Vector2 origin    = transform.position;
        Vector2 direction = ((Vector2)target.position - origin).normalized;

        // Offset the ray origin so it starts outside the enemy's own collider.
        // Without this the ray immediately hits the enemy itself, calls TakeDamage
        // on its own PlayerHealth, and kills it before it can shoot anyone.
        Vector2 rayOrigin = origin + direction * 0.6f;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, shootingRange, hitLayers);

        // Calculate the visual end point regardless of whether anything was hit.
        Vector2 endPoint = hit.collider != null
            ? hit.point
            : rayOrigin + direction * shootingRange;

        // Deal damage if we hit something damageable that isn't ourselves.
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
            Debug.Log($"[Enemy] {gameObject.name} shot {hit.collider.gameObject.name}.");
        }

        PlayGunshotAudio();
        SpawnSoundRipple(rayOrigin);
        StartCoroutine(ShowLineFlash(rayOrigin, endPoint));
        StartCoroutine(ShowMuzzleFlash(rayOrigin));
    }

    private void PlayGunshotAudio()
    {
        if (audioSource != null && gunshotClip != null)
            audioSource.PlayOneShot(gunshotClip);
    }

    private IEnumerator ShowLineFlash(Vector2 start, Vector2 end)
    {
        if (lineRenderer == null) yield break;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(lineDuration);

        lineRenderer.enabled = false;
    }

    private IEnumerator ShowMuzzleFlash(Vector2 barrelPosition)
    {
        if (muzzleFlash == null) yield break;

        muzzleFlash.transform.position = barrelPosition;
        muzzleFlash.enabled = true;

        yield return new WaitForSeconds(lineDuration);

        muzzleFlash.enabled = false;
    }

    private void SpawnSoundRipple(Vector2 position)
    {
        if (soundRipplePrefab == null) return;

        Instantiate(soundRipplePrefab, position, Quaternion.identity);
    }
}
