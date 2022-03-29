using TMPro;
using translator;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshProTranslation : MonoBehaviour
{
    private TextMeshProUGUI _textMeshProUGUI;

    public string id;
    
    void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        _textMeshProUGUI.text = TranslationController.Instance.TranslateWithDefault(id, _textMeshProUGUI.text);
    }
}
