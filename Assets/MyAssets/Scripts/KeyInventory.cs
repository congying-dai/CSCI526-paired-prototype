using UnityEngine;

public enum KeyColor
{
    Red,
    Blue,
    Green
}

public class KeyInventory : MonoBehaviour
{
    [Header("Key Counts")]
    public int redKeys;
    public int blueKeys;
    public int greenKeys;

    [Header("Princess")]
    [SerializeField] private bool hasPrincess = false;

    public void AddKey(KeyColor keyColor)
    {
        switch (keyColor)
        {
            case KeyColor.Red:
                redKeys++;
                break;

            case KeyColor.Blue:
                blueKeys++;
                break;

            case KeyColor.Green:
                greenKeys++;
                break;
        }

        Debug.Log($"Collected one {keyColor} key.");

        Debug.Log(
            $"Keys — Red: {redKeys}, " +
            $"Blue: {blueKeys}, " +
            $"Green: {greenKeys}"
        );
    }

    public bool UseKey(KeyColor keyColor)
    {
        switch (keyColor)
        {
            case KeyColor.Red:
                if (redKeys <= 0) return false;
                redKeys--;
                break;

            case KeyColor.Blue:
                if (blueKeys <= 0) return false;
                blueKeys--;
                break;

            case KeyColor.Green:
                if (greenKeys <= 0) return false;
                greenKeys--;
                break;
        }

        Debug.Log($"Used one {keyColor} key.");
        return true;
    }

    public bool HasPrincess
    {
        get { return hasPrincess; }
    }

    public void CollectPrincess()
    {
        if (hasPrincess)
        {
            return;
        }

        hasPrincess = true;
        Debug.Log("The player now has the princess.");
    }
}