using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Spirit power. Press G to slip into spirit form for a few seconds: the player turns pale and
// see-through, can walk through walls and moving railings, and is immune to powder keg blasts. Doors, the princess and barrels stay solid.
[RequireComponent(typeof(PlayerRespawn))]
public class GhostMode : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How long Ghost Mode lasts (seconds).")]
    public float duration = 4f;

    [Tooltip("Cooldown after Ghost Mode ends (seconds).")]
    public float cooldown = 8f;

    [Header("Visuals")]
    [Range(0.1f, 1f)] public float ghostAlpha = 0.35f;

    [Tooltip("Colour of the spirit form.")]
    public Color spiritColor = new Color(0.65f, 0.9f, 1f);

    [Tooltip("Optional text that shows Ghost Mode status.")]
    public TMP_Text statusText;

    [Header("What the ghost can pass through")]
    [Tooltip("Parents of all wall colliders (e.g. the 'Walls' and 'Obstacles' objects). If empty, objects named Walls and Obstacles are searched for automatically.")]
    public Transform[] wallRoots;

    public bool IsGhost { get; private set; }

    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private readonly List<Collider2D> ignoredColliders = new List<Collider2D>();
    private float ghostEndTime;
    private float nextAvailableTime;
    private bool waitingToLeaveWalls;

    private void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (wallRoots == null || wallRoots.Length == 0)
        {
            List<Transform> found = new List<Transform>();

            foreach (string rootName in new[] { "Walls", "Obstacles" })
            {
                GameObject root = GameObject.Find(rootName);

                if (root != null)
                {
                    found.Add(root.transform);
                }
            }

            wallRoots = found.ToArray();
        }
    }

    private void Update()
    {
        if (IsGhost)
        {
            if (!waitingToLeaveWalls && Time.time >= ghostEndTime)
            {
                waitingToLeaveWalls = true;
            }

            // Don't turn solid while standing inside a wall, otherwise the player would get stuck
            if (waitingToLeaveWalls && !IsInsideAWall())
            {
                EndGhostMode();
            }
        }
        else if (Keyboard.current != null &&
                 Keyboard.current.gKey.wasPressedThisFrame &&
                 Time.time >= nextAvailableTime)
        {
            Activate(duration);
        }

        UpdateStatusText();
    }

    // Also called by mystery boxes.
    public void Activate(float seconds)
    {
        if (IsGhost)
        {
            ghostEndTime = Mathf.Max(ghostEndTime, Time.time + seconds);
            waitingToLeaveWalls = false;
            return;
        }

        IsGhost = true;
        waitingToLeaveWalls = false;
        ghostEndTime = Time.time + seconds;

        SetIgnoredColliders(true);

        if (spriteRenderer != null)
        {
            Color c = spiritColor;
            c.a = ghostAlpha;
            spriteRenderer.color = c;
        }

        SoundEffects.Play(SoundEffects.Sfx.GhostOn);
        HUDMessage.Show("Your spirit slips free of your body!");
    }

    private void EndGhostMode()
    {
        SoundEffects.Play(SoundEffects.Sfx.GhostOff);
        IsGhost = false;
        waitingToLeaveWalls = false;
        nextAvailableTime = Time.time + cooldown;

        SetIgnoredColliders(false);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void SetIgnoredColliders(bool ignore)
    {
        if (ignore)
        {
            ignoredColliders.Clear();

            foreach (Transform root in wallRoots)
            {
                if (root != null)
                {
                    ignoredColliders.AddRange(root.GetComponentsInChildren<Collider2D>());
                }
            }

            foreach (MovingRailing railing in FindObjectsByType<MovingRailing>(FindObjectsSortMode.None))
            {
                ignoredColliders.AddRange(railing.GetComponentsInChildren<Collider2D>());
            }
        }

        foreach (Collider2D col in ignoredColliders)
        {
            if (col != null)
            {
                Physics2D.IgnoreCollision(playerCollider, col, ignore);
            }
        }

        if (!ignore)
        {
            ignoredColliders.Clear();
        }
    }

    private bool IsInsideAWall()
    {
        foreach (Collider2D col in ignoredColliders)
        {
            if (col != null && playerCollider.Distance(col).isOverlapped)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        if (IsGhost)
        {
            float left = Mathf.Max(0f, ghostEndTime - Time.time);
            statusText.text = waitingToLeaveWalls
                ? "Spirit form: leave the wall!"
                : $"Spirit form: {left:0.0}s";
        }
        else if (Time.time < nextAvailableTime)
        {
            statusText.text = $"Spirit form ready in {nextAvailableTime - Time.time:0.0}s";
        }
        else
        {
            statusText.text = "Spirit form ready (G)";
        }
    }

    private void OnDisable()
    {
        if (IsGhost)
        {
            EndGhostMode();
        }
    }
}
