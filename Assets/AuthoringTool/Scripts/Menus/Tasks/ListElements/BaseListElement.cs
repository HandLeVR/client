using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Base class for elements which can be used in a list
/// </summary>
public abstract class BaseListElement : MonoBehaviour
{
    public TextMeshProUGUI label;
    public TMP_Dropdown dropdown;
    public TMP_InputField inputField;
    public Button btnAdd;
    public Button btnRemove;

    /// <summary>
    /// Sets up the element in the settings column (ride side).
    /// </summary>
    public virtual void SetUpForSettings()
    {
        dropdown.gameObject.SetActive(true);
        inputField.gameObject.SetActive(false);
        btnAdd.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets up the element in the settings column (ride side) but uses existing properties.
    /// </summary>
    public virtual void SetUpForSettings(JObject sortableObject)
    {
        dropdown.gameObject.SetActive(true);
        inputField.gameObject.SetActive(false);
        btnAdd.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets up the element in the column for the used sub tasks (middle).
    /// </summary>
    public virtual void SetUpForDisplaying(JObject sortableObject)
    {
        dropdown.gameObject.SetActive(false);
    }

    /// <summary>
    /// Determines whether a value is missing.
    /// </summary>
    public virtual bool ValuesMissing()
    {
        return dropdown.value == 0 && inputField.text == "";
    }

    /// <summary>
    /// Creates a json from the item represented by this element.
    /// </summary>
    public abstract JObject SetItemJson();
}