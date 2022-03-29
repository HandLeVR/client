using UnityEngine.Events;

/// <summary>
/// Represents a media entry in the media menu.
/// </summary>
public class MediaTableElement : BasicTableElement
{
    public Media media;

    public void Init(Media media, UnityAction<MediaTableElement, bool> onClick,
        UnityAction<MediaTableElement, bool> onDelete)
    {
        this.media = media;
        text1.text = media.name;
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this, false));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false));
        deleteButton.gameObject.SetActive(media.permission.editable);
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(media.permission, true));
    }
}
