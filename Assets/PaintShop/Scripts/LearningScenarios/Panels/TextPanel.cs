using TMPro;
using UnityEngine;

/// <summary>
/// Displays a text on the monitor.
/// </summary>
public class TextPanel : MonoBehaviour
{
    public TextMeshProUGUI introductionText;

    public void InitPanel(string text)
    {
        introductionText.text = text;
    }
}
