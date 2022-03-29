/// <summary>
/// Implements the dragging functionality for sub task elements.
/// </summary>
public class SubTaskDragHandler : BaseDragHandler
{
    /// <summary>
    /// Clones the sub task if dragged from the original sub task panel but not if dragged from the currently used
    /// sub tasks panel.
    /// </summary>
    protected override void OnBeginDragImpl()
    {
        SubTaskController.Instance.currentSelection = gameObject.GetComponent<BaseSubTaskContainer>();
        if (transform.parent == SubTaskController.Instance.availableSubTasksPanel)
            SubTaskController.Instance.CloneSubTask(transform.GetSiblingIndex());
    }
    
    protected override bool IsSupportedDropAreaHandle(BaseDropArea dropAreaHandler)
    {
        return dropAreaHandler is SubTaskDropArea;
    }

    protected override void OnEndDragImpl()
    {
        SubTaskController.Instance.currentSelection = null;
        DestroyImmediate(gameObject);
        SubTaskController.Instance.UpdateSubTaskNumbers();
    }
}
