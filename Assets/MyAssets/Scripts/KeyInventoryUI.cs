using TMPro;
using UnityEngine;

public class KeyInventoryUI : MonoBehaviour
{
    [Tooltip("The player's key inventory.")]
    public KeyInventory keyInventory;

    [Tooltip("The text used to display key counts.")]
    public TMP_Text keyCounterText;

    private void Update()
    {
        if (keyInventory == null || keyCounterText == null)
        {
            return;
        }

        keyCounterText.text =
            $"<color=#FF5555>Red Key: {keyInventory.redKeys}</color>\n" +
            $"<color=#5599FF>Blue Key: {keyInventory.blueKeys}</color>\n" +
            $"<color=#55DD77>Green Key: {keyInventory.greenKeys}</color>";
    }
}