using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Represents a task consisting of one ore multiple sub tasks.
/// </summary>
/// 
public class Task : IEquatable<Task>
{
    public long id;
    public Permission permission;
    public string name;
    public string description;
    public TaskClass taskClass;
    [JsonConverter(typeof(SubTaskJsonConverter))]
    public List<SubTask> subTasks = new List<SubTask>();
    public bool partTaskPractice;
    public bool valuesMissing;
    public HashSet<Coat> usedCoats;
    public HashSet<Recording> usedRecordings;
    public HashSet<Media> usedMedia;
    public HashSet<Workpiece> usedWorkpieces;

    public Task()
    {
    }

    public Task(long id, string name, string description, TaskClass taskClass, bool partTaskPractice, List<SubTask> subTasks)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.taskClass = taskClass;
        this.partTaskPractice = partTaskPractice;
        this.subTasks = subTasks;
    }
    
    /// <summary>
    /// Returns true if the task only contains support info sub tasks.
    /// </summary>
    public bool IsSupportInfo()
    {
        return subTasks.All(subTask => subTask.type == "Supportive Information Summary");
    }

    /// <summary>
    /// Returns true if the task contains a paint workpiece task.
    /// </summary>
    public bool HasPaintingTask()
    {
        return subTasks.Any(subTask => subTask.type == "Paint Workpiece");
    }
    
    public override int GetHashCode() {
        return id.GetHashCode();
    }
    
    public override bool Equals(object obj) {
        return Equals(obj as Task);
    }
    
    public bool Equals(Task other)
    {
        return other != null && other.id == id;
    }
}

public enum TaskClass
{
    NewPartPainting,
    RefinishingJob,
    SpotRepair
}
