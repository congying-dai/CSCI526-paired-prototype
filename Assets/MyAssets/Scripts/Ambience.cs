using UnityEngine;
using UnityEngine.UI;

// Extra atmosphere on top of the storm: drifting mist, glowing fireflies, a dark vignette around
// the screen edges and a glowing trail behind the player.
//
// It creates itself when the scene starts, so no Unity setup is needed.
// Add the component to an empty GameObject yourself if you want to change the settings.
public class Ambience : MonoBehaviour
{
    [Header("Mist")]
    public bool mist = true;
    public int mistClouds = 9;
    [Range(0f, 0.3f)] public float mistOpacity = 0.07f;
    public Color mistColor = new Color(0.7f, 0.8f, 1f);

    [Header("Fireflies")]
    public bool fireflies = true;
    public int fireflyCount = 35;
    public Color fireflyColor = new Color(1f, 0.95f, 0.5f);

    [Header("Vignette (dark screen edges)")]
    public bool vignette = true;
    [Range(0f, 1f)] public float vignetteStrength = 0.65f;

    [Header("Player trail")]
    public bool playerTrail = true;
    public Color trailColor = new Color(1f, 0.4f, 0.85f);
    public float trailInterval = 0.05f;

    private class Mote
    {
        public Transform t;
        public SpriteRenderer sr;
        public Vector2 home;      // offset from the camera centre
        public float seed;
        public float speed;
        public float radius;
    }

    private static Sprite softSprite;

    private Mote[] cloudMotes;
    private Mote[] flyMotes;
    private Transform player;
    private Rigidbody2D playerBody;
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
            playerBody = pc.GetComponent<Rigidbody2D>();
        }

        Rect view = GetView();

        if (mist)
        {
            cloudMotes = new Mote[mistClouds];

            for (int i = 0; i < mistClouds; i++)
            {
                float size = Random.Range(7f, 12f);
                Mote m = CreateMote("Mist", size, mistColor, 44);
                m.home = new Vector2(Random.Range(-view.width / 2f, view.width / 2f),
                                     Random.Range(-view.height / 2f, view.height / 2f));
                m.radius = Random.Range(1.5f, 3f);
                m.speed = Random.Range(0.05f, 0.12f);
                m.sr.color = new Color(mistColor.r, mistColor.g, mistColor.b, mistOpacity * Random.Range(0.6f, 1.2f));
                cloudMotes[i] = m;
            }
        }

        if (fireflies)
        {
            flyMotes = new Mote[fireflyCount];

            for (int i = 0; i < fireflyCount; i++)
            {
                Mote m = CreateMote("Firefly", Random.Range(0.18f, 0.35f), fireflyColor, 45);
                m.home = new Vector2(Random.Range(-view.width / 2f, view.width / 2f),
                                     Random.Range(-view.height / 2f, view.height / 2f));
                m.radius = Random.Range(0.5f, 1.5f);
                m.speed = Random.Range(0.3f, 0.7f);
                flyMotes[i] = m;
            }
        }

        if (vignette)
        {
            CreateVignette();
        }
    }

    private void Update()
    {
        Vector2 cam = Camera.main != null ? (Vector2)Camera.main.transform.position : Vector2.zero;

        AnimateMotes(cloudMotes, cam, false);
        AnimateMotes(flyMotes, cam, true);

        if (playerTrail)
        {
            UpdateTrail();
        }
    }

    // ---------- Mist and fireflies ----------

    private void AnimateMotes(Mote[] motes, Vector2 cam, bool twinkle)
    {
        if (motes == null)
        {
            return;
        }

        foreach (Mote m in motes)
        {
            float time = Time.time * m.speed;
            // Perlin noise gives smooth, natural wandering
            Vector2 wander = new Vector2(
                Mathf.PerlinNoise(m.seed, time) - 0.5f,
                Mathf.PerlinNoise(time, m.seed + 50f) - 0.5f) * 2f * m.radius;

            m.t.position = new Vector3(cam.x + m.home.x + wander.x, cam.y + m.home.y + wander.y, 0f);

            if (twinkle)
            {
                float glow = Mathf.Clamp01(0.25f + 0.85f * Mathf.PerlinNoise(m.seed + 20f, Time.time * 1.5f));
                m.sr.color = new Color(fireflyColor.r, fireflyColor.g, fireflyColor.b, glow);
            }
        }
    }

    private Mote CreateMote(string name, float size, Color color, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * size;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSoftSprite();
        sr.color = color;
        sr.sortingOrder = order;

        return new Mote { t = go.transform, sr = sr, seed = Random.Range(0f, 100f) };
    }

    // ---------- Player trail ----------

    private void UpdateTrail()
    {
        if (player == null || playerBody == null || Time.time < nextTrailTime)
        {
            return;
        }

        if (playerBody.linearVelocity.sqrMagnitude < 0.5f)
        {
            return;
        }

        nextTrailTime = Time.time + trailInterval;

        GameObject go = new GameObject("Trail");
        go.transform.position = player.position;
        go.transform.localScale = Vector3.one * 0.5f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSoftSprite();
        sr.color = new Color(trailColor.r, trailColor.g, trailColor.b, 0.6f);
        sr.sortingOrder = 9; // just below the player

        go.AddComponent<FadeAndShrink>().duration = 0.6f;
    }

    // ---------- Vignette ----------

    private void CreateVignette()
    {
        GameObject canvasGo = new GameObject("VignetteCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90; // under the objective panel and other UI

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

    // ---------- Helpers ----------

    private static Sprite GetSoftSprite()
    {
        if (softSprite != null)
        {
            return softSprite;
        }

        // A circle with fuzzy edges, exactly 1 world unit wide
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

    private static Rect GetView()
    {
        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
        {
            return new Rect(-10f, -6f, 20f, 12f);
        }

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        return new Rect(-width / 2f, -height / 2f, width, height);
    }
}

// Fades a sprite out while shrinking it, then destroys it.
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
