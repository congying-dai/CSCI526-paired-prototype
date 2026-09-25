using UnityEngine;
using UnityEngine.InputSystem;

public class PrincessInteraction : MonoBehaviour
{
    private KeyInventory nearbyPlayerInventory;

    private void Update()
    {
        if (nearbyPlayerInventory == null)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void Interact()
    {
        nearbyPlayerInventory.CollectPrincess();
        SoundEffects.Play(SoundEffects.Sfx.Princess);

        Debug.Log("Princess collected.");

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        KeyInventory inventory =
            other.GetComponentInParent<KeyInventory>();

        if (inventory == null && other.attachedRigidbody != null)
        {
            inventory =
                other.attachedRigidbody.GetComponentInParent<KeyInventory>();
        }

        if (inventory != null)
        {
            nearbyPlayerInventory = inventory;
            Debug.Log("Player can interact with the princess.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        KeyInventory inventory =
            other.GetComponentInParent<KeyInventory>();

        if (inventory == nearbyPlayerInventory)
        {
            nearbyPlayerInventory = null;
        }
    }
}