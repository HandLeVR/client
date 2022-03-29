using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a row an the task result table. Clicking on the row allows to lead the according recording.
/// </summary>
public class TaskResultRow : MonoBehaviour
{
    public GameObject dateCell;
    public GameObject timeCell;
    public GameObject durationCell;
    public GameObject deleteCell;
    public Color selectedColor;

    [HideInInspector] public Task task;

    private TextMeshProUGUI dateText;
    private TextMeshProUGUI timeText;
    private TextMeshProUGUI durationText;
    private Color unselectedColor;
    private int index;
    
    private readonly List<Image> images = new List<Image>();
    
    void Awake()
    {
        dateText = dateCell.GetComponentInChildren<TextMeshProUGUI>();
        timeText = timeCell.GetComponentInChildren<TextMeshProUGUI>();
        durationText = durationCell.GetComponentInChildren<TextMeshProUGUI>();
        images.Add(dateCell.GetComponent<Image>());
        images.Add(timeCell.GetComponent<Image>());
        images.Add(durationCell.GetComponent<Image>());
        images.Add(deleteCell.GetComponent<Image>());
        unselectedColor = images[0].color;
    }

    public void Init(Task task, DateTime date, float time, int i, UnityAction<int> onClick, UnityAction<int, bool> deleteResult)
    {
        this.task = task;
        dateText.text = date.ToString("dd.MM.yyyy");
        timeText.text = date.ToString("HH:mm:ss");
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        durationText.text = timeSpan.ToString("hh':'mm':'ss");
        index = i;
        GetComponent<Button>().onClick.AddListener(() => onClick(index));
        deleteCell.GetComponentInChildren<Button>().onClick.AddListener(() => deleteResult(index, false));
    }

    public void Select()
    {
        images.ForEach(image => image.color = selectedColor);
    }

    public void Deselect()
    {
        images.ForEach(image => image.color = unselectedColor);
    }
}
