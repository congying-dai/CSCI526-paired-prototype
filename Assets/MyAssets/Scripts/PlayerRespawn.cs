using UnityEngine;

// Remembers where the player started and can send them back there.
public class PlayerRespawn : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody2D rb;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
    }

    public void SendToStart()
    {
        SoundEffects.Play(SoundEffects.Sfx.Reset);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = startPosition;
        }

        transform.SetPositionAndRotation(startPosition, startRotation);
        HUDMessage.Show("Back to the start!");
    }
}
