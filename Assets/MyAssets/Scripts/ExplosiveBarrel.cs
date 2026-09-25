using System.Collections;
using UnityEngine;

// A barrel that explodes after being shot. If the player is inside the blast
// radius (and not in Ghost Mode) they are sent back to the start.
public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Explosion")]
    [Tooltip("Number of projectile hits required to set the barrel off.")]
    public int hitsRequired = 3;

    [Tooltip("Blast radius in world units.")]
    public float blastRadius = 2f;

    [Tooltip("Colour of the explosion flash.")]
    public Color explosionColor = new Color(1f, 0.55f, 0.1f, 0.8f);

    [Tooltip("How long the explosion flash lasts.")]
    public float explosionDuration = 0.4f;

    private int currentHits = 0;
    private bool hasExploded = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded || !collision.gameObject.CompareTag("Projectile"))
        {
            return;
        }

        currentHits++;

        if (currentHits >= hitsRequired)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);

        foreach (Collider2D hit in hits)
        {
            PlayerRespawn player = hit.GetComponentInParent<PlayerRespawn>();

            if (player == null)
            {
                continue;
            }

            GhostMode ghost = player.GetComponent<GhostMode>();

            if (ghost != null && ghost.IsGhost)
            {
                HUDMessage.Show("Ghost Mode protected you from the explosion!");
            }
            else
            {
                player.SendToStart();
            }

            break;
        }

        SpawnExplosionVisual();
        Destroy(gameObject);
    }

    private void SpawnExplosionVisual()
    {
        GameObject flash = new GameObject("Explosion");
        flash.transform.position = transform.position;
        flash.AddComponent<ExplosionFlash>().Play(blastRadius, explosionDuration, explosionColor);
    }
}

// Expanding, fading circle drawn at runtime, so no art assets are needed.
public class ExplosionFlash : MonoBehaviour
{
    private static Sprite circleSprite;

    public void Play(float radius, float duration, Color color)
    {
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.sortingOrder = 20;
        StartCoroutine(Animate(sr, radius, duration, color));
    }

    private IEnumerator Animate(SpriteRenderer sr, float radius, float duration, Color color)
    {
        float t = 0f;

        while (t < duration)
        {
            float p = t / duration;
            // The sprite is 1 unit wide, so scale = diameter
            transform.localScale = Vector3.one * radius * 2f * Mathf.Lerp(0.2f, 1f, p);
            sr.color = new Color(color.r, color.g, color.b, color.a * (1f - p));
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) <= size / 2f;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        // pixelsPerUnit = size, so the sprite is exactly 1 world unit wide
        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }
}
