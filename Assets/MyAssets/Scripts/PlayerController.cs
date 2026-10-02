using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f; // The speed at which the player moves
    public bool canMoveDiagonally = true; // Controls whether the player can move diagonally

    public InputActionReference moveAction;
    private float speedMultiplier = 1f;
    private float speedEffectEndTime;

    private Rigidbody2D rb; // Reference to the Rigidbody2D component attached to the player
    private Vector2 movement; // Stores the direction of player movement
    private bool isMovingHorizontally = true; // Flag to track if the player is moving horizontally

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (speedMultiplier != 1f && Time.time >= speedEffectEndTime)
        {
            speedMultiplier = 1f;
        }

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        if (canMoveDiagonally)
        {
            movement = new Vector2(horizontalInput, verticalInput);
            // Optionally rotate the player based on movement direction
            RotatePlayer(horizontalInput, verticalInput);
        }
        else
        {
            if (horizontalInput != 0)
            {
                isMovingHorizontally = true;
            }
            else if (verticalInput != 0)
            {
                isMovingHorizontally = false;
            }
            if (isMovingHorizontally)
            {
                movement = new Vector2(horizontalInput, 0);
                RotatePlayer(horizontalInput, 0);
            }
            else
            {
                movement = new Vector2(0, verticalInput);
                RotatePlayer(0, verticalInput);
            }
        }
    }

    void FixedUpdate()
    {
      
        rb.linearVelocity = movement * speed * speedMultiplier;
    }

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        speedEffectEndTime = Time.time + duration;
    }

    void RotatePlayer(float x, float y)
    {
       
        if (x == 0 && y == 0) return;

        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg - 90f;
        
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }
}
