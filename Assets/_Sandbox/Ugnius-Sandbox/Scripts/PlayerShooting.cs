using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 20f;
    [Tooltip("Time in seconds between shots")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private Vector2 barrelOffset = new Vector2(0f, 1f);
    [SerializeField] private LayerMask hitLayers;

    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private SpriteRenderer muzzleFlash;
    [SerializeField] private float lineDuration = 0.05f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gunshotClip;

    private float nextFireTime = 0f;
    private Vector2 AimDirection
    {
        get
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            return (mouseWorld - (Vector2)transform.position).normalized;
        }
    }

    private void Awake()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
            Shoot();
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireRate;

        Vector2 origin = (Vector2)transform.position + (Vector2)transform.TransformDirection(barrelOffset);
        Vector2 direction = AimDirection;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, hitLayers);

        Vector2 hitPoint;

        if (hit.collider != null)
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = origin + direction * range;
        }

        // Deal damage if target implements IDamageable
        if (hit.collider != null)
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
        PlayGunshotAudio();
        StartCoroutine(ShowLineFlash(origin, hitPoint));
        StartCoroutine(ShowMuzzleFlash());
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

    private IEnumerator ShowMuzzleFlash()
    {
        if (muzzleFlash == null) yield break;

        muzzleFlash.enabled = true;

        yield return new WaitForSeconds(lineDuration);

        muzzleFlash.enabled = false;
    }

    private void PlayGunshotAudio()
    {
        if (audioSource != null && gunshotClip != null)
            audioSource.PlayOneShot(gunshotClip);
    }
}