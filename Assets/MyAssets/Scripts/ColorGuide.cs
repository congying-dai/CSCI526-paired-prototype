using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Color-coded hints that teach the game at a glance:
//  - Doors glow in the colour of the key they need, brighter when you are carrying that key,
//    and say "Press F" or "Need Red key" when you stand next to them.
//  - Shootable coloured blocks show a health bar in their key colour.
//  - Explosive barrels show a pulsing red danger circle (the real blast radius) and an orange health bar.
//  - Mystery boxes cycle through rainbow colours with a "?" above them.
//  - The princess glows pink and the checkpoint glows gold.
//  - An objective panel in the top-left corner tells you what to do next.
//
// It creates itself when the scene starts, so no Unity setup is needed.
public class ColorGuide : MonoBehaviour
{
    [Header("What to show")]
    public bool showObjectivePanel = true;
    public bool showGlows = true;
    public bool showHealthBars = true;
    public bool showLabels = true;
    public bool showDangerRadius = true;

    public static Color GetKeyColor(KeyColor color)
    {
        switch (color)
        {
            case KeyColor.Red: return new Color(1f, 0.33f, 0.33f);
            case KeyColor.Blue: return new Color(0.35f, 0.6f, 1f);
            default: return new Color(0.33f, 0.87f, 0.47f);
        }
    }

    private enum Kind { Door, Breakable, Barrel, MysteryBox, Princess, Goal }

    private class Marker
    {
        public Transform target;
        public Kind kind;
        public Color color;
        public SpriteRenderer glow;
        public SpriteRenderer dangerRing;
        public SpriteRenderer barBack;
        public SpriteRenderer barFill;
        public TextMeshPro label;
        public Door door;
        public Damageable damageable;
        public ExplosiveBarrel barrel;
        public int sortingOrder;
    }

    private static Sprite squareSprite;

    private readonly List<Marker> markers = new List<Marker>();
    private KeyInventory inventory;
    private PrincessInteraction princess;
    private TMP_Text objectiveText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindFirstObjectByType<ColorGuide>() == null)
        {
            new GameObject("ColorGuide").AddComponent<ColorGuide>();
        }
    }

    private void Start()
    {
        inventory = FindFirstObjectByType<KeyInventory>();
        princess = FindFirstObjectByType<PrincessInteraction>();

        foreach (Door door in FindObjectsByType<Door>(FindObjectsSortMode.None))
        {
            Marker m = AddMarker(door.DoorObject.transform, Kind.Door, GetKeyColor(door.requiredKeyColor));
            m.door = door;
        }

        foreach (Damageable d in FindObjectsByType<Damageable>(FindObjectsSortMode.None))
        {
            Marker m = AddMarker(d.transform, Kind.Breakable, GetKeyColor(d.keyColor));
            m.damageable = d;
        }

        foreach (ExplosiveBarrel b in FindObjectsByType<ExplosiveBarrel>(FindObjectsSortMode.None))
        {
            Marker m = AddMarker(b.transform, Kind.Barrel, new Color(1f, 0.55f, 0.1f));
            m.barrel = b;
        }

        foreach (MysteryBox box in FindObjectsByType<MysteryBox>(FindObjectsSortMode.None))
        {
            AddMarker(box.transform, Kind.MysteryBox, Color.white);
        }

        if (princess != null)
        {
            AddMarker(princess.transform, Kind.Princess, new Color(1f, 0.5f, 0.8f));
        }

        foreach (Checkpoint cp in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
        {
            AddMarker(cp.transform, Kind.Goal, new Color(1f, 0.85f, 0.3f));
        }

        if (showObjectivePanel)
        {
            CreateObjectivePanel();
        }
    }

    // ---------- Creating markers ----------

    private Marker AddMarker(Transform target, Kind kind, Color color)
    {
        Marker m = new Marker { target = target, kind = kind, color = color };

        SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
        m.sortingOrder = targetRenderer != null ? targetRenderer.sortingOrder : 0;

        if (showGlows)
        {
            m.glow = CreateSprite("Glow", GetCircleSprite(), m.sortingOrder - 1);
        }

        if (kind == Kind.Barrel && showDangerRadius)
        {
            m.dangerRing = CreateSprite("DangerRadius", GetCircleSprite(), m.sortingOrder - 2);
        }

        if (showHealthBars && (kind == Kind.Breakable || kind == Kind.Barrel))
        {
            m.barBack = CreateSprite("BarBack", GetSquareSprite(), 35);
            m.barBack.color = new Color(0f, 0f, 0f, 0.7f);
            m.barFill = CreateSprite("BarFill", GetSquareSprite(), 36);
            m.barFill.color = color;
        }

        if (showLabels && kind != Kind.Breakable)
        {
            m.label = CreateLabel();
        }

        markers.Add(m);
        return m;
    }

    private SpriteRenderer CreateSprite(string name, Sprite sprite, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }

    private TextMeshPro CreateLabel()
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(transform, false);

        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.fontSize = 3f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;
        text.sortingOrder = 40;
        text.rectTransform.sizeDelta = new Vector2(6f, 1f);
        return text;
    }

    // ---------- Updating ----------

    private void Update()
    {
        for (int i = markers.Count - 1; i >= 0; i--)
        {
            Marker m = markers[i];

            if (m.target == null)
            {
                Remove(m);
                markers.RemoveAt(i);
                continue;
            }

            UpdateMarker(m);
        }

        if (objectiveText != null)
        {
            UpdateObjective();
        }
    }

    private void UpdateMarker(Marker m)
    {
        Bounds b = GetBounds(m.target.gameObject);
        Vector3 center = b.center;
        center.z = 0f;
        float top = b.max.y;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 3f);
        Color color = m.color;
        bool hasKey = m.kind == Kind.Door && inventory != null && KeyCount(m.door.requiredKeyColor) > 0;

        if (m.kind == Kind.MysteryBox)
        {
            // Rainbow cycle
            color = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.4f, 1f), 0.6f, 1f);
        }

        // Glow behind the object
        if (m.glow != null)
        {
            float alpha = Mathf.Lerp(0.12f, 0.28f, pulse);

            if (hasKey)
            {
                alpha = Mathf.Lerp(0.4f, 0.7f, pulse); // door you can open right now
            }

            float size = Mathf.Max(b.size.x, b.size.y) * 1.6f + 0.3f;
            m.glow.transform.position = center;
            m.glow.transform.localScale = Vector3.one * size;
            m.glow.color = new Color(color.r, color.g, color.b, alpha);
        }

        // Barrel danger radius (the real blast radius)
        if (m.dangerRing != null)
        {
            float progress = HitProgress(m);
            float speed = Mathf.Lerp(2f, 9f, progress);
            float ringPulse = 0.5f + 0.5f * Mathf.Sin(Time.time * speed);

            m.dangerRing.transform.position = center;
            m.dangerRing.transform.localScale = Vector3.one * m.barrel.blastRadius * 2f;
            m.dangerRing.color = new Color(1f, 0.1f, 0.1f, Mathf.Lerp(0.04f, 0.16f + 0.1f * progress, ringPulse));
        }

        // Health bar
        if (m.barBack != null)
        {
            float width = 0.9f;
            float remaining = 1f - HitProgress(m);
            Vector3 barPos = new Vector3(center.x, top + 0.2f, 0f);

            // The square sprite's pivot is its left edge, so offset both bars by half the width
            m.barBack.transform.position = barPos - new Vector3((width + 0.06f) / 2f, 0f, 0f);
            m.barBack.transform.localScale = new Vector3(width + 0.06f, 0.16f, 1f);

            m.barFill.transform.position = barPos - new Vector3(width / 2f, 0f, 0f);
            m.barFill.transform.localScale = new Vector3(width * remaining, 0.1f, 1f);
        }

        // Text above the object
        if (m.label != null)
        {
            m.label.transform.position = new Vector3(center.x, top + (m.barBack != null ? 0.55f : 0.35f), 0f);
            m.label.color = color;
            m.label.text = GetLabelText(m, hasKey);
        }
    }

    private string GetLabelText(Marker m, bool hasKey)
    {
        switch (m.kind)
        {
            case Kind.Door:
                if (m.door.PlayerNearby)
                {
                    return hasKey ? "Press F to open" : $"Need a {m.door.requiredKeyColor} key";
                }

                return $"{m.door.requiredKeyColor} door";

            case Kind.Barrel:
                return "DANGER";

            case Kind.MysteryBox:
                return "?";

            case Kind.Princess:
                return "Princess (F)";

            case Kind.Goal:
                return "GOAL";
        }

        return "";
    }

    private float HitProgress(Marker m)
    {
        if (m.damageable != null)
        {
            return Mathf.Clamp01((float)m.damageable.HitsTaken / m.damageable.hitsRequired);
        }

        if (m.barrel != null)
        {
            return Mathf.Clamp01((float)m.barrel.HitsTaken / m.barrel.hitsRequired);
        }

        return 0f;
    }

    private int KeyCount(KeyColor color)
    {
        switch (color)
        {
            case KeyColor.Red: return inventory.redKeys;
            case KeyColor.Blue: return inventory.blueKeys;
            default: return inventory.greenKeys;
        }
    }

    private void Remove(Marker m)
    {
        if (m.glow != null) Destroy(m.glow.gameObject);
        if (m.dangerRing != null) Destroy(m.dangerRing.gameObject);
        if (m.barBack != null) Destroy(m.barBack.gameObject);
        if (m.barFill != null) Destroy(m.barFill.gameObject);
        if (m.label != null) Destroy(m.label.gameObject);
    }

    // ---------- Objective panel ----------

    private void CreateObjectivePanel()
    {
        GameObject canvasGo = new GameObject("GuideCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textGo = new GameObject("ObjectiveText");
        textGo.transform.SetParent(canvasGo.transform, false);

        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = 30f;
        text.richText = true;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;

        RectTransform rt = text.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(30f, -25f);
        rt.sizeDelta = new Vector2(1100f, 200f);

        objectiveText = text;
    }

    private void UpdateObjective()
    {
        string legend =
            "<size=24><color=#FF5555>Red</color> / <color=#5599FF>Blue</color> / <color=#55DD77>Green</color> " +
            "blocks: shoot them to get a key, then open the door of the <b>same colour</b> (F)</size>";

        string objective;

        if (inventory != null && inventory.HasPrincess)
        {
            objective = "<color=#FFD94D>OBJECTIVE: Carry the princess into the GOAL</color>";
        }
        else if (princess != null)
        {
            objective = "<color=#FF80CC>OBJECTIVE: Rescue the princess (press F next to her)</color>";
        }
        else
        {
            objective = "<color=#FFD94D>OBJECTIVE: Reach the GOAL</color>";
        }

        objectiveText.text = objective + "\n" + legend +
            "\n<size=22><color=#FF8C1A>Orange barrels</color> explode: stay out of the red circle!  " +
            "Press <b>G</b> for Ghost Mode</size>";
    }

    // ---------- Helpers ----------

    private static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            return b;
        }

        Collider2D col = go.GetComponentInChildren<Collider2D>();
        return col != null ? col.bounds : new Bounds(go.transform.position, Vector3.one * 0.5f);
    }

    private static Sprite GetCircleSprite()
    {
        return ExplosionFlash.GetCircleSprite();
    }

    // A 1x1 white square. Pivot is on the left edge so bars can shrink from the right.
    private static Sprite GetSquareSprite()
    {
        if (squareSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }

        return squareSprite;
    }
}
