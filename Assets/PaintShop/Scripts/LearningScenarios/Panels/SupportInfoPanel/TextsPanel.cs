using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Allows to display texts on the monitor.
/// </summary>
public class TextsPanel : SlidePanel
{
    public TextMeshProUGUI textField;
    [HideInInspector] public List<string> texts;

    protected override int GetSlideCount()
    {
        return texts.Count;
    }

    protected override void SetSlideContent()
    {
        headerField.text = "";
        textField.text = texts[currentPage];
    }
}
