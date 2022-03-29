using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows to (de)activate spraying assistance during the painting sub task.
/// </summary>
public class PaintWorkpiecePanel : MonoBehaviour
{
    public TextMeshProUGUI textField;
    public GameObject supportOptions;
    public Toggle distanceRay;
    public Toggle distanceMarker;
    public Toggle angleRay;

    public void InitPanel(string text)
    {
        textField.text = text;
    }

    public void ActivateDistanceRaySprayingAssistance(bool active)
    {
        ApplicationController.Instance.ActivateDistanceRaySprayingAssistance(active);
    }

    public void ActivateDistanceMarkerSprayingAssistance(bool active)
    {
        ApplicationController.Instance.ActivateDistanceMarkerSprayingAssistance(active);
    }

    public void ActivateAngleRaySprayingAssistance(bool active)
    {
        ApplicationController.Instance.ActivateAngleRaySprayingAssistance(active);
    }
}
