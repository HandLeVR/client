using TMPro;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a video support info.
/// </summary>
public class VideoSupportInfoContainer : BaseSupportInfoContainer
{
    public TMP_InputField videoNameInput;
    public TMP_InputField videoInput;

    public override void SetupForDisplaying(SupportInfo supportInfo)
    {
        base.SetupForDisplaying(supportInfo);

        videoNameInput.text = properties.TryGetValue("videoId", out JToken jToken)
            ? DataController.Instance.availableVideos[(long)jToken].name
            : "";
        videoNameInput.transform.parent.gameObject.SetActive(videoNameInput.text != "");
        videoInput.interactable = false;
        videoInput.transform.parent.gameObject.SetActive(false);
    }

    public override void SetupForSettings(SupportInfo supportInfo, bool saveSettings)
    {
        base.SetupForSettings(supportInfo, saveSettings);
        videoNameInput.interactable = false;
        videoNameInput.transform.parent.gameObject.SetActive(false);
        videoInput.interactable = true;
        SetMediaInputSettings(videoInput, "videoId", "Klicken zur Auswahl eines Videos");
        videoInput.transform.parent.gameObject.SetActive(true);
        videoInput.onValueChanged.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());
    }

    public override bool ValuesMissing()
    {
        return base.ValuesMissing() || GetProperties(supportInfoData)["videoId"] == null;
    }

    protected override void SetJSON()
    {
        JObject json = new JObject();
        SetMediaInputJSON(json, videoInput, "videoId");
        supportInfoData.properties = json.ToString();
    }
}