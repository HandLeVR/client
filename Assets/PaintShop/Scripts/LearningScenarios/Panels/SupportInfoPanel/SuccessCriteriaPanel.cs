using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays success criteria on the panel and usages some coat properties for it.
/// </summary>
public class SuccessCriteriaPanel : MonoBehaviour
{
    private const string CoatThicknessString = "{0} µm";
    private const string DistanceString = "{0} - {1} cm";
    private const string CoatUsageString = "ca. {0} ml";

    public TextMeshProUGUI coatThicknessText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI coatUsageText;
    
    [HideInInspector] public string targetThickness;
    [HideInInspector] public string coatUsage;
    
    private void OnEnable()
    {
        Coat coat = PaintController.Instance.chosenCoat;
        
        distanceText.text = String.Format(DistanceString, $"{coat.minSprayDistance}", $"{coat.maxSprayDistance}");
        
        if (String.IsNullOrEmpty(targetThickness))
            coatThicknessText.text = String.Format(CoatThicknessString, $"{coat.targetMinThicknessWet + (coat.targetMaxThicknessWet - coat.targetMinThicknessWet) / 2}");
        else
            coatThicknessText.text = String.Format(CoatThicknessString, $"{targetThickness}");
        
        if (String.IsNullOrEmpty(coatUsage))
            coatUsageText.text = String.Format(CoatUsageString, "500");
        else
            coatUsageText.text = String.Format(CoatUsageString, $"{coatUsage}");
    }
}
