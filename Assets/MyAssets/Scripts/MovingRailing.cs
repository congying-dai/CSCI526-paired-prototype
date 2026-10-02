using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingRailing : MonoBehaviour
{
    [Tooltip("How far the railing travels from its start position (world units).")]
    public float travelDistance = 3f;

    [Tooltip("Movement speed in units per second.")]
    public float speed = 2f;

    [Tooltip("Offsets the movement cycle (0-1) so several railings don't move in sync.")]
    [Range(0f, 1f)] public float phase = 0f;

    private Rigidbody2D rb;
    private Vector2 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        startPosition = rb.position;
    }

    private void FixedUpdate()
    {
        float cycle = Mathf.Max(travelDistance * 2f, 0.0001f);
        float offset = Mathf.PingPong(Time.time * speed + phase * cycle, cycle) - travelDistance;
        rb.MovePosition(startPosition + Vector2.up * offset);
    }
}
