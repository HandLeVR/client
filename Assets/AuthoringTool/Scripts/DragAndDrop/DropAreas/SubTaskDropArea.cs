using UnityEngine;

/// <summary>
/// Drop area for sub task elements.
/// </summary>
public class SubTaskDropArea : BaseDropArea
{
    protected override void OnDropImpl(int index)
    {
        BaseSubTaskContainer subTaskContainer = BaseDragHandler.draggedObject.GetComponent<BaseSubTaskContainer>();
        if (subTaskContainer == null)
            return;

        BaseDragHandler.draggedObject.transform.SetParent(contentContainer);
        BaseSubTaskContainer container =
            SubTaskController.Instance.CreateSubTaskComponent(subTaskContainer.SubTaskData, contentContainer);
        GameObject lastDrop = container.gameObject;
        lastDrop.transform.SetSiblingIndex(index);
        lastDrop.gameObject.layer = 1;
        lastDrop.GetComponent<CanvasGroup>().blocksRaycasts = true;

        // the original container is removed therefore we need to reload the settings panel if the corresponding sub task was active
        if (subTaskContainer.IsCurrentSubTask())
            TaskController.Instance.ShowCorrespondingSettingPanel(subTaskContainer.SubTaskData.type, container);
    }

    protected override void AfterDropFinished()
    {
        SubTaskController.Instance.UpdateSubTaskNumbers();
    }
}