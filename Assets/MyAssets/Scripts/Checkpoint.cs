using TMPro;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("The text displayed when the player succeeds.")]
    public TMP_Text successText;

    private Collider2D checkpointCollider;
    private bool hasCompleted = false;

    private void Awake()
    {
        checkpointCollider = GetComponent<Collider2D>();

        if (successText != null)
        {
            successText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasCompleted)
        {
            return;
        }

        KeyInventory inventory =
            other.GetComponentInParent<KeyInventory>();

        if (inventory == null)
        {
            return;
        }

        // Wait until the player's collider is completely inside
        if (!IsFullyInside(other))
        {
            return;
        }

        // The player must have collected the princess
        if (!inventory.HasPrincess)
        {
            return;
        }

        CompleteGame();
    }

    private bool IsFullyInside(Collider2D playerCollider)
    {
        Bounds checkpointBounds = checkpointCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        return
            playerBounds.min.x >= checkpointBounds.min.x &&
            playerBounds.max.x <= checkpointBounds.max.x &&
            playerBounds.min.y >= checkpointBounds.min.y &&
            playerBounds.max.y <= checkpointBounds.max.y;
    }

    private void CompleteGame()
    {
        hasCompleted = true;
        SoundEffects.Play(SoundEffects.Sfx.Success);

        if (successText != null)
        {
            successText.text = "SUCCESS!";
            successText.gameObject.SetActive(true);
        }

        Debug.Log("Success! The player returned with the princess.");
    }
}