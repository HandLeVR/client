/// <summary>
/// A supportive information represent information (e.g. in form of a video, image or audio) that can be used in a task.
/// </summary>
public class SupportInfo
{
    public string name;
    public string description;
    public string type;
    public string properties;

    public SupportInfo()
    {
    }

    public SupportInfo(string name, string description, string type, string properties)
    {
        this.name = name;
        this.description = description;
        this.type = type;
        this.properties = properties;
    }

    public SupportInfo Copy()
    {
        return new SupportInfo(name, description, type, properties);
    }
}
