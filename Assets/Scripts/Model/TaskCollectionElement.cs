
public class TaskCollectionElement 
{
    public long id;
    public int index;
    public bool mandatory;
    public Task task;

    public TaskCollectionElement()
    {
    }

    public TaskCollectionElement(long id, int index, bool mandatory, Task task)
    {
        this.id = id;
        this.index = index;
        this.mandatory = mandatory;
        this.task = task;
    }
}
