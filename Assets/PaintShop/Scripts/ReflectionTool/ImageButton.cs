using UnityEngine.UI;

public class ImageButton : Button
{
    public Image buttonImage;

    protected override void Awake()
    {
        base.Awake();
        buttonImage = transform.GetChild(0).GetComponent<Image>();
    }
}
