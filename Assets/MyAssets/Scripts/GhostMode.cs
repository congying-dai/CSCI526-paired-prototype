using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerRespawn))]
public class GhostMode : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How long Ghost Mode lasts (seconds).")]
    public float duration = 2.5f;

    [Tooltip("Cooldown after Ghost Mode ends (seconds).")]
    public float cooldown = 15f;

    [Header("Limits")]
    [Tooltip("How many times the player can use spirit form per run (G key). Chests give a free use and don't count.")]
    public int maxCharges = 3;

    [Header("Visuals")]
    [Range(0.1f, 1f)] public float ghostAlpha = 0.35f;

    [Tooltip("Colour of the spirit form.")]
    public Color spiritColor = new Color(0.65f, 0.9f, 1f);

    [Tooltip("Text that shows the spirit form status and uses left (e.g. 2/3). If empty, one is created in the bottom-left corner.")]
    public TMP_Text statusText;

    [Header("What the ghost can pass through")]
    [Tooltip("Parents of all wall colliders (e.g. the 'Walls' and 'Obstacles' objects). If empty, objects named Walls and Obstacles are searched for automatically.")]
    public Transform[] wallRoots;

    [Tooltip("Walls the ghost can NEVER pass, even if they are under a wall root (e.g. the 4 outer boundary walls). Drag their colliders here.")]
    public Collider2D[] solidWalls;

    [Tooltip("If on, the ghost can also pass through locked doors (the 'Doors' object).")]
    public bool passThroughDoors = true;

    [Tooltip("Parent of all door objects. If empty, an object named Doors is searched for automatically.")]
    public Transform doorsRoot;

    public bool IsGhost { get; private set; }

    public int ChargesLeft { get; private set; }

    private KeyInventory inventory;
    private Collider2D playerCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private readonly List<Collider2D> ignoredColliders = new List<Collider2D>();
    private float ghostEndTime;
    private float nextAvailableTime;
    private bool waitingToLeaveWalls;
    // Only spirit form started with the G key triggers the cooldown (mystery box ones are free)
    private bool activatedByKey;

    private void Awake()
    {
        if (statusText == null)
        {
            statusText = CreateStatusText();
        }

        TextBackdrop.Attach(statusText, 16f, 8f);

        inventory = GetComponent<KeyInventory>();
        ChargesLeft = maxCharges;
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

        if (doorsRoot == null)
        {
            GameObject doors = GameObject.Find("Doors");

            if (doors != null)
            {
                doorsRoot = doors.transform;
            }
        }
    }

    private void Update()
    {
        if (IsGhost)
        {
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                HUDMessage.Show("You are already in spirit form!");
            }

            if (!waitingToLeaveWalls && Time.time >= ghostEndTime)
            {
                waitingToLeaveWalls = true;
            }

            if (waitingToLeaveWalls && !IsInsideAWall())
            {
                EndGhostMode();
            }
        }
        else if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            TryActivate();
        }

        UpdateStatusText();
    }

    private void TryActivate()
    {
        if (ChargesLeft <= 0)
        {
            HUDMessage.Show("Your spirit has no strength left...");
        }
        else if (Time.time < nextAvailableTime)
        {
            HUDMessage.Show("Your spirit is still recovering...");
        }
        else if (inventory != null && inventory.HasPrincess)
        {
            HUDMessage.Show("The princess is too heavy for spirit form!");
        }
        else
        {
            ChargesLeft--;
            Activate(duration);
            activatedByKey = true;
        }
    }

    public void Activate(float seconds)
    {
        if (IsGhost)
        {
            ghostEndTime = Mathf.Max(ghostEndTime, Time.time + seconds);
            waitingToLeaveWalls = false;
            return;
        }

        IsGhost = true;
        activatedByKey = false;
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

        if (activatedByKey)
        {
            nextAvailableTime = Time.time + cooldown;
        }

        activatedByKey = false;

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
                AddPassableColliders(root);
            }

            if (passThroughDoors)
            {
                AddPassableColliders(doorsRoot);
            }

            foreach (MovingRailing railing in FindObjectsByType<MovingRailing>(FindObjectsSortMode.None))
            {
                AddPassableColliders(railing.transform);
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

    private void AddPassableColliders(Transform root)
    {
        if (root == null)
        {
            return;
        }

        foreach (Collider2D col in root.GetComponentsInChildren<Collider2D>())
        {
            if (col.isTrigger || (solidWalls != null && System.Array.IndexOf(solidWalls, col) >= 0))
            {
                continue;
            }

            ignoredColliders.Add(col);
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

    private TMP_Text CreateStatusText()
    {
        GameObject canvasGo = new GameObject("SpiritStatusCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 95;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textGo = new GameObject("SpiritStatusText");
        textGo.transform.SetParent(canvasGo.transform, false);

        TextMeshProUGUI t = textGo.AddComponent<TextMeshProUGUI>();
        t.fontSize = 32f;
        t.fontStyle = FontStyles.Bold;
        t.color = spiritColor;
        t.alignment = TextAlignmentOptions.BottomLeft;
        t.raycastTarget = false;

        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(40f, 40f);
        rt.sizeDelta = new Vector2(700f, 50f);

        return t;
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        string uses = $"{ChargesLeft}/{maxCharges} remaining";

        if (IsGhost)
        {
            float left = Mathf.Max(0f, ghostEndTime - Time.time);
            statusText.text = waitingToLeaveWalls
                ? $"Spirit form {uses} - leave the wall!"
                : $"Spirit form {uses} - active {left:0.0}s";
        }
        else if (ChargesLeft <= 0)
        {
            statusText.text = $"Spirit form {uses} - no strength left";
        }
        else if (inventory != null && inventory.HasPrincess)
        {
            statusText.text = $"Spirit form {uses} - the princess is too heavy";
        }
        else if (Time.time < nextAvailableTime)
        {
            statusText.text = $"Spirit form {uses} - recovering {nextAvailableTime - Time.time:0.0}s";
        }
        else
        {
            statusText.text = $"Spirit form {uses} - ready (G)";
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
