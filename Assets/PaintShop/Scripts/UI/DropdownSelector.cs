using TMPro;
using UnityEngine;

/// <summary>
/// Is needed because otherwise the dropdown has a grey overlay if the user switches to a panel where the dropdown is used.
/// </summary>
public class DropdownSelector : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    
    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        dropdown.Select();
    }
}
