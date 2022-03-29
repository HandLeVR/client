using TMPro;
using translator;
using UnityEngine;

/// <summary>
/// Provides a preview for tasks.
/// </summary>
public class TaskSelectionPreview : MonoBehaviour
{
    public TMP_InputField imageNameInputField;
    public TMP_InputField descriptionInputField;
    public TMP_InputField authorInputField;
    public TMP_InputField taskClassInputField;
    public TMP_InputField numSubtasksInputField;
    public TMP_InputField dateInputField;

    private void Awake()
    {
        SetUpPreviewPanel(null, true);
    }

    private void OnEnable()
    {
        ClearPreview();
        dateInputField.text = "";
    }

    public void SetUpPreviewPanel(Task task, bool empty = false)
    {
        if (!empty)
        {
            imageNameInputField.text = task.name;
            descriptionInputField.text = task.description != null ? task.description : "-";
            descriptionInputField.textComponent.enableWordWrapping = true;
            descriptionInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "keine Beschreibung vorhanden";
            authorInputField.text = task.permission.createdByFullName;
            taskClassInputField.text = TranslationController.Instance.Translate(task.taskClass.ToString());
            numSubtasksInputField.text = task.subTasks.Count.ToString();
        }
        else
            ClearPreview();
        dateInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl eines Abgabezeitpunkts";
        dateInputField.onSelect.RemoveAllListeners();
        dateInputField.onSelect.AddListener(_ => DeadlinePicker.Instance.Init(dateInputField));
    }

    public void SetDeadlineSelectionActive(bool active)
    {
        dateInputField.transform.parent.parent.gameObject.SetActive(active);
    }

    private void ClearPreview()
    {
        imageNameInputField.text = "";
        descriptionInputField.text = "";
        authorInputField.text = "";
        taskClassInputField.text = "";
        numSubtasksInputField.text = "";
    }
}