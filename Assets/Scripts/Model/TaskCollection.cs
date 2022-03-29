using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// A TaskCollection is a summary of tasks belonging to the same task class.
/// </summary>
public class TaskCollection
{
    public long id;
    public Permission permission;
    public string name;
    public string description;
    public TaskClass taskClass;
    public List<TaskCollectionElement> taskCollectionElements = new List<TaskCollectionElement>();
    public List<TaskCollectionAssignment> taskCollectionAssignments = new List<TaskCollectionAssignment>();

    public TaskCollection()
    {
    }

    public TaskCollection(TaskCollection taskCollection)
    {
        id = taskCollection.id;
        name = taskCollection.name;
        description = taskCollection.description;
        taskClass = taskCollection.taskClass;
        taskCollectionElements = new List<TaskCollectionElement>(taskCollection.taskCollectionElements);
    }

    public void SortTaskCollectionElements()
    {
        taskCollectionElements.Sort((e1, e2) => e1.index.CompareTo(e2.index));
    }
}