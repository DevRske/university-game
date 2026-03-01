using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();

        if (body != null)
            body.gravityScale = 0f;
    }

    private void Start()
    {
        body.linearVelocity = transform.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Solid"))
            Destroy(gameObject);
    }
}
