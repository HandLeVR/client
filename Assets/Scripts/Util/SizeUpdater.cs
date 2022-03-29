using UnityEngine;

/// <summary>
/// Ensures that the element fully fills the parent without changing the aspect ratio if the element.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class SizeUpdater : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        float size = Mathf.Max(parentRectTransform.rect.width, parentRectTransform.rect.height);
        rectTransform.sizeDelta = new Vector2(size,size);
    }
}
