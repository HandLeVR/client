using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TaskAssignment
{
    public long id;
    public User user;
    public Task task;
    public TaskCollectionAssignment taskCollectionAssignment;
    public UserGroupTaskAssignment userGroupTaskAssignment;
    public List<TaskResult> taskResults;
    public DateTime? deadline;

    public TaskAssignment()
    {
    }

    public TaskAssignment(long id, User user, Task task, TaskCollectionAssignment taskCollectionAssignment,
        UserGroupTaskAssignment userGroupTaskAssignment, DateTime? deadline)
    {
        this.id = id;
        this.user = user;
        this.task = task;
        this.taskCollectionAssignment = taskCollectionAssignment;
        this.userGroupTaskAssignment = userGroupTaskAssignment;
        this.deadline = deadline;
    }
}