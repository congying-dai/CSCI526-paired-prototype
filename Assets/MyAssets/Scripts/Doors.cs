using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    [Tooltip("The key color required to open this door.")]
    public KeyColor requiredKeyColor;

    [Tooltip("The complete door object that will be destroyed.")]
    private GameObject doorObject;

    private KeyInventory nearbyPlayerInventory;

    private void Awake()
    {
        // Automatically use the parent as the door object
        if (transform.parent != null)
        {
            doorObject = transform.parent.gameObject;
        }
        else
        {
            doorObject = gameObject;
        }
    }

    private void Update()
    {
        if (nearbyPlayerInventory == null)
        {
            return;
        }
        Debug.Log("nearbyPlayerInventory != null");

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("TryOpenDoor()");
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        bool successfullyUsedKey =
            nearbyPlayerInventory.UseKey(requiredKeyColor);

        if (successfullyUsedKey)
        {
            SoundEffects.Play(SoundEffects.Sfx.DoorOpen);
            Destroy(doorObject);
        }
        else
        {
            SoundEffects.Play(SoundEffects.Sfx.Denied);
            Debug.Log(
                $"You do not have a {requiredKeyColor} key."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"InteractionZone detected: {other.gameObject.name}");

        KeyInventory inventory =
            other.GetComponentInParent<KeyInventory>();

        if (inventory != null)
        {
            nearbyPlayerInventory = inventory;
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