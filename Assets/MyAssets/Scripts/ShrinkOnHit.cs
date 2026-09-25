using UnityEngine;

// Makes an object smaller every time it is shot. Added automatically by Damageable and ExplosiveBarrel.
public class ShrinkOnHit : MonoBehaviour
{
    [Tooltip("Size (as a fraction of the original) right before the object is destroyed.")]
    [Range(0.1f, 1f)] public float minScale = 0.4f;

    [Tooltip("How quickly the object shrinks to its new size.")]
    public float shrinkSpeed = 8f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    // hits / hitsRequired, from 0 (untouched) to 1 (about to be destroyed)
    public void SetProgress(float progress)
    {
        targetScale = originalScale * Mathf.Lerp(1f, minScale, Mathf.Clamp01(progress));
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, shrinkSpeed * Time.deltaTime);
    }
}
