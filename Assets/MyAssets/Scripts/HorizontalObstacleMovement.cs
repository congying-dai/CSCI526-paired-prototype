using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HorizontalObstacleMovement : MonoBehaviour
{
    [Tooltip("Maximum distance moved to either side of the starting position.")]
    public float moveDistance = 3f;

    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float direction = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = rb.position;
    }

    private void FixedUpdate()
    {
        float leftLimit = startPosition.x - moveDistance;
        float rightLimit = startPosition.x + moveDistance;

        Vector2 nextPosition = rb.position;
        nextPosition.x += direction * moveSpeed * Time.fixedDeltaTime;

        if (nextPosition.x >= rightLimit)
        {
            nextPosition.x = rightLimit;
            direction = -1f;
        }
        else if (nextPosition.x <= leftLimit)
        {
            nextPosition.x = leftLimit;
            direction = 1f;
        }

        rb.MovePosition(nextPosition);
    }
}