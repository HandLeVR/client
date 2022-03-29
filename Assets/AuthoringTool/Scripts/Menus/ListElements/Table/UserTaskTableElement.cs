using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// This class is responsible for displaying all tasks (task itself and those from an assigned task collection)
/// that have been assigned to a user. 
/// </summary>
public class UserTaskTableElement : BasicTableElement
{
    public Button evaluationButton;

    public TaskAssignment taskAssignment;
    public TaskCollectionAssignment taskCollectionAssignment;

    public void Init(TaskAssignment taskAssignment, TaskCollectionAssignment taskCollectionAssignment, UserGroup group,
        List<TaskResult> taskResults, UnityAction onEvaluation, UnityAction onRemove)
    {
        this.taskAssignment = taskAssignment;
        this.taskCollectionAssignment = taskCollectionAssignment;

        text1.text = taskAssignment.task.name;
        text2.text = taskAssignment.deadline != null
            ? taskAssignment.deadline.GetValueOrDefault().ToString("dd.MM.yyyy HH:mm:ss")
            : "-";
        text3.text = taskResults != null ? taskResults.Count.ToString() : "0";
        text4.text = taskResults != null && taskResults.Count > 0
            ? taskResults.OrderByDescending(result => result.date).ToList()[0].date.ToString("dd.MM.yyyy HH:mm:ss")
            : "-";
        text5.text = taskCollectionAssignment != null ? taskCollectionAssignment.taskCollection.name : "-";
        text6.text = group != null ? group.name : "-";
        evaluationButton.onClick.AddListener(onEvaluation);
        if (taskResults != null && taskResults.Count > 0)
            deleteButton.gameObject.SetActive(false);
        else
            deleteButton.onClick.AddListener(onRemove);
        infoButton.gameObject.SetActive(false);
    }
}