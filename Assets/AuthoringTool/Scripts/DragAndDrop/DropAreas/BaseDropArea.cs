using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a area where elements can be dropped on. Handles displaying a preview at the target location.
/// OnDropImpl needs to be implemented by specialized child classes.
/// </summary>
public abstract class BaseDropArea : MonoBehaviour, IDropHandler
{
    public Transform contentContainer;
    public GameObject preview;

    private ScrollRect _scrollRect;

    private void Awake()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        int index = GetIndex(eventData.position);
        HidePreview();

        if (BaseDragHandler.draggedObject != null)
            OnDropImpl(index);

        AfterDropFinished();
    }
    
    protected abstract void OnDropImpl(int index);

    /// <summary>
    /// Is called after the drop is finished.
    /// </summary>
    protected virtual void AfterDropFinished()
    {
    }

    /// <summary>
    /// Shows the preview on dependence of the given screen position. Also deactivates the buttons of the elements
    /// to avoid visual clutter. 
    /// </summary>
    public void ShowPreview(Vector3 position)
    {
        preview.SetActive(true);
        preview.transform.SetSiblingIndex(GetIndex(position));
        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Hides the preview and activates the buttons of the elements.
    /// </summary>
    public void HidePreview()
    {
        preview.SetActive(false);
        SetButtonsInteractable(true);
    }

    /// <summary>
    /// Determines the index of the element on dependence of the given screen position.
    /// </summary>
    private int GetIndex(Vector3 position)
    {
        int index = 0;
        bool previewFound = false;
        for (int i = 0; i < contentContainer.childCount; i++)
        {
            if (contentContainer.GetChild(i).GetComponent<BaseDragHandler>() == null)
            {
                previewFound = true;
                continue;
            }

            if (position.y < contentContainer.GetChild(i).position.y)
                index = i + (previewFound ? 0 : 1);
        }

        return index;
    }

    /// <summary>
    /// Sets the buttons of the elements (in)active if they have one.
    /// </summary>
    private void SetButtonsInteractable(bool active)
    {
        foreach (Transform obj in contentContainer)
            if (obj.GetComponent<Button>())
                obj.GetComponent<Button>().interactable = active;
    }

    /// <summary>
    /// Scrolls to the active element.
    /// </summary>
    public void ScrollTo(RectTransform activeElement)
    {
        if (activeElement.transform.position.y > _scrollRect.viewport.position.y ||
            _scrollRect.viewport.transform.TransformPoint(activeElement.localPosition).y - activeElement.rect.size.y <
            0)
        {
            Canvas.ForceUpdateCanvases();
            // first child
            if (activeElement.transform.GetSiblingIndex() == 0)
                _scrollRect.verticalNormalizedPosition = 1f;
            // last child
            else if (activeElement.transform.GetSiblingIndex() == contentContainer.childCount - 1)
                _scrollRect.verticalNormalizedPosition = 0f;
            // somewhere in between
            else
            {
                float n = Mathf.Abs(_scrollRect.content.transform.InverseTransformPoint(activeElement.position).y /
                                    _scrollRect.content.rect.height);
                _scrollRect.verticalNormalizedPosition = 1f - (n);
            }
        }
    }
}