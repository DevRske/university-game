using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float sprintMultiplier = 2f;
    private Rigidbody2D body;
    private Vector2 currentInput;
    private bool isSprinting;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float currentSpeed = isSprinting ? movementSpeed * sprintMultiplier : movementSpeed;
        body.linearVelocity = currentInput * currentSpeed;
    }

    private void OnMove(InputValue value)
    {
        currentInput = value.Get<Vector2>().normalized;
    }

    private void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }
}