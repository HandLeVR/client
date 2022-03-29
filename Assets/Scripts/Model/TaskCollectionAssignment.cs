using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TaskCollectionAssignment : IEquatable<TaskCollectionAssignment>
{
    public long id;
    public User user;
    public TaskCollection taskCollection;
    public UserGroupTaskAssignment userGroupTaskAssignment;
    public DateTime? deadline;
    
    [JsonIgnore]
    public List<TaskAssignment> taskAssignments;

    public TaskCollectionAssignment()
    {
    }

    public TaskCollectionAssignment(long id, TaskCollection taskCollection, DateTime? deadline)
    {
        this.id = id;
        this.taskCollection = taskCollection;
        this.deadline = deadline;
    }
    
    public override int GetHashCode() {
        return id.GetHashCode();
    }
    
    public override bool Equals(object obj) {
        return Equals(obj as TaskCollectionAssignment);
    }
    
    public bool Equals(TaskCollectionAssignment other)
    {
        return other != null && other.id == id;
    }
}