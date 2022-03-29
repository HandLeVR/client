using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Changes the color of the evaluation rating sprites based on the thresholds
/// and the current value handed over by the evaluation controller
/// </summary>
public class EvaluationRating : MonoBehaviour
{
    public Color disabledColor = Color.black;
    public Color enabledColor = Color.white;

    private Image[] sprites;

    public void UpdateSprites(float currentValue, EvaluationParameterValues values)
    {
        if (sprites == null)
            sprites = GetComponentsInChildren<Image>();
        sprites[2].color = InRange(currentValue, values.optimalValue, values.threshold1) ? enabledColor : disabledColor;
        sprites[1].color = InRange(currentValue, values.optimalValue, values.threshold2) ? enabledColor : disabledColor;
        sprites[0].color = InRange(currentValue, values.optimalValue, values.threshold3) ? enabledColor : disabledColor;
    }

    /// <summary>
    /// Checks whether the given value is in a given range. The range is determined by the optimal value +- the
    /// percentage value applied to the base value. Normally, the base value equals the optimal value.
    /// </summary>
    bool InRange(float currentValue, float optimalValue, float threshold)
    {
        return currentValue >= optimalValue - threshold && currentValue <= optimalValue + threshold;
    }
}