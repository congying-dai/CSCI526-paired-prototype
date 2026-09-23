using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Maximum time before the projectile destroys itself.")]
    public float lifetime = 3f;

    private void Start()
    {
        // Destroy the projectile if it does not hit anything
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy the projectile when it hits something
        Destroy(gameObject);
    }
}