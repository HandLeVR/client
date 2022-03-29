using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a preview for a coat.
/// </summary>
public class CoatSelectionPreview : MonoBehaviour
{
    public TMP_InputField imageNameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_InputField typeInputField;
    public Image colorPreview;

    private void OnEnable()
    {
        ClearPreview();
    }

    public void SetUpPreviewPanel(Coat coat)
    {
        imageNameInputField.text = coat.name;
        descriptionInputField.gameObject.SetActive(coat.description.Length > 0);
        descriptionInputField.text = coat.description;
        descriptionInputField.textComponent.enableWordWrapping = true;
        typeInputField.text = TranslationController.Instance.Translate(coat.type.ToString());
        colorPreview.color = coat.color;
    }

    private void ClearPreview()
    {
        imageNameInputField.text = "";
        descriptionInputField.text = "";
        typeInputField.text = "";
        colorPreview.color = new Color(1, 1, 1, 0);
    }
}