using UnityEngine;

// Walk into the box to get a random effect - good or bad.
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
    public float ghostDuration = 5f;

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
        Effect effect = (Effect)Random.Range(0, System.Enum.GetValues(typeof(Effect)).Length);

        switch (effect)
        {
            case Effect.FreeKey:
                KeyColor color = (KeyColor)Random.Range(0, System.Enum.GetValues(typeof(KeyColor)).Length);
                inventory.AddKey(color);
                HUDMessage.Show($"Mystery Box: free {color} key!");
                break;

            case Effect.SpeedBoost:
                player.GetComponent<PlayerController>()?.ApplySpeedMultiplier(speedBoostMultiplier, speedEffectDuration);
                HUDMessage.Show("Mystery Box: speed boost!");
                break;

            case Effect.GhostMode:
                GhostMode ghost = player.GetComponent<GhostMode>();

                if (ghost != null)
                {
                    ghost.Activate(ghostDuration);
                    HUDMessage.Show("Mystery Box: Ghost Mode!");
                }
                break;

            case Effect.Slowdown:
                player.GetComponent<PlayerController>()?.ApplySpeedMultiplier(slowdownMultiplier, speedEffectDuration);
                HUDMessage.Show("Mystery Box: slowed down!");
                break;

            case Effect.BackToStart:
                PlayerRespawn respawn = player.GetComponent<PlayerRespawn>();

                if (respawn != null)
                {
                    respawn.SendToStart();
                }
                break;
        }
    }
}
