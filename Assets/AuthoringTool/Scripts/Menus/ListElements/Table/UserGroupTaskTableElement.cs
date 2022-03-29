using UnityEngine.Events;

/// <summary>
/// Represents a task assigned to a user group in the user group menu.
/// </summary>
public class UserGroupTaskTableElement : BasicTableElement
{
    public UserGroupTaskAssignment userGroupTaskAssignment;

    public void Init(UserGroupTaskAssignment userGroupTaskAssignment, UnityAction onDelete)
    {
        this.userGroupTaskAssignment = userGroupTaskAssignment;

        text1.text = userGroupTaskAssignment.task != null
            ? userGroupTaskAssignment.task.name
            : userGroupTaskAssignment.taskCollection.name;
        text2.text = userGroupTaskAssignment.deadline != null
            ? userGroupTaskAssignment.deadline.GetValueOrDefault().ToString("dd.MM.yyyy HH:mm:ss")
            : "-";
        text3.text = userGroupTaskAssignment.task != null ? "Aufgabe" : "Aufgabensammlung";
        text4.text = userGroupTaskAssignment.task != null
            ? "1"
            : userGroupTaskAssignment.taskCollection.taskCollectionElements.Count.ToString();
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(onDelete);
        infoButton.gameObject.SetActive(false);
    }
}