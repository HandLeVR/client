using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows to display a image in the preview panel as fullscreen.
/// </summary>
public class MediaPreviewFullscreenImage : MonoBehaviour
{
    public TextMeshProUGUI fileNameTMP;
    public Image image;

    public void Init(string filename, Texture2D texture)
    {
        fileNameTMP.text = filename;
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100f);
        image.preserveAspect = true;
    }
}