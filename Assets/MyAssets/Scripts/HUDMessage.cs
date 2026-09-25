using TMPro;
using UnityEngine;

// Shows short on-screen messages ("Ghost Mode!", "Speed boost!", ...).
// Put one in the scene and assign a TMP_Text. If there is none, messages only go to the console.
public class HUDMessage : MonoBehaviour
{
    [Tooltip("The text used to display messages.")]
    public TMP_Text messageText;

    [Tooltip("How long a message stays on screen.")]
    public float displayTime = 2f;

    private static HUDMessage instance;
    private float hideTime;

    private void Awake()
    {
        instance = this;

        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    private void Update()
    {
        if (messageText != null && messageText.text != "" && Time.time >= hideTime)
        {
            messageText.text = "";
        }
    }

    public static void Show(string message)
    {
        Debug.Log(message);

        if (instance == null || instance.messageText == null)
        {
            return;
        }

        instance.messageText.text = message;
        instance.hideTime = Time.time + instance.displayTime;
    }
}
