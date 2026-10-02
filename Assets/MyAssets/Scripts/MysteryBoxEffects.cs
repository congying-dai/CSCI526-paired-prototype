using System.Collections;
using TMPro;
using UnityEngine;
public class MysteryBoxEffects : MonoBehaviour
{
    public static void PlayGood(Vector3 position, string message)
    {
        MysteryBoxEffects fx = Spawn(position);
        fx.StartGood();
        SoundEffects.Play(SoundEffects.Sfx.MysteryGood);
        fx.ShowText(message, new Color(1f, 0.9f, 0.3f));
    }

    public static void PlayBad(Vector3 position, string message)
    {
        MysteryBoxEffects fx = Spawn(position);
        fx.StartBad();
        SoundEffects.Play(SoundEffects.Sfx.MysteryBad);
        fx.ShowText(message, new Color(1f, 0.25f, 0.25f));
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
    private void ShowText(string message, Color color)
    {
        GameObject go = new GameObject("MysteryBoxText");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.6f, 0f);

        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.text = message;
        text.fontSize = 4f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;
        text.sortingOrder = 40;
        text.rectTransform.sizeDelta = new Vector2(8f, 2f);
        text.ForceMeshUpdate();
        Bounds bounds = text.textBounds;

        GameObject box = new GameObject("TextBackdrop");
        box.transform.SetParent(go.transform, false);
        box.transform.localPosition = bounds.center;
        box.transform.localScale = new Vector3(bounds.size.x + 0.5f, bounds.size.y + 0.25f, 1f);

        SpriteRenderer boxRenderer = box.AddComponent<SpriteRenderer>();
        boxRenderer.sprite = GetBoxSprite();
        boxRenderer.sortingOrder = 39; // just behind the text
        boxRenderer.color = new Color(0.04f, 0.04f, 0.1f, 0.8f);

        StartCoroutine(AnimateText(text, color, boxRenderer));
    }

    private static Sprite boxSprite;
    private static Sprite GetBoxSprite()
    {
        if (boxSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            boxSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        return boxSprite;
    }

    private IEnumerator AnimateText(TextMeshPro text, Color color, SpriteRenderer box)
    {
        const float life = 2f;
        float time = 0f;
        Vector3 start = text.transform.position;

        while (time < life && text != null)
        {
            float p = time / life;
            text.transform.position = start + Vector3.up * p * 0.8f;
            
            float fade = Mathf.Clamp01(2f * (1f - p));
            text.color = new Color(color.r, color.g, color.b, fade);
            box.color = new Color(0.04f, 0.04f, 0.1f, 0.8f * fade);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator GoodRoutine()
    {
        Color[] colors = { new Color(1f, 0.9f, 0.3f), Color.white, new Color(0.5f, 1f, 0.6f) };

        
        SpawnParticle(Vector2.zero, new Color(1f, 0.9f, 0.3f, 0.5f), 0.3f, 2.2f, 0.5f, 0f);

        for (int i = 0; i < 24; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(0.8f, 2.5f);
            dir.y += 1f;
            float size = Random.Range(0.08f, 0.2f);
            SpawnParticle(dir, colors[Random.Range(0, colors.Length)], size, size * 0.2f, Random.Range(0.6f, 1.1f), 0f);
        }

        yield return new WaitForSeconds(2.1f);
        Destroy(gameObject);
    }

    private IEnumerator BadRoutine()
    {
        SpawnParticle(Vector2.zero, new Color(0.9f, 0.05f, 0.05f, 0.7f), 0.3f, 3f, 0.45f, 0f);

        
        for (int i = 0; i < 28; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(1.5f, 4f);
            Color c = Random.value < 0.5f ? new Color(0.8f, 0.1f, 0.1f) : new Color(0.15f, 0.05f, 0.05f);
            float size = Random.Range(0.1f, 0.25f);
            SpawnParticle(dir, c, size, size * 0.3f, Random.Range(0.5f, 0.9f), -2f);
        }

        yield return ShakeCamera(0.35f, 0.25f);
        yield return new WaitForSeconds(1.7f);
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
