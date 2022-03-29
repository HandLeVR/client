using UnityEngine.Events;

/// <summary>
/// Represents a user group entry in the user group menu.
/// </summary>
public class UserGroupTableElement : BasicTableElement
{
    public UserGroup userGroup;

    public void Init(UserGroup userGroup, UnityAction<UserGroupTableElement, bool> onClick,
        UnityAction<UserGroupTableElement, bool> onDelete)
    {
        this.userGroup = userGroup;
        text1.text = userGroup.name;
        text2.text = userGroup.users.Count.ToString();
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this, false));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false));
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(userGroup.permission, true));
    }

    public void UpdateUserCount()
    {
        text2.text = userGroup.users.Count.ToString();
    }
}