using UnityEngine;

namespace TopDown.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Mover : MonoBehaviour
    {
        [SerializeField] private float MovementSpeed;
        [SerializeField] private float sprintMultiplier = 2f;
        private Rigidbody2D body;
        protected Vector3 currentInput;
        protected bool isSprinting;


        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }
        private void FixedUpdate()
        {
            float currentSpeed = isSprinting ? MovementSpeed * sprintMultiplier : MovementSpeed;
            body.linearVelocity = currentInput * currentSpeed;
            //Debug.Log("Sprint: " + isSprinting);
        }
    }
}
