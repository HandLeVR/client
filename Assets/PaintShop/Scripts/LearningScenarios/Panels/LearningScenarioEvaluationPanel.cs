using UnityEngine;

/// <summary>
/// Displays various evaluation parameters.
/// </summary>
public class LearningScenarioEvaluationPanel : EvaluationPanel
{
    public GameObject correctDistance;
    public GameObject correctAngle;
    public GameObject colorConsumption;
    public GameObject colorWastage;
    public GameObject colorUsage;
    public GameObject fullyPressed;
    public GameObject averageSpeed;
    public GameObject averageCoatThickness;

    public GameObject toggles;

    public void InitPanel(bool correctDistanceActive, bool correctAngleActive, bool colorConsumptionActive,
        bool colorWastageActive, bool colorUsageActive, bool fullyPressedActive, bool averageSpeedActive, bool averageCoatThicknessActive)
    {
        correctDistance.SetActive(correctDistanceActive);
        correctDistanceRating.gameObject.SetActive(correctDistanceActive);
        
        correctAngle.SetActive(correctAngleActive);
        correctAngleRating.gameObject.SetActive(correctAngleActive);
        
        colorConsumption.SetActive(colorConsumptionActive);
        colorConsumptionRating.gameObject.SetActive(colorUsageActive);
        
        colorWastage.SetActive(colorWastageActive);
        colorWastageRating.gameObject.SetActive(colorUsageActive);

        colorUsage.SetActive(colorUsageActive);
        colorUsageRating.gameObject.SetActive(colorUsageActive);
        
        fullyPressed.SetActive(fullyPressedActive);
        fullyPressedRating.gameObject.SetActive(fullyPressedActive);
        
        averageSpeed.SetActive(averageSpeedActive);
        averageSpeedRating.gameObject.SetActive(averageSpeedActive);
        
        averageCoatThickness.SetActive(averageCoatThicknessActive);
        averageCoatThicknessRating.gameObject.SetActive(averageCoatThicknessActive);
    }
}