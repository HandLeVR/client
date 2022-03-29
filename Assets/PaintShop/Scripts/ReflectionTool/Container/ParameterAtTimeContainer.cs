using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Class to handle an evaluation parameter at a specific time (target value OR current value OR current average value
/// OR final average value)
/// </summary>
public class ParameterAtTimeContainer : MonoBehaviour
{
    // GUI
    public TextMeshProUGUI labelTmp;
    public Transform colorBlockContainer;
    public TMP_InputField valueInputField;

    /// <summary>
    /// Setup the evaluation parameters current row (value at time x, x in {target value OR current value OR current average value OR final average value}).
    /// </summary>
    public void Init(EvaluationParameterValues values, bool frozen = false, string label = "")
    {
        if (label != "")
            labelTmp.text = label;
        colorBlockContainer.GetComponent<TimePeriodColorBlock>().SetUpSegments(values, frozen);
        if (frozen)
            FreezeContent();
        else
            UnfreezeContent();
    }

    /// <summary>
    /// Switch between frozen and unfrozen content
    /// </summary>
    public void ToggleFrozenness()
    {
        if (labelTmp.color.Equals(Color.black))
            FreezeContent();
        else
            UnfreezeContent();
    }

    /// <summary>
    /// Unfreeze content, so show the gray gradient colors, text colors are set to gray also.
    /// </summary>
    public void FreezeContent()
    {
        labelTmp.color = Color.gray;
        colorBlockContainer.GetComponent<TimePeriodColorBlock>().FreezeSlider();
        valueInputField.enabled = false;
    }

    public void SetDoNotUnfreezeMe(bool doNotUnfreezeMe)
    {
        colorBlockContainer.GetComponent<TimePeriodColorBlock>().doNotUnfreezeMe = doNotUnfreezeMe;
    }

    /// <summary>
    /// Unfreeze content, so show the colorful gradient colors, text colors are set to black.
    /// </summary>
    public void UnfreezeContent()
    {
        if (!colorBlockContainer.GetComponent<TimePeriodColorBlock>().doNotUnfreezeMe)
        {
            labelTmp.color = Color.black;
            colorBlockContainer.GetComponent<TimePeriodColorBlock>().UnfreezeSlider();
            valueInputField.enabled = true;
        }
    }

    /// <summary>
    /// A value has changed, show it.
    /// </summary>
    /// <param name="value">New value to be displayed.</param>
    public void UpdateValue(float value, string unit = "%")
    {
        colorBlockContainer.GetComponent<TimePeriodColorBlock>().SetSliderValue(value);
        valueInputField.text = value.ToString("F2") + " " + unit;
        StartCoroutine(SetInputFieldAlignment(valueInputField));
    }

    /// <summary>
    /// TMP messes up the text alignment when setting the text to sth. new. -.-
    /// </summary>
    /// <param name="field">Affected input field</param>
    /// <returns></returns>
    private IEnumerator SetInputFieldAlignment(TMP_InputField field)
    {
        yield return new WaitForEndOfFrame();
        field.textComponent.alignment = TextAlignmentOptions.Right;
        // needed to change text alignment
        field.textComponent.color = Color.red;
        field.textComponent.color = Color.black;
    }
}