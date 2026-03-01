using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float spawnOffset = 1f;

    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }
    void Shoot()
    {
        Vector3 spawnPosition = transform.position + transform.up * spawnOffset;
        Instantiate(projectilePrefab, spawnPosition, transform.rotation);
    }
}
