using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class DummyElementWidthUpdater : MonoBehaviour
{
    private LayoutElement layoutElement;
    private HorizontalLayoutGroup parentLayoutGroup;
    private RectTransform parentRectTransform;
    
    private void Start()
    {
        layoutElement = GetComponent<LayoutElement>();
        parentLayoutGroup = transform.parent.GetComponent<HorizontalLayoutGroup>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        layoutElement.preferredWidth = (parentRectTransform.rect.width - (parentLayoutGroup.padding.left +
                                                                          parentLayoutGroup.padding.right +
                                                                          parentLayoutGroup.spacing)) / 2;
    }
}
