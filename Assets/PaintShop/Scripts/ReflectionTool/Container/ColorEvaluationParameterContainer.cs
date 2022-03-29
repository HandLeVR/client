using TMPro;
using UnityEngine;

/// <summary>
/// A container for multiple EvaluationParameterContainer.
/// </summary>
public class ColorEvaluationParameterContainer : MonoBehaviour
{
    public EvaluationParameterContainer colorConsumptionContainer;
    public EvaluationParameterContainer colorWastedContainer;
    public EvaluationParameterContainer colorOnWorkpieceContainer;
    public TextMeshProUGUI labelFinalValue;
    public TextMeshProUGUI labelCurrentValue;

    public void InitColorContainer()
    {
        colorConsumptionContainer.InitContainerColorConsumption();
        colorWastedContainer.InitContainerColorWasted();
        colorOnWorkpieceContainer.InitContainerColorOnWorkpiece();
    }
    
    public void ToggleFrozenCurrentValue()
    {
        labelCurrentValue.color = labelCurrentValue.color.Equals(Color.black) ? Color.gray : Color.black;
        colorConsumptionContainer.ToggleFrozenCurrentValue();
        colorWastedContainer.ToggleFrozenCurrentValue();
        colorOnWorkpieceContainer.ToggleFrozenCurrentValue();
    }
    
    public void ToggleFrozenFinalValue()
    {
        labelFinalValue.color = labelFinalValue.color.Equals(Color.black) ? Color.gray : Color.black;
        colorConsumptionContainer.ToggleFrozenFinalValue();
        colorWastedContainer.ToggleFrozenFinalValue();
        colorOnWorkpieceContainer.ToggleFrozenFinalValue();
    }

    public void UpdateCurrentContainerContent(float colorConsumption, float colorWasted, float colorOnWorkpiece)
    {
        colorConsumptionContainer.UpdateCurrentContainerContent(colorConsumption, "ml");
        colorWastedContainer.UpdateCurrentContainerContent(colorWasted, "ml");
        colorOnWorkpieceContainer.UpdateCurrentContainerContent(colorOnWorkpiece, "ml");
    }

    public void UpdateFinalAverageContainerContent(float colorConsumption, float colorWasted, float colorOnWorkpiece)
    {
        colorConsumptionContainer.UpdateFinalAverageContainerContent(colorConsumption, "ml");
        colorWastedContainer.UpdateFinalAverageContainerContent(colorWasted, "ml");
        colorOnWorkpieceContainer.UpdateFinalAverageContainerContent(colorOnWorkpiece, "ml");
    }
}
