using UnityEngine;
using UnityEngine.UI;


public class Ambience : MonoBehaviour
{
    [Header("Vignette (dark screen edges)")]
    public bool vignette = true;
    [Range(0f, 1f)] public float vignetteStrength = 0.65f;

    [Header("Spirit trail")]
    public bool spiritTrail = true;
    public Color trailColor = new Color(0.6f, 0.9f, 1f);
    public float trailInterval = 0.05f;

    private static Sprite softSprite;

    private GhostMode ghost;
    private Transform player;
    private float nextTrailTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindFirstObjectByType<Ambience>() == null)
        {
            new GameObject("Ambience").AddComponent<Ambience>();
        }
    }

    private void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();

        if (pc != null)
        {
            player = pc.transform;
            ghost = pc.GetComponent<GhostMode>();
        }

        if (vignette)
        {
            CreateVignette();
        }
    }

    private void Update()
    {
        if (spiritTrail && ghost != null && ghost.IsGhost && Time.time >= nextTrailTime)
        {
            nextTrailTime = Time.time + trailInterval;
            SpawnTrailPuff();
        }
    }


    private void SpawnTrailPuff()
    {
        GameObject go = new GameObject("SpiritTrail");
        go.transform.position = player.position;
        go.transform.localScale = Vector3.one * 0.6f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSoftSprite();
        sr.color = new Color(trailColor.r, trailColor.g, trailColor.b, 0.6f);
        sr.sortingOrder = 9; // just below the player

        go.AddComponent<FadeAndShrink>().duration = 0.8f;
    }

    private void CreateVignette()
    {
        GameObject canvasGo = new GameObject("VignetteCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90; // under other UI

        GameObject imageGo = new GameObject("Vignette");
        imageGo.transform.SetParent(canvasGo.transform, false);

        Image image = imageGo.AddComponent<Image>();
        image.sprite = CreateVignetteSprite();
        image.color = new Color(0f, 0f, 0f, vignetteStrength);
        image.raycastTarget = false;

        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Sprite CreateVignetteSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 0 in the middle, 1 at the corners
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.414f;
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 1f, d));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite GetSoftSprite()
    {
        if (softSprite != null)
        {
            return softSprite;
        }

        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - d), 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        softSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return softSprite;
    }
}

public class FadeAndShrink : MonoBehaviour
{
    public float duration = 0.6f;

    private SpriteRenderer sr;
    private Vector3 startScale;
    private Color startColor;
    private float time;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        startColor = sr.color;
    }

    private void Update()
    {
        time += Time.deltaTime;
        float p = Mathf.Clamp01(time / duration);

        transform.localScale = startScale * (1f - p * 0.7f);
        sr.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1f - p));

        if (p >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
