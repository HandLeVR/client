using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents an element in the selection popup. 
/// </summary>
public class SelectionElement : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Toggle checkbox;
    public Button button;
    
    [HideInInspector] public long id;
    
    private string title;
    private bool alwaysVisible;

    public void Init(string title, long id, bool alwaysVisible)
    {
        this.title = title;
        this.id = id;
        this.alwaysVisible = alwaysVisible;
        label.text = title;
    }

    public void Filter(string searchString)
    {
        gameObject.SetActive(alwaysVisible || title.ToLower().Contains(searchString));
    }
}