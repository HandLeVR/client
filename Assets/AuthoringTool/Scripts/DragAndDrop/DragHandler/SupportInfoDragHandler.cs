/// <summary>
/// Implements the dragging functionality for support info elements.
/// </summary>
public class SupportInfoDragHandler : BaseDragHandler
{
    /// <summary>
    /// Clones a support info if it is dragged from the original support info panel but not if dragged from the support
    /// info task scroll view.
    /// </summary>
    protected override void OnBeginDragImpl()
    {
        SupportInfoController.Instance.currentSelection = gameObject.GetComponent<BaseSupportInfoContainer>();
        if (transform.parent == SupportInfoController.Instance.supportInfoContainer)
            SupportInfoController.Instance.CloneSupportInfo(transform.GetSiblingIndex());
    }
    
    protected override bool IsSupportedDropAreaHandle(BaseDropArea dropAreaHandler)
    {
        return dropAreaHandler is SupportInfoDropArea;
    }

    protected override void OnEndDragImpl()
    {
        SupportInfoController.Instance.currentSelection = null;
        DestroyImmediate(gameObject);
    }
}
