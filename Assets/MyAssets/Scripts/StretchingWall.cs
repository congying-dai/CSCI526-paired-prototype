using UnityEngine;

public class StretchingWall : MonoBehaviour
{
    public enum StretchAxis
    {
        Horizontal,
        Vertical
    }

    public enum FixedEnd
    {
        Negative,
        Positive
    }

    [Header("Stretch Settings")]
    [Tooltip("The axis along which the wall stretches.")]
    public StretchAxis stretchAxis = StretchAxis.Horizontal;

    [Tooltip("Negative means left or bottom. Positive means right or top.")]
    public FixedEnd fixedEnd = FixedEnd.Negative;

    [Tooltip("The shortest length relative to the original length.")]
    public float minimumLength = 0.5f;

    [Tooltip("The longest length relative to the original length.")]
    public float maximumLength = 2f;

    [Tooltip("The speed of the stretching cycle.")]
    public float stretchSpeed = 2f;

    private BoxCollider2D wallCollider;
    private Vector3 originalScale;

    private Vector3 fixedPointLocal;
    private Vector3 fixedPointWorld;

    private void Start()
    {
        wallCollider = GetComponent<BoxCollider2D>();
        originalScale = transform.localScale;

        if (wallCollider == null)
        {
            Debug.LogError(
                $"{gameObject.name} requires a BoxCollider2D.",
                this
            );

            enabled = false;
            return;
        }

        CalculateFixedPoint();

        // Remember where the fixed end starts in world space
        fixedPointWorld = transform.TransformPoint(fixedPointLocal);
    }

    private void FixedUpdate()
    {
        float cycle =
            (Mathf.Sin(Time.fixedTime * stretchSpeed) + 1f) / 2f;

        float lengthMultiplier =
            Mathf.Lerp(minimumLength, maximumLength, cycle);

        Vector3 newScale = originalScale;

        if (stretchAxis == StretchAxis.Horizontal)
        {
            newScale.x =
                originalScale.x * lengthMultiplier;
        }
        else
        {
            newScale.y =
                originalScale.y * lengthMultiplier;
        }

        transform.localScale = newScale;

        // Move the wall so the selected end remains fixed
        Vector3 currentFixedPoint =
            transform.TransformPoint(fixedPointLocal);

        transform.position +=
            fixedPointWorld - currentFixedPoint;
    }

    private void CalculateFixedPoint()
    {
        Vector2 point = wallCollider.offset;

        if (stretchAxis == StretchAxis.Horizontal)
        {
            float direction =
                fixedEnd == FixedEnd.Negative ? -1f : 1f;

            point.x +=
                direction * wallCollider.size.x / 2f;
        }
        else
        {
            float direction =
                fixedEnd == FixedEnd.Negative ? -1f : 1f;

            point.y +=
                direction * wallCollider.size.y / 2f;
        }

        fixedPointLocal = point;
    }
}