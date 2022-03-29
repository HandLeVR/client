using UnityEngine;

/// <summary>
/// Sets the size of the collider to the size of the corresponding RectTransform.
/// </summary>
public class ColliderSizeUpdater : MonoBehaviour
{
    private RectTransform rectTransform;
    private BoxCollider col;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        col = GetComponent<BoxCollider>();
    }

    void Update()
    {
        // we get a warning if the collider size is negative
        // (probably happens in the first frame after initialization)
        if (rectTransform.rect.width > 0)
            col.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, col.size.z);
    }
}
