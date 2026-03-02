using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float spawnOffset = 1f;

    private void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }
    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab is not assigned in WeaponController!");
            return;
        }

        Vector3 spawnPosition = transform.position + transform.up * spawnOffset;
        Instantiate(projectilePrefab, spawnPosition, transform.rotation);
    }
}
