using translator;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextTranslation : MonoBehaviour
{
    private Text _textMeshProUGUI;

    public string id;
    
    void Awake()
    {
        _textMeshProUGUI = GetComponent<Text>();
    }

    private void Start()
    {
        _textMeshProUGUI.text = TranslationController.Instance.TranslateWithDefault(id, _textMeshProUGUI.text);
    }
}
