using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This component is needed to avoid weird Text alignments after the text in an InputField is set.
/// </summary>
[RequireComponent(typeof(TMP_InputField))]
public class RebuildTextMesh : MonoBehaviour
{
    private TMP_InputField textMesh;

    void Awake()
    {
        textMesh = GetComponent<TMP_InputField>();
        textMesh.onValueChanged.AddListener(_ =>
            textMesh.GetComponentInChildren<TextMeshProUGUI>().Rebuild(CanvasUpdate.PreRender));
    }
}
