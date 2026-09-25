using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Stormy night atmosphere: dimmed scene, random lightning flashes, falling rain and a
// flickering torch around the player.
//
// It creates itself automatically when the scene starts, so no Unity setup is needed.
// To tweak the values, add this component to an empty GameObject in the scene yourself
// (an existing one is used instead of creating a new one).
public class Atmosphere : MonoBehaviour
{
    [Header("Ambient light")]
    [Tooltip("Global light is multiplied by this to make the scene darker (1 = unchanged).")]
    [Range(0.1f, 1f)] public float darkness = 0.55f;

    [Header("Lightning")]
    public bool lightning = true;
    public float minTimeBetweenStrikes = 6f;
    public float maxTimeBetweenStrikes = 14f;

    [Tooltip("The global light is multiplied by this at the peak of a flash.")]
    public float flashBrightness = 2.2f;

    [Header("Rain")]
    public bool rain = true;
    public int dropCount = 120;
    public float rainSpeed = 14f;
    public Color rainColor = new Color(0.7f, 0.8f, 1f, 0.35f);

    [Header("Player torch")]
    public bool playerTorch = true;
    public float torchRadius = 4f;
    public Color torchColor = new Color(1f, 0.85f, 0.6f);

    private Light2D globalLight;
    private float baseIntensity;
    private Light2D torch;
    private float torchBaseIntensity = 0.9f;

    private Transform[] drops;
    private float[] dropSpeedFactor;
    private const float DropSlant = 0.25f;

    // Creates the atmosphere automatically unless one was already placed in the scene.
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindFirstObjectByType<Atmosphere>() == null)
        {
            new GameObject("Atmosphere").AddComponent<Atmosphere>();
        }
    }

    private void Start()
    {
        foreach (Light2D light in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (light.lightType == Light2D.LightType.Global)
            {
                globalLight = light;
                break;
            }
        }

        if (globalLight != null)
        {
            baseIntensity = globalLight.intensity;
            globalLight.intensity = baseIntensity * darkness;

            if (lightning)
            {
                StartCoroutine(LightningRoutine());
            }
        }
        else
        {
            Debug.LogWarning("Atmosphere: no Global Light 2D found, so darkness and lightning are skipped.");
        }

        if (playerTorch)
        {
            CreateTorch();
        }

        if (rain)
        {
            CreateRain();
        }
    }

    private void Update()
    {
        if (torch != null)
        {
            // Small random flicker
            float noise = Mathf.PerlinNoise(Time.time * 6f, 0.5f);
            torch.intensity = torchBaseIntensity * Mathf.Lerp(0.75f, 1.1f, noise);
        }

        if (drops != null)
        {
            UpdateRain();
        }
    }

    // ---------- Lightning ----------

    private IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes));

            // Two quick flashes, then a slow fade back to the dark level
            yield return Flash(flashBrightness, 0.06f);
            SetLight(darkness * 0.8f);
            yield return new WaitForSeconds(0.08f);
            yield return Flash(flashBrightness * 0.85f, 0.14f);

            float t = 0f;
            const float fade = 0.5f;

            while (t < fade)
            {
                SetLight(Mathf.Lerp(flashBrightness * 0.5f, darkness, t / fade));
                t += Time.deltaTime;
                yield return null;
            }

            SetLight(darkness);
        }
    }

    private IEnumerator Flash(float brightness, float duration)
    {
        SetLight(brightness);
        yield return new WaitForSeconds(duration);
    }

    private void SetLight(float multiplier)
    {
        globalLight.intensity = baseIntensity * multiplier;
    }

    // ---------- Player torch ----------

    private void CreateTorch()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player == null)
        {
            return;
        }

        GameObject go = new GameObject("PlayerTorch");
        go.transform.SetParent(player.transform, false);

        torch = go.AddComponent<Light2D>();
        torch.lightType = Light2D.LightType.Point;
        torch.color = torchColor;
        torch.intensity = torchBaseIntensity;
        torch.pointLightInnerRadius = 0.5f;
        // The player is scaled down, so compensate to get the radius in world units
        float scale = Mathf.Max(player.transform.lossyScale.x, 0.01f);
        torch.pointLightOuterRadius = torchRadius / scale;
    }

    // ---------- Rain ----------

    private void CreateRain()
    {
        Sprite sprite = ExplosionFlash.GetCircleSprite();
        drops = new Transform[dropCount];
        dropSpeedFactor = new float[dropCount];

        GameObject parent = new GameObject("Rain");
        parent.transform.SetParent(transform, false);

        Rect view = GetViewRect();

        for (int i = 0; i < dropCount; i++)
        {
            GameObject d = new GameObject("Drop");
            d.transform.SetParent(parent.transform, false);

            SpriteRenderer sr = d.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = rainColor;
            sr.sortingOrder = 50;

            // A thin streak tilted to match the fall direction
            d.transform.localScale = new Vector3(0.03f, Random.Range(0.3f, 0.6f), 1f);
            d.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(DropSlant, 1f) * Mathf.Rad2Deg);

            d.transform.position = new Vector3(
                Random.Range(view.xMin, view.xMax),
                Random.Range(view.yMin, view.yMax),
                0f);

            drops[i] = d.transform;
            dropSpeedFactor[i] = Random.Range(0.8f, 1.2f);
        }
    }

    private void UpdateRain()
    {
        Rect view = GetViewRect();

        for (int i = 0; i < drops.Length; i++)
        {
            Vector3 pos = drops[i].position;
            float fall = rainSpeed * dropSpeedFactor[i] * Time.deltaTime;

            pos.y -= fall;
            pos.x += fall * DropSlant;

            // Wrap back to the top when leaving the screen
            if (pos.y < view.yMin - 0.5f)
            {
                pos.y = view.yMax + 0.5f;
                pos.x = Random.Range(view.xMin - 2f, view.xMax);
            }

            if (pos.x > view.xMax + 0.5f)
            {
                pos.x = view.xMin - 0.5f;
            }

            drops[i].position = pos;
        }
    }

    private Rect GetViewRect()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return new Rect(-10f, -6f, 20f, 12f);
        }

        float height = cam.orthographic ? cam.orthographicSize * 2f : 12f;
        float width = height * cam.aspect;
        Vector3 c = cam.transform.position;

        return new Rect(c.x - width / 2f, c.y - height / 2f, width, height);
    }

    private void OnDestroy()
    {
        // Leave the global light as we found it
        if (globalLight != null)
        {
            globalLight.intensity = baseIntensity;
        }
    }
}
