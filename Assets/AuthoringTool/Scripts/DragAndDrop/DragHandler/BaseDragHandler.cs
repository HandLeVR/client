using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the basic dragging functionality for draggable elements. There are specialized sub classes for the
/// different draggable elements.
/// </summary>
public abstract class BaseDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static GameObject draggedObject; // Drag only one Object

    protected BaseDropArea lastDropAreaHandler;

    /// <summary>
    /// Sets some general properties and calls the OnBeginDragImpl of the specialized child classes.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragImpl();

        draggedObject = gameObject;
        draggedObject.transform.SetParent(MainScreenController.Instance.transform);
        draggedObject.gameObject.layer = 1;
        draggedObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    protected abstract void OnBeginDragImpl();

    /// <summary>
    /// Is called while dragging and is used to show the preview if possible.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        BaseDropArea dropAreaHandler = null;
        if (eventData.pointerEnter != null)
            dropAreaHandler = eventData.pointerEnter.GetComponentInParent<BaseDropArea>();
        if (dropAreaHandler != null && IsSupportedDropAreaHandle(dropAreaHandler))
            dropAreaHandler.ShowPreview(eventData.position);
        else if (lastDropAreaHandler != null)
            lastDropAreaHandler.HidePreview();
        lastDropAreaHandler = dropAreaHandler;
    }

    protected abstract bool IsSupportedDropAreaHandle(BaseDropArea dropAreaHandler);

    /// <summary>
    /// Is called at the end of the drag and is used for cleanup. The specialized sub classes handle the creation of
    /// new elements in the OnEndDragImpl method.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        draggedObject = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (lastDropAreaHandler)
            lastDropAreaHandler.preview.SetActive(false);
        OnEndDragImpl();
    }
    
    protected abstract void OnEndDragImpl();
}