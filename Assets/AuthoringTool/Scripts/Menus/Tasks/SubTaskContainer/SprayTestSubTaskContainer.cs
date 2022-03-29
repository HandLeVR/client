using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents a spray test sub task.
/// </summary>
public class SprayTestSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public TMP_InputField probabilityInputField;
    public Toggle splittedToggle;
    public Toggle excessiveMaterialToggle;
    public Toggle oneSidedCurvedToggle;
    public Toggle oneSidedDisplacedToggle;
    public Toggle sShapedToggle;
    public Toggle flutteringSprayToggle;
    public Toggle distanceRayToggle;
    public Toggle distanceMarkerToggle;
    public Toggle angleRayToggle;
    public TMP_InputField paintInputField;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;
    public TMP_InputField finalAudioInputField;

    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        SetCoatInputField(paintInputField, "coatId");
        SetToggle(distanceRayToggle, "distanceRay");
        SetToggle(distanceMarkerToggle, "distanceMarker");
        SetToggle(angleRayToggle, "angleRay");
        distanceRayToggle.transform.parent.parent.gameObject.SetActive(
            distanceRayToggle.isOn || distanceMarkerToggle.isOn || angleRayToggle.isOn);
        SetToggle(splittedToggle, "splittedSpray");
        SetToggle(excessiveMaterialToggle, "excessiveMaterial");
        SetToggle(oneSidedCurvedToggle, "oneSidedCurved");
        SetToggle(oneSidedDisplacedToggle, "oneSidedDisplaced");
        SetToggle(sShapedToggle, "sShaped");
        SetToggle(flutteringSprayToggle, "flutteringSpray");
        splittedToggle.transform.parent.parent.gameObject.SetActive(
            splittedToggle.isOn || excessiveMaterialToggle.isOn || oneSidedCurvedToggle.isOn ||
            oneSidedDisplacedToggle.isOn || sShapedToggle.isOn ||
            flutteringSprayToggle.isOn);
        SetSkippableInputField(skippableInputField);
        SetTextInputField(probabilityInputField, "errorRate");
        SetAudioInputField(finalAudioInputField, "finalAudioId");
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return paintInputField.text == "" || probabilityInputField.text == "";
    }
}
