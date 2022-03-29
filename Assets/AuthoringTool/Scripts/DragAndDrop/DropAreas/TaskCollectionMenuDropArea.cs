
/// <summary>
/// Handles dropping task collection elements in the task collection menu.
/// </summary>
public class TaskCollectionMenuDropArea : BaseDropArea
{
    public TaskCollectionMenuController taskCollectionMenuController;
    
    protected override void OnDropImpl(int index)
    {
        if (BaseDragHandler.draggedObject.GetComponent<BasicAssignableElement>() != null)
            taskCollectionMenuController.HandleDrop(index);
    }
}


