using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [Header("Shooting")]
    [Tooltip("The input action used for shooting.")]
    public InputActionReference shootAction;

    [Tooltip("The projectile prefab to spawn.")]
    public GameObject projectilePrefab;

    [Tooltip("The position where projectiles are spawned.")]
    public Transform firePoint;

    [Tooltip("The speed of the projectile.")]
    public float projectileSpeed = 10f;

    [Tooltip("Time between shots while holding the button.")]
    public float fireInterval = 0.2f;

    private float nextFireTime;

    private void OnEnable()
    {
        shootAction.action.Enable();
    }

    private void Update()
    {
        // IsPressed allows continuous shooting while holding the button
        if (shootAction.action.IsPressed() && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireInterval;
        }
    }

    private void Shoot()
    {
        SoundEffects.Play(SoundEffects.Sfx.Shoot);
        Vector2 shootDirection = transform.up;

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            transform.rotation
        );

        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();

        if (projectileRb != null)
        {
            projectileRb.linearVelocity = shootDirection * projectileSpeed;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D projectileCollider = projectile.GetComponent<Collider2D>();

        if (playerCollider != null && projectileCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, projectileCollider);
        }
    }

    private void OnDisable()
    {
        shootAction.action.Disable();
    }
}