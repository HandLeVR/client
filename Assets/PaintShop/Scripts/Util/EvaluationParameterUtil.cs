/// <summary>
/// Provides methods used for the evaluation of the paint application.
/// </summary>
public static class EvaluationParameterUtil
{
    public static EvaluationParameterValues GetCorrectDistanceValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            upperBound = 100
        };
        SetValues(values, ConfigController.EVAL_CORRECT_DISTANCE);
        return values;
    }
    
    public static EvaluationParameterValues GetCorrectAngleValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            upperBound = 100
        };
        SetValues(values, ConfigController.EVAL_CORRECT_ANGLE);
        return values;
    }
    
    public static EvaluationParameterValues GetColorConsumptionValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0
        };
        SetValues(values, ConfigController.EVAL_COLOR_CONSUMPTION);
        values.upperBound = 2 * values.optimalValue;
        return values;
    }

    public static EvaluationParameterValues GetColorWastageValues()
    {
        float colorConsumptionOptValue = GetFloat(ConfigController.EVAL_COLOR_CONSUMPTION_OPT_VALUE);
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            optimalValue = 0
        };
        float baseValue = colorConsumptionOptValue / 100 * GetFloat(ConfigController.EVAL_COLOR_WASTAGE_OPT_VALUE);
        values.threshold1 = baseValue + baseValue / 100 * GetFloat(ConfigController.EVAL_COLOR_WASTAGE_THRESHOLD1);
        values.threshold2 = values.threshold1 + baseValue / 100 * GetFloat(ConfigController.EVAL_COLOR_WASTAGE_THRESHOLD2);
        values.threshold3 = values.threshold2 + baseValue / 100 * GetFloat(ConfigController.EVAL_COLOR_WASTAGE_THRESHOLD3);
        values.upperBound = values.threshold3 + values.threshold3 / 3;
        return values;
    }

    public static EvaluationParameterValues GetColorUsageValues()
    {
        float colorConsumptionOptValue = GetFloat(ConfigController.EVAL_COLOR_CONSUMPTION_OPT_VALUE);
        float optimalValue = colorConsumptionOptValue / 100 * GetFloat(ConfigController.EVAL_COLOR_USAGE_OPT_VALUE);
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            optimalValue = optimalValue,
            upperBound = 2 * optimalValue 
        };
        SetValues(values, ConfigController.EVAL_COLOR_USAGE, values.optimalValue);
        return values;
    }
    
    public static EvaluationParameterValues GetFullyPressedValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            upperBound = 100
        };
        SetValues(values, ConfigController.EVAL_FULLY_PRESSED_TRIGGER);
        return values;
    }
    
    public static EvaluationParameterValues GetAverageSpeedValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0
        };
        SetValues(values, ConfigController.EVAL_AVERAGE_SPEED);
        values.upperBound = 2 * values.optimalValue;
        return values;
    }
    
    public static EvaluationParameterValues GetAverageCoatThicknessValues()
    {
        // if the optimal average coat thickness is not set in the config file we use the coat values
        if (!ConfigController.Instance.TryGetValue(ConfigController.EVAL_AVERAGE_COAT_THICKNESS_OPT_VALUE,
            out float optimalValue))
            optimalValue = PaintController.Instance.chosenCoat.targetMinThicknessWet +
                       (PaintController.Instance.chosenCoat.targetMaxThicknessWet -
                        PaintController.Instance.chosenCoat.targetMinThicknessWet) / 2;
        // if the first threshold of the optimal average coat thickness is not set in the config file we use the coat values
        if (!ConfigController.Instance.TryGetValue(ConfigController.EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD1,
            out float threshold1))
            threshold1 = (optimalValue - PaintController.Instance.chosenCoat.targetMinThicknessWet) / optimalValue * 100;
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            optimalValue = optimalValue,
            upperBound = 2 * optimalValue,
            threshold1 = threshold1
        };
        values.threshold2 = values.threshold1 + optimalValue / 100 * GetFloat(ConfigController.EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD2);
        values.threshold3 = values.threshold2 + optimalValue / 100 * GetFloat(ConfigController.EVAL_AVERAGE_COAT_THICKNESS_THRESHOLD3);
        return values;
    }

    public static EvaluationParameterValues GetCorrectDistanceAbsValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0
        };
        SetValues(values, ConfigController.EVAL_CORRECT_DISTANCE_ABS);
        values.upperBound = 2 * values.optimalValue;
        return values;
    }

    public static EvaluationParameterValues GetCorrectAngleAbsValues()
    {
        EvaluationParameterValues values = new EvaluationParameterValues
        {
            lowerBound = 0,
            upperBound = 90
        };
        SetValues(values, ConfigController.EVAL_CORRECT_ANGLE_ABS);
        return values;
    }

    private static void SetValues(EvaluationParameterValues values, string property)
    {
        values.optimalValue = ConfigController.Instance.GetFloatValue(property + ConfigController.OPT_VALUE);
        SetValues(values, property, values.optimalValue);
    }

    private static void SetValues(EvaluationParameterValues values, string property, float baseValue)
    {
        values.threshold1 = baseValue / 100 * GetFloat(property + ConfigController.THRESHOLD1);
        values.threshold2 = values.threshold1 + baseValue / 100 * GetFloat(property + ConfigController.THRESHOLD2);
        values.threshold3 = values.threshold2 + baseValue / 100 * GetFloat(property + ConfigController.THRESHOLD3);
    }

    private static float GetFloat(string propertyName)
    {
        return ConfigController.Instance.GetFloatValue(propertyName);
    }
}

public class EvaluationParameterValues
{
    public float lowerBound;
    public float upperBound;
    public float optimalValue;
    public float threshold1;
    public float threshold2;
    public float threshold3;
}