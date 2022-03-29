using System;

public class UserGroupTaskAssignment
{
    public long id;
    public UserGroup userGroup;
    public Task task;
    public TaskCollection taskCollection;
    public DateTime? deadline;

    public UserGroupTaskAssignment()
    {
    }

    public UserGroupTaskAssignment(long id, UserGroup userGroup, Task task, TaskCollection taskCollection,
        DateTime? deadline)
    {
        this.id = id;
        this.userGroup = userGroup;
        this.task = task;
        this.taskCollection = taskCollection;
        this.deadline = deadline;
    }
}