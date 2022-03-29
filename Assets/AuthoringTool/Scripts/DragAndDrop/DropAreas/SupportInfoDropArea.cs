using UnityEngine;

/// <summary>
/// Drop area for support info elements.
/// </summary>
public class SupportInfoDropArea : BaseDropArea
{
    private int _maxNumSupportiveInfos = 6;

    protected override void OnDropImpl(int index)
    {
        BaseSupportInfoContainer supportInfoContainer =
            BaseDragHandler.draggedObject.GetComponent<BaseSupportInfoContainer>();
        if (supportInfoContainer == null || contentContainer.transform.childCount > _maxNumSupportiveInfos)
            return;

        GameObject lastDrop = SupportInfoController.Instance.CreateSupportInfoSettings(
            supportInfoContainer.supportInfoData, contentContainer, true);
        lastDrop.transform.SetParent(contentContainer);
        lastDrop.transform.SetSiblingIndex(index);
        lastDrop.gameObject.layer = 1;
        lastDrop.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}