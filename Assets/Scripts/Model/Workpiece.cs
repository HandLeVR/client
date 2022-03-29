/// <summary>
/// Represents a workpiece.
/// </summary>
public class Workpiece
{
    public long id;
    public Permission permission;
    public string name;
    public string data;

    public Workpiece()
    {
    }

    public Workpiece(long id, string name, string data)
    {
        this.id = id;
        this.name = name;
        this.data = data;
    }
}
