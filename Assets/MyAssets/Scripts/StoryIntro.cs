using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tells the story at the start of the game so the player knows the goal straight away.
// It creates itself when the scene starts, shows the text for a few seconds and fades it out.
public class StoryIntro : MonoBehaviour
{
    [TextArea(2, 5)]
    public string title = "THE HAUNTED CASTLE";

    [TextArea(2, 5)]
    public string story =
        "The princess is trapped deep inside the castle.\n" +
        "Shoot the coloured blocks to take their keys, unlock the doors, free her and bring her out.\n" +
        "Beware the powder kegs and cursed chests. Press G to slip into spirit form.";

    public float showTime = 7f;
    public float fadeTime = 1.5f;

    private TMP_Text text;
    private float startTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindFirstObjectByType<StoryIntro>() == null)
        {
            new GameObject("StoryIntro").AddComponent<StoryIntro>();
        }
    }

    private void Start()
    {
        GameObject canvasGo = new GameObject("IntroCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textGo = new GameObject("IntroText");
        textGo.transform.SetParent(canvasGo.transform, false);

        TextMeshProUGUI t = textGo.AddComponent<TextMeshProUGUI>();
        t.richText = true;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.text = $"<size=64><b><color=#C9D6FF>{title}</color></b></size>\n\n<size=32>{story}</size>";

        RectTransform rt = t.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1400f, 500f);

        text = t;
        startTime = Time.time;
    }

    private void Update()
    {
        if (text == null)
        {
            return;
        }

        float elapsed = Time.time - startTime;
        float alpha = 1f;

        // Fade in quickly, hold, then fade out
        if (elapsed < 0.6f)
        {
            alpha = elapsed / 0.6f;
        }
        else if (elapsed > showTime)
        {
            alpha = 1f - (elapsed - showTime) / fadeTime;
        }

        text.alpha = Mathf.Clamp01(alpha);

        if (elapsed > showTime + fadeTime)
        {
            Destroy(gameObject);
        }
    }
}
