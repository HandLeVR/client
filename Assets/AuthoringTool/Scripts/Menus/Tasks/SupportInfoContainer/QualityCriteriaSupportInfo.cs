using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a quality criteria support info.
/// </summary>
public class QualityCriteriaSupportInfo : BaseSupportInfoContainer
{
    public Toggle sequentialToggle;

    public override void SetupForDisplaying(SupportInfo supportInfo)
    {
        base.SetupForDisplaying(supportInfo);
        SetToggle(sequentialToggle, "sequential", false);
        sequentialToggle.transform.parent.gameObject.SetActive(sequentialToggle.isOn);
    }

    public override void SetupForSettings(SupportInfo supportInfo, bool saveSettings)
    {
        base.SetupForSettings(supportInfo,saveSettings);
        SetToggle(sequentialToggle, "sequential");
        sequentialToggle.onValueChanged.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());
    }

    protected override void SetJSON()
    {
        JObject json = new JObject {{"sequential", sequentialToggle.isOn}};
        supportInfoData.properties = json.ToString();
    }
}
