using TMPro;
using UnityEngine;

/// <summary>
/// Ensures that the height if the corresponding rect transform matches the height of the text.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextHeightFitter : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private RectTransform _rectTransform;
    
    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        _rectTransform.sizeDelta = new Vector2 (_rectTransform.sizeDelta.x, _text.textBounds.size.y);;
    }
}
