using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A dark, semi-transparent box behind a UI text so it stays readable on any background.
// The box sizes itself to the text, follows it, and is hidden whenever the text is empty or inactive.
//
//   TextBackdrop.Attach(myTmpText);
public class TextBackdrop : MonoBehaviour
{
    public static readonly Color BoxColor = new Color(0.04f, 0.04f, 0.10f, 1f);

    private TMP_Text text;
    private Image image;
    private RectTransform rect;
    private Vector2 padding;
    private float opacity;

    public static TextBackdrop Attach(TMP_Text text, float padX = 24f, float padY = 12f, float opacity = 0.8f)
    {
        if (text == null || text.transform.parent == null)
        {
            return null;
        }

        GameObject go = new GameObject(text.name + "_Backdrop", typeof(RectTransform));
        go.transform.SetParent(text.transform.parent, false);

        // Put the box just before the text in the hierarchy so it is drawn behind it
        go.transform.SetSiblingIndex(text.transform.GetSiblingIndex());

        TextBackdrop backdrop = go.AddComponent<TextBackdrop>();
        backdrop.text = text;
        backdrop.padding = new Vector2(padX, padY);
        backdrop.opacity = opacity;
        backdrop.rect = go.GetComponent<RectTransform>();
        backdrop.rect.anchorMin = backdrop.rect.anchorMax = new Vector2(0.5f, 0.5f);
        backdrop.rect.pivot = new Vector2(0.5f, 0.5f);

        backdrop.image = go.AddComponent<Image>();
        backdrop.image.raycastTarget = false;
        backdrop.image.enabled = false;

        return backdrop;
    }

    private void LateUpdate()
    {
        if (text == null)
        {
            Destroy(gameObject);
            return;
        }

        bool show = text.gameObject.activeInHierarchy &&
                    !string.IsNullOrEmpty(text.text) &&
                    text.alpha > 0.01f;

        image.enabled = show;

        if (!show)
        {
            return;
        }

        text.ForceMeshUpdate();
        Bounds b = text.textBounds;
        Vector3 scale = text.transform.localScale;

        rect.rotation = text.transform.rotation;
        rect.position = text.transform.TransformPoint(b.center);
        rect.sizeDelta = new Vector2(b.size.x * scale.x, b.size.y * scale.y) + padding * 2f;

        image.color = new Color(BoxColor.r, BoxColor.g, BoxColor.b, opacity * text.alpha);
    }
}
