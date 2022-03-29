using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// The start panel displayed after selecting a task.
/// </summary>
public class TaskPanel : MonoBehaviour
{
    public TextMeshProUGUI taskNameText;
    public GameObject startTaskButton;
    public GameObject redoTaskButton;
    public Button evaluationButton;
    
    private const string TaskNameTemplate = "Aufgabe:\n {0}";

    public void InitPanel(string taskName, bool taskFinished, bool showEvaluationButton, UnityAction evaluationButtonAction)
    {
        taskNameText.text = String.Format(TaskNameTemplate, taskName);
        startTaskButton.SetActive(!taskFinished);
        redoTaskButton.SetActive(taskFinished);
        evaluationButton.gameObject.SetActive(showEvaluationButton);

        if (taskFinished)
        {
            evaluationButton.interactable = true;
            evaluationButton.onClick.RemoveAllListeners();
            evaluationButton.onClick.AddListener(evaluationButtonAction);
        }
        else
        {
            evaluationButton.interactable = false;
        }
    }
}
