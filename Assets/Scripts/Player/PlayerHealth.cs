using UnityEngine;
using System;
using Core.Systems;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public PlayerState State { get; private set; } = PlayerState.Active;

    public event Action<float, float> onHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (State == PlayerState.Eliminated)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0f); // Prevents from health going below zero
        Debug.Log($"[PlayerHealth] {gameObject.name} took {amount} damage. Current Health: {currentHealth}");

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        State = PlayerState.Eliminated;
        Debug.Log($"[PlayerHealth] {gameObject.name} has been eliminated.");
        gameObject.SetActive(false);
    }
}
