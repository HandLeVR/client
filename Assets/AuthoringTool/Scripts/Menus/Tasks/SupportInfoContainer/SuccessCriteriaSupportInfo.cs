using TMPro;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a success criteria support info.
/// </summary>
public class SuccessCriteriaSupportInfo : BaseSupportInfoContainer
{
    public TMP_InputField colorConsumption;
    public TMP_InputField layerThickness;

    public override void SetupForDisplaying(SupportInfo supportInfo)
    {
        base.SetupForDisplaying(supportInfo);
        SetInputField(colorConsumption, "coatUsage", false);
        colorConsumption.transform.parent.gameObject.SetActive(colorConsumption.text != "");
        SetInputField(layerThickness, "targetThickness", false);
        layerThickness.transform.parent.gameObject.SetActive(layerThickness.text != "");
    }

    public override void SetupForSettings(SupportInfo supportInfo, bool saveSettings)
    {
        base.SetupForSettings(supportInfo, saveSettings);
        SetInputField(colorConsumption, "coatUsage");
        SetInputField(layerThickness, "targetThickness");
        colorConsumption.onEndEdit.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());
        layerThickness.onEndEdit.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());
    }

    protected override void SetJSON()
    {
        JObject json = new JObject();
        CheckValidInput(colorConsumption, 0f, 9999f, 0f);
        json.Add("coatUsage", colorConsumption.text);
        CheckValidInput(layerThickness, 0f, 9999f, 0f);
        json.Add("targetThickness", layerThickness.text);
        supportInfoData.properties = json.ToString();
    }
}