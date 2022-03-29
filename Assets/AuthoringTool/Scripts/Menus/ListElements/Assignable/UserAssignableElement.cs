using TMPro;
using UnityEngine.Events;

/// <summary>
/// Represents a user list element in the user group menu.
/// </summary>
public class UserAssignableElement : BasicAssignableElement
{
    public User user;
    
    public void Init(User user, UnityAction onAdd, UnityAction onDelete)
    {
        this.user = user;
        
        nameInputField.text = user.fullName;

        if (onDelete != null)
            deleteButton.onClick.AddListener(onDelete);
        else
            deleteButton.gameObject.SetActive(false);
        
        if (onAdd != null)
            addButton.onClick.AddListener(onAdd);
        else
            addButton.gameObject.SetActive(false);
        
        nameInputField.textComponent.alignment = TextAlignmentOptions.Left;
        descriptionInputField.textComponent.alignment = TextAlignmentOptions.Left;
    }
}
