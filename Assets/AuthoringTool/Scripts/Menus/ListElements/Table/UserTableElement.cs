using translator;
using UnityEngine.Events;

/// <summary>
/// Represents a user entry in the user menu.
/// </summary>
public class UserTableElement : BasicTableElement
{
    public User user;

    public void Init(User user, UnityAction<UserTableElement, bool> onClick,
        UnityAction<UserTableElement, bool> onDelete)
    {
        this.user = new User(user);

        text1.text = user.fullName;
        text2.text = user.userName;
        text3.text = TranslationController.Instance.Translate(user.role.ToString());
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this, false));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false));
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(user.permission, true, user));
        if (DataController.Instance.CurrentUser.role != User.Role.Teacher)
            deleteButton.interactable = false;
    }
}