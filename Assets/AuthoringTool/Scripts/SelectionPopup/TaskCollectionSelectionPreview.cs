using TMPro;
using translator;
using UnityEngine;

/// <summary>
/// Provides a preview for task collections.
/// </summary>
public class TaskCollectionSelectionPreview : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_InputField authorInputField;
    public TMP_InputField taskClassInputField;
    public TMP_InputField numTaskCollectionElementsInputField;
    public TMP_InputField dateInputField;

    private void Awake()
    {
        SetUpPreviewPanel(null);
    }

    public void SetUpPreviewPanel(TaskCollection tc)
    {
        if (tc != null)
        {
            nameInputField.text = tc.name;
            descriptionInputField.text = tc.description != null ? tc.description : "-";
            descriptionInputField.textComponent.enableWordWrapping = true;
            descriptionInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "keine Beschreibung vorhanden";
            authorInputField.text = tc.permission?.createdByFullName != null ? tc.permission?.createdByFullName : "-";
            taskClassInputField.text = TranslationController.Instance.Translate(tc.taskClass.ToString());
            numTaskCollectionElementsInputField.text = tc.taskCollectionElements.Count.ToString();
        }
        else
            ClearPreview();
        dateInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl eines Abgabezeitpunkts";
        dateInputField.text = "";
        dateInputField.onSelect.RemoveAllListeners();
        dateInputField.onSelect.AddListener(_ => DeadlinePicker.Instance.Init(dateInputField));
    }

    private void ClearPreview()
    {
        nameInputField.text = "";
        descriptionInputField.text = "";
        authorInputField.text = "";
        taskClassInputField.text = "";
        numTaskCollectionElementsInputField.text = "";
    }
}