using translator;
using UnityEngine.Events;

/// <summary>
/// Represents a coat entry in the coat menu.
/// </summary>
public class CoatTableElement : BasicTableElement
{
    public Coat coat;
 
    public void Init(Coat coat, UnityAction<CoatTableElement, bool> onClick, UnityAction<CoatTableElement, bool> onDelete)
    {
        this.coat = coat;
        text1.text = coat.name;
        text2.text = TranslationController.Instance.Translate(coat.type.ToString());
        image.color = coat.color;
        containerButton.onClick.RemoveAllListeners();
        containerButton.onClick.AddListener(() => onClick(this, false));
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete(this, false));
        deleteButton.gameObject.SetActive(coat.permission.editable);
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(() => PopupScreenHandler.Instance.ShowInfos(coat.permission, true));
    }
}
