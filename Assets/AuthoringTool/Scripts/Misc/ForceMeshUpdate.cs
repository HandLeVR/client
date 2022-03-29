using TMPro;
using UnityEngine;

/// <summary>
/// Force mesh update on every frame update. This is needed because there is a bug when the anchor preset is set to
/// stretch resulting in weird positions of text elements.
/// </summary>
public class ForceMeshUpdate : MonoBehaviour
{
    private TextMeshProUGUI textField;
    
    private void Start()
    {
        textField = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        textField.ForceMeshUpdate();
    }
}
