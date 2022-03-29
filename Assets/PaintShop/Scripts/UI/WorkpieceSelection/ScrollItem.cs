using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item in the scroll component containing a preview of a workpiece.
/// </summary>
public class ScrollItem : MonoBehaviour
{
    public Workpiece relatedWorkpiece;
    public GameObject bigPreview;
    public Text bigPreviewName;
    public Sprite sprite;
    
    [HideInInspector] public ComponentScroll componentScroll;

    void Awake()
    {
        transform.GetComponent<Button>().onClick.AddListener(ShowBigPreviewPic);
        bigPreview = GameObject.Find("Big Component Preview");
        bigPreviewName = GameObject.Find("Big Preview Name").GetComponent<Text>();
    }

    public void ShowBigPreviewPic()
    {
        bigPreview.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        componentScroll.currentWorkpiece = relatedWorkpiece;
        Debug.Log(relatedWorkpiece.name);
        bigPreviewName.text = TranslationController.Instance.Translate("paint-shop-workpiece", relatedWorkpiece.name);
    }
}
