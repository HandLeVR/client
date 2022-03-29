using Newtonsoft.Json;

/// <summary>
/// Instance of an SubTaskType. A Task consists of one or more sub tasks.
/// </summary>
public class SubTask
{
    public string name;
    public string description;
    public string type;
    public string properties;

    public SubTask()
    {
    }

    public SubTask(string name, string description, string type, string properties)
    {
        this.name = name;
        this.description = description;
        this.type = type;
        this.properties = properties;
    }

    public SubTask Copy()
    {
        return new SubTask(name, description, type, properties);
    }
}
