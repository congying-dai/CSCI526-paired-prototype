using System.Collections;
using UnityEngine;

// Runtime visual effects for mystery boxes (no art assets needed).
// Good result: golden sparkles float up. Bad result: red burst, dark debris and a camera shake.
public class MysteryBoxEffects : MonoBehaviour
{
    public static void PlayGood(Vector3 position)
    {
        Spawn(position).StartGood();
    }

    public static void PlayBad(Vector3 position)
    {
        Spawn(position).StartBad();
    }

    private static MysteryBoxEffects Spawn(Vector3 position)
    {
        GameObject go = new GameObject("MysteryBoxEffect");
        go.transform.position = position;
        return go.AddComponent<MysteryBoxEffects>();
    }

    private void StartGood()
    {
        StartCoroutine(GoodRoutine());
    }

    private void StartBad()
    {
        StartCoroutine(BadRoutine());
    }

    private IEnumerator GoodRoutine()
    {
        Color[] colors = { new Color(1f, 0.9f, 0.3f), Color.white, new Color(0.5f, 1f, 0.6f) };

        // Glow ring
        SpawnParticle(Vector2.zero, new Color(1f, 0.9f, 0.3f, 0.5f), 0.3f, 2.2f, 0.5f, 0f);

        // Sparkles that burst out, drift upward and fade
        for (int i = 0; i < 24; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(0.8f, 2.5f);
            dir.y += 1f;
            float size = Random.Range(0.08f, 0.2f);
            SpawnParticle(dir, colors[Random.Range(0, colors.Length)], size, size * 0.2f, Random.Range(0.6f, 1.1f), 0f);
        }

        yield return new WaitForSeconds(1.3f);
        Destroy(gameObject);
    }

    private IEnumerator BadRoutine()
    {
        // Red shock ring
        SpawnParticle(Vector2.zero, new Color(0.9f, 0.05f, 0.05f, 0.7f), 0.3f, 3f, 0.45f, 0f);

        // Dark red / black debris thrown outward, pulled down a bit
        for (int i = 0; i < 28; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(1.5f, 4f);
            Color c = Random.value < 0.5f ? new Color(0.8f, 0.1f, 0.1f) : new Color(0.15f, 0.05f, 0.05f);
            float size = Random.Range(0.1f, 0.25f);
            SpawnParticle(dir, c, size, size * 0.3f, Random.Range(0.5f, 0.9f), -2f);
        }

        yield return ShakeCamera(0.35f, 0.25f);
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }

    private void SpawnParticle(Vector2 velocity, Color color, float startSize, float endSize, float life, float gravity)
    {
        GameObject p = new GameObject("Particle");
        p.transform.position = transform.position;
        p.transform.SetParent(transform, true);

        SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
        sr.sprite = ExplosionFlash.GetCircleSprite();
        sr.sortingOrder = 30;
        sr.color = color;

        StartCoroutine(AnimateParticle(p.transform, sr, velocity, color, startSize, endSize, life, gravity));
    }

    private IEnumerator AnimateParticle(Transform t, SpriteRenderer sr, Vector2 velocity, Color color,
                                        float startSize, float endSize, float life, float gravity)
    {
        float time = 0f;

        while (time < life && t != null)
        {
            float p = time / life;
            velocity.y += gravity * Time.deltaTime;
            t.position += (Vector3)(velocity * Time.deltaTime);
            t.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, p);
            sr.color = new Color(color.r, color.g, color.b, color.a * (1f - p));
            time += Time.deltaTime;
            yield return null;
        }

        if (t != null)
        {
            Destroy(t.gameObject);
        }
    }

    private IEnumerator ShakeCamera(float duration, float strength)
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            yield break;
        }

        Vector3 original = cam.transform.position;
        float time = 0f;

        while (time < duration && cam != null)
        {
            float falloff = 1f - time / duration;
            Vector2 offset = Random.insideUnitCircle * strength * falloff;
            cam.transform.position = original + (Vector3)offset;
            time += Time.deltaTime;
            yield return null;
        }

        if (cam != null)
        {
            cam.transform.position = original;
        }
    }
}
