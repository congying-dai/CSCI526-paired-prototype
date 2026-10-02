using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Number of projectile hits required to destroy this object.")]
    public int hitsRequired = 5;

    [Header("Key Reward")]
    [Tooltip("The color of the key awarded when this object is destroyed.")]
    public KeyColor keyColor;

    [Tooltip("The player's key inventory.")]
    private KeyInventory keyInventory;

    private int currentHits = 0;
    private bool hasBeenDestroyed = false;
    private ShrinkOnHit shrink;

    private void Awake()
    {
        shrink = GetComponent<ShrinkOnHit>();

        if (shrink == null)
        {
            shrink = gameObject.AddComponent<ShrinkOnHit>();
        }

        keyInventory = FindFirstObjectByType<KeyInventory>();

        if (keyInventory == null)
        {
            Debug.LogWarning("No KeyInventory found in the scene.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Projectile"))
        {
            return;
        }


        if (hasBeenDestroyed)
        {
            return;
        }

        currentHits++;
        SoundEffects.Play(SoundEffects.Sfx.Hit);
        shrink.SetProgress((float)currentHits / hitsRequired);

        Debug.Log(
            $"{gameObject.name}: {currentHits}/{hitsRequired}"
        );

        if (currentHits >= hitsRequired)
        {
            DestroyAndGiveKey();
        }
    }

    private void DestroyAndGiveKey()
    {
        hasBeenDestroyed = true;
        SoundEffects.Play(SoundEffects.Sfx.KeyPickup);
        Debug.Log("DestroyAndGiveKey()");

        if (keyInventory != null)
        {
            keyInventory.AddKey(keyColor);
            Debug.Log($"Added key: {keyColor}");
        }
        else
        {
            Debug.LogWarning(
                $"{gameObject.name} does not have a Key Inventory assigned."
            );
        }

        Destroy(gameObject);
    }
}