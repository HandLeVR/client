using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a task entry in the task list of the task selection panel.
/// </summary>
public class TaskContainer : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Toggle finishedToggle;
    public Button executeButton;
    public GameObject HierarchyVisualization;

    private GameObject HierarchyVisualizationMiddle;
    private GameObject HierarchyVisualizationEnd;

    public void SetToLastElement()
    {
        HierarchyVisualization.SetActive(true);
        HierarchyVisualizationMiddle.SetActive(false);
        HierarchyVisualizationEnd.SetActive(true);
    }
    
    public void Instantiate(TaskAssignment taskAssignment, bool executable, bool isSingleTask = false)
    {
        executeButton.interactable = executable;
        HierarchyVisualization.SetActive(!isSingleTask);
        
        HierarchyVisualizationMiddle = HierarchyVisualization.transform.Find("Middle Part").gameObject;
        HierarchyVisualizationEnd = HierarchyVisualization.transform.Find("Last Part").gameObject;
        HierarchyVisualizationEnd.SetActive(false);
        
        // for own tasks there are never task results
        if (taskAssignment.taskResults != null)
            finishedToggle.isOn = taskAssignment.taskResults.Count > 0;
        
        if (taskAssignment.task.IsSupportInfo())
        {
            title.text = "(U) " + taskAssignment.task.name;
            executeButton.GetComponentInChildren<TextMeshProUGUI>().text = finishedToggle.isOn ? "Wiederholen" : "Anschauen";
        }
        else
        {
            title.text = (taskAssignment.task.partTaskPractice ? "(Ü) " : "(A) ") + taskAssignment.task.name;
            executeButton.GetComponentInChildren<TextMeshProUGUI>().text = finishedToggle.isOn ? "Wiederholen" : "Durchführen";
        }

        executeButton.onClick.AddListener(() => LearningScenariosTaskController.Instance.LoadTask(taskAssignment));
    }
}
