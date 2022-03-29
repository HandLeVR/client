using UnityEngine;

/// <summary>
/// Allows to overwrite coat properties on runtime to test different appearances.
/// </summary>
public class OverwriteCoatProperties : MonoBehaviour
{
    public bool loadCoatAgain;
    public bool loadCoatProperties;
    public bool overwriteCoatProperties;
    public float viscosity;
    public float minSprayDistance;
    public float maxSprayDistance;
    public float glossWet;
    public float glossDry;
    public float targetMinThicknessWet;
    public float targetMaxThicknessWet;
    public float targetMinThicknessDry;
    public float targetMaxThicknessDry;
    public float fullOpacityMinThicknessWet;
    public float fullOpacityMinThicknessDry;
    public float fullGlossMinThicknessWet;
    public float fullGlossMinThicknessDry;
    public float runsStartThicknessWet;
    public float roughness;
 
    public void Update()
    {
        if (loadCoatAgain)
        {
            OverwriteCoat();
            PaintController.Instance.LoadCoat(PaintController.Instance.chosenCoat, true);
        }
        
        if (loadCoatProperties)
        {
            LoadCoatProperties(PaintController.Instance.chosenCoat);
        }

        loadCoatAgain = false;
        loadCoatProperties = false;
    }

    private void LoadCoatProperties(Coat coat)
    {
        this.viscosity = coat.viscosity;
        this.minSprayDistance = coat.minSprayDistance;
        this.maxSprayDistance = coat.maxSprayDistance;
        this.glossWet = coat.glossWet;
        this.glossDry = coat.glossDry;
        this.targetMinThicknessWet = coat.targetMinThicknessWet;
        this.targetMaxThicknessWet = coat.targetMaxThicknessWet;
        this.targetMinThicknessDry = coat.targetMinThicknessDry;
        this.targetMaxThicknessDry = coat.targetMaxThicknessDry;
        this.fullOpacityMinThicknessWet = coat.fullOpacityMinThicknessWet;
        this.fullOpacityMinThicknessDry = coat.fullOpacityMinThicknessDry;
        this.fullGlossMinThicknessWet = coat.fullGlossMinThicknessWet;
        this.fullGlossMinThicknessDry = coat.fullGlossMinThicknessDry;
        this.runsStartThicknessWet = coat.runsStartThicknessWet;
        this.roughness = coat.roughness;
    }  
    
    private void OverwriteCoat()
    {
        PaintController.Instance.chosenCoat.viscosity = viscosity;
        PaintController.Instance.chosenCoat.minSprayDistance = minSprayDistance;
        PaintController.Instance.chosenCoat.maxSprayDistance = maxSprayDistance;
        PaintController.Instance.chosenCoat.glossWet = glossWet;
        PaintController.Instance.chosenCoat.glossDry = glossDry;
        PaintController.Instance.chosenCoat.targetMinThicknessWet = targetMinThicknessWet;
        PaintController.Instance.chosenCoat.targetMaxThicknessWet = targetMaxThicknessWet;
        PaintController.Instance.chosenCoat.targetMinThicknessDry = targetMinThicknessDry;
        PaintController.Instance.chosenCoat.targetMaxThicknessDry = targetMaxThicknessDry;
        PaintController.Instance.chosenCoat.fullOpacityMinThicknessWet = fullOpacityMinThicknessWet;
        PaintController.Instance.chosenCoat.fullOpacityMinThicknessDry = fullOpacityMinThicknessDry;
        PaintController.Instance.chosenCoat.fullGlossMinThicknessWet = fullGlossMinThicknessWet;
        PaintController.Instance.chosenCoat.fullGlossMinThicknessDry = fullGlossMinThicknessDry;
        PaintController.Instance.chosenCoat.runsStartThicknessWet = runsStartThicknessWet;
        PaintController.Instance.chosenCoat.roughness = roughness;
    }
}
