using System;
using Newtonsoft.Json;

public class TaskResult
{
    public long id;
    public DateTime date;
    public TaskAssignment taskAssignment;
    [JsonIgnore]
    public Recording recording;

    public TaskResult(long id, DateTime date, TaskAssignment taskAssignment, Recording recording)
    {
        this.id = id;
        this.date = date;
        this.taskAssignment = taskAssignment;
        this.recording = recording;
    }
}
