using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for list elements that can be assigned (e.g. users to a user group).
/// </summary>
public class BasicAssignableElement : MonoBehaviour
{
    public TextMeshProUGUI indexLabel;
    public TMP_InputField nameInputField;
    public TMP_InputField descriptionInputField;
    public Button deleteButton;
    public Button addButton;

    public void SetIndex(int index)
    {
        indexLabel.text = index.ToString();
    }
}