using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    private enum Effect
    {
        FreeKey,
        SpeedBoost,
        GhostMode,
        Slowdown,
        BackToStart
    }

    [Header("Effect strength")]
    public float speedBoostMultiplier = 1.6f;
    public float slowdownMultiplier = 0.5f;
    public float speedEffectDuration = 5f;
    public float ghostDuration = 3f;

    private bool opened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened)
        {
            return;
        }

        KeyInventory inventory = other.GetComponentInParent<KeyInventory>();

        if (inventory == null)
        {
            return;
        }

        opened = true;
        ApplyRandomEffect(inventory);
        Destroy(gameObject);
    }

    private void ApplyRandomEffect(KeyInventory inventory)
    {
        GameObject player = inventory.gameObject;
        Vector3 boxPosition = transform.position;
        Effect effect = (Effect)Random.Range(0, System.Enum.GetValues(typeof(Effect)).Length);

        switch (effect)
        {
            case Effect.FreeKey:
                KeyColor color = (KeyColor)Random.Range(0, System.Enum.GetValues(typeof(KeyColor)).Length);
                inventory.AddKey(color);
                Report(true, boxPosition, $"The chest holds a {color} key!");
                break;

            case Effect.SpeedBoost:
                player.GetComponent<PlayerController>()?.ApplySpeedMultiplier(speedBoostMultiplier, speedEffectDuration);
                Report(true, boxPosition, $"A friendly spirit quickens your steps for {speedEffectDuration:0} seconds!");
                break;

            case Effect.GhostMode:
                GhostMode ghost = player.GetComponent<GhostMode>();

                if (ghost != null)
                {
                    ghost.Activate(ghostDuration);
                    Report(true, boxPosition, $"The chest frees your spirit form for {ghostDuration:0} seconds!");
                }
                else
                {
                    Report(true, boxPosition, "Spirit form (not set up on the player)");
                }
                break;

            case Effect.Slowdown:
                player.GetComponent<PlayerController>()?.ApplySpeedMultiplier(slowdownMultiplier, speedEffectDuration);
                Report(false, boxPosition, $"Cursed! A chill of dread slows you for {speedEffectDuration:0} seconds!");
                break;

            case Effect.BackToStart:
                Report(false, boxPosition, "Cursed! The chest drags you back to the gate!");
                player.GetComponent<PlayerRespawn>()?.SendToStart();
                break;
        }
    }

    // Shows the result as text above the box (and on the HUD if there is one) and plays the good/bad effect.
    private void Report(bool good, Vector3 position, string message)
    {
        HUDMessage.Show("Cursed Chest: " + message);

        if (good)
        {
            MysteryBoxEffects.PlayGood(position, message);
        }
        else
        {
            MysteryBoxEffects.PlayBad(position, message);
        }
    }
}
