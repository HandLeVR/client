using TMPro;

/// <summary>
/// Represents a sub task list element for the task menu.
/// </summary>
public class SubTaskAssignableElement : BasicAssignableElement
{
    public void Init(SubTask subtask)
    {
        indexLabel.text = (transform.GetSiblingIndex() + 1).ToString();
        nameInputField.text = subtask.name;
        SetSubTaskTMPSettings();
    }
    
    /// <summary>
    /// Workaround to solve weird formatting after container is created
    /// </summary>
    private void SetSubTaskTMPSettings()
    {
        nameInputField.textComponent.alignment = TextAlignmentOptions.Left;
        descriptionInputField.textComponent.alignment = TextAlignmentOptions.Left;
    }
}
