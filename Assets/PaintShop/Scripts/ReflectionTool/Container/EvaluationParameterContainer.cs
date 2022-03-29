using TMPro;
using translator;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Class to handle all values for a specific evaluation parameter at different times (target value, current value,
/// current average value and final average value). Enable, disable, hide or show specific time container, create the
/// gradient based on min, max and perfect value related to the specific evaluation parameter.
/// </summary>
public class EvaluationParameterContainer : MonoBehaviour
{
    /// GUI
    public TextMeshProUGUI title;

    public ParameterAtTimeContainer targetValueContainer;
    public ParameterAtTimeContainer currentValueContainer;
    public ParameterAtTimeContainer currentAverageValueContainer;
    public ParameterAtTimeContainer finalAverageValueContainer;

    public void InitContainerCorrectDistance()
    {
        title.text = TranslationController.Instance.Translate("reflection-tool-correct-distance");
        EvaluationParameterValues values = EvaluationParameterUtil.GetCorrectDistanceValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(100);
        currentAverageValueContainer.Init(values);
        currentAverageValueContainer.UpdateValue(0);
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0);
        currentValueContainer.Init(EvaluationParameterUtil.GetCorrectDistanceAbsValues());
        currentValueContainer.UpdateValue(0, "cm");
    }

    public void InitContainerCorrectAngle()
    {
        title.text = TranslationController.Instance.Translate("reflection-tool-correct-angle");
        EvaluationParameterValues values = EvaluationParameterUtil.GetCorrectDistanceValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(100);
        currentAverageValueContainer.Init(values);
        currentAverageValueContainer.UpdateValue(0);
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0);
        currentValueContainer.Init(EvaluationParameterUtil.GetCorrectAngleAbsValues());
        currentValueContainer.UpdateValue(0, "°");
    }


    public void InitContainerColorConsumption()
    {
        EvaluationParameterValues values = EvaluationParameterUtil.GetColorConsumptionValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(values.optimalValue, "ml");
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0, "ml");
        currentValueContainer.Init(values);
        currentValueContainer.UpdateValue(0, "ml");
    }

    public void InitContainerColorWasted()
    {
        EvaluationParameterValues values = EvaluationParameterUtil.GetColorWastageValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(values.optimalValue, "ml");
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0, "ml");
        currentValueContainer.Init(values);
        currentValueContainer.UpdateValue(0, "ml");
    }

    public void InitContainerColorOnWorkpiece()
    {
        EvaluationParameterValues values = EvaluationParameterUtil.GetColorUsageValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(values.optimalValue, "ml");
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0, "ml");
        currentValueContainer.Init(values);
        currentValueContainer.UpdateValue(0, "ml");
    }

    public void InitContainerTriggerPressed()
    {
        title.text = TranslationController.Instance.Translate("reflection-tool-pressed-trigger");
        EvaluationParameterValues values = EvaluationParameterUtil.GetFullyPressedValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(100);
        currentAverageValueContainer.Init(values);
        currentAverageValueContainer.UpdateValue(0);
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0);
        currentValueContainer.Init(values);
        currentValueContainer.UpdateValue(0);
    }

    public void InitContainerSpeed()
    {
        title.text = TranslationController.Instance.Translate("reflection-tool-speed");
        EvaluationParameterValues values = EvaluationParameterUtil.GetAverageSpeedValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(values.optimalValue, "m/s");
        currentAverageValueContainer.Init(values);
        currentAverageValueContainer.UpdateValue(0, "m/s");
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0, "m/s");
        currentValueContainer.Init(values);
        currentValueContainer.UpdateValue(0, "m/s");
    }

    public void InitContainerThickness()
    {
        title.text = TranslationController.Instance.Translate("reflection-tool-coat-thickness");
        EvaluationParameterValues values = EvaluationParameterUtil.GetAverageCoatThicknessValues();
        targetValueContainer.Init(values);
        targetValueContainer.UpdateValue(values.optimalValue, "μm");
        currentAverageValueContainer.Init(values);
        currentAverageValueContainer.UpdateValue(0, "μm");
        finalAverageValueContainer.Init(values);
        finalAverageValueContainer.UpdateValue(0, "μm");
        currentValueContainer.Init(values, true,
            TranslationController.Instance.Translate("reflection-tool-value-at-current-position"));
        currentValueContainer.UpdateValue(0, "μm");
    }

    /// <summary>
    /// Switch between frozen and unfrozen content, doesn't affect the parent's layout.
    /// </summary>
    public void ToggleFrozenCurrentValue()
    {
        currentValueContainer.ToggleFrozenness();
    }

    /// <summary>
    /// Switch between frozen and unfrozen content, doesn't affect the parent's layout.
    /// </summary>
    public void ToggleFrozenCurrentAverageValue()
    {
        currentAverageValueContainer.ToggleFrozenness();
    }

    /// <summary>
    /// Switch between frozen and unfrozen content, doesn't affect the parent's layout.
    /// </summary>
    public void ToggleFrozenFinalValue()
    {
        finalAverageValueContainer.ToggleFrozenness();
    }

    /// <summary>
    /// Update the value for the current container. Affects the displayed value and the sliders position.
    /// </summary>
    public void UpdateCurrentContainerContent(float value, string unit = "%")
    {
        currentValueContainer.UpdateValue(value, unit);
    }

    /// <summary>
    /// Update the value for the current average container. Affects the displayed value and the sliders position.
    /// </summary>
    public void UpdateCurrentAverageContainerContent(float value, string unit = "%")
    {
        currentAverageValueContainer.UpdateValue(value, unit);
    }

    /// <summary>
    /// Update the value for the final average container. Affects the displayed value and the sliders position.
    /// </summary>
    public void UpdateFinalAverageContainerContent(float value, string unit = "%")
    {
        finalAverageValueContainer.UpdateValue(value, unit);
    }
}