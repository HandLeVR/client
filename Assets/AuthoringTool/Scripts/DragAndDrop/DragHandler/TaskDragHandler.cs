using UnityEngine;

/// <summary>
/// Implements the dragging functionality for task elements.
/// </summary>
public class TaskDragHandler : BaseDragHandler
{
    private Transform _originalParent;
    private int _originalSiblingIndex;
    
    protected override void OnBeginDragImpl()
    {
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();
    }

    protected override bool IsSupportedDropAreaHandle(BaseDropArea dropAreaHandler)
    {
        return dropAreaHandler is TaskCollectionMenuDropArea;
    }

    protected override void OnEndDragImpl()
    {
        if (!lastDropAreaHandler)
            RestoreOriginalPosition();
        else
            DestroyImmediate(gameObject);
    }

    private void RestoreOriginalPosition()
    {
        gameObject.transform.SetParent(_originalParent);
        gameObject.transform.SetSiblingIndex(_originalSiblingIndex);
    }
}
