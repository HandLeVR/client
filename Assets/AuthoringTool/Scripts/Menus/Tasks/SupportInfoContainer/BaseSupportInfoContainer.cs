using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// This is the base class for support info container. Support info container represent a support info in the sub task
/// support info container. These container can be used in the middle panel (Displaying) or in the settings
/// panel (Settings).
/// Provides methods used by multiple specialized sub classes.
/// </summary>
public class BaseSupportInfoContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string type;
    public TextMeshProUGUI supportInfoName;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI nameLabel;

    public SupportInfo supportInfoData;

    protected JObject properties;
    protected SupportInfosSettingsPanel supportInfosSettingsPanel;

    private void Awake()
    {
        supportInfosSettingsPanel = GetComponentInParent<SupportInfosSettingsPanel>();
    }

    /// <summary>
    /// Sets up the container for the middle panel (only for displaying).
    /// </summary>
    public virtual void SetupForDisplaying(SupportInfo supportInfo)
    {
        transform.FindDeepChild("Button Delete").gameObject.SetActive(false);
        transform.GetComponent<CanvasGroup>().interactable = false;
        transform.GetComponent<CanvasGroup>().blocksRaycasts = false;
        nameLabel.gameObject.SetActive(supportInfo.name != "");
        nameInputField.interactable = false;
        nameInputField.text = supportInfo.name;
        properties = GetProperties(supportInfo);
        supportInfoData = supportInfo;
    }

    /// <summary>
    /// Sets up the support info for the settings panel (can be modified).
    /// </summary>
    public virtual void SetupForSettings(SupportInfo supportInfo, bool saveSettings)
    {
        Button deleteButton = transform.FindDeepChild("Button Delete").gameObject.GetComponent<Button>();
        deleteButton.gameObject.SetActive(true);
        deleteButton.interactable = true;
        deleteButton.onClick.AddListener(Delete);
        transform.GetComponentInParent<CanvasGroup>().interactable = true;
        transform.GetComponentInParent<CanvasGroup>().blocksRaycasts = true;
        nameLabel.gameObject.SetActive(true);
        nameInputField.interactable = true;
        nameInputField.characterLimit = 25;
        nameInputField.text = supportInfo.name;
        properties = GetProperties(supportInfo);
        supportInfoData = supportInfo;
        nameInputField.onEndEdit.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());
        // save settings at the and of the frame, because otherwise the changes done in OnDropImpl of the
        // SupportInfoDropArea are not recognized
        if (saveSettings)
            StartCoroutine(WaitFor.DoEndOfFrame(supportInfosSettingsPanel.SupportInfoContainerChanged));
    }

    private void Delete()
    {
        DestroyImmediate(gameObject);
        supportInfosSettingsPanel.SupportInfoContainerChanged();
    }

    public void SaveData()
    {
        if (nameInputField == null)
            return;
        
        supportInfoData.name = nameInputField.text;
        SetJSON();
    }

    /// <summary>
    /// Generates a json from the container. Is implemented by the specialized sub classes.
    /// </summary>
    protected virtual void SetJSON()
    {
        supportInfoData.properties = "{}";
    }

    protected JObject GetProperties(SupportInfo supportInfo)
    {
        return string.IsNullOrEmpty(supportInfo.properties) ? new JObject() : JObject.Parse(supportInfo.properties);
    }

    protected void SetInputField(TMP_InputField inputField, string propertyName, bool setActive = true)
    {
        inputField.interactable = setActive;
        inputField.text = properties.TryGetValue(propertyName, out JToken text)
            ? (string)text
            : "";
    }

    protected void SetToggle(Toggle toggle, string propertyName, bool setActive = true)
    {
        toggle.interactable = setActive;
        toggle.isOn = properties.TryGetValue(propertyName, out JToken text) && (bool)text;
    }

    public virtual bool ValuesMissing()
    {
        return nameInputField.text == "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging) transform.GetComponent<Button>().enabled = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.GetComponent<Button>().enabled = true;
    }

    /// <summary>
    /// Checks whether the value in an input field is in a given range and replaces the value otherwise.
    /// </summary>
    protected void CheckValidInput(TMP_InputField input, float min, float max, float replaceValue)
    {
        if (float.TryParse(input.text, out float f))
            input.text = f >= min && f <= max ? f.ToString() : replaceValue.ToString();
    }

    /// <summary>
    /// The id of the media file was saved in the placeholder of the input field.
    /// </summary>
    protected void SetMediaInputJSON(JObject json, TMP_InputField inputField, string propertyName)
    {
        if (!inputField.text.Equals(string.Empty))
        {
            if (long.TryParse(inputField.placeholder.GetComponent<TextMeshProUGUI>().text, out long tmp))
                json.Add(propertyName, tmp);
        }
    }

    protected void SelectMedia(TMP_InputField inputField, string placeholder)
    {
        SetMediaInput(
            SelectionPopup.Instance.IsValidSelection()
                ? DataController.Instance.media[SelectionPopup.Instance.GetSelectedId()]
                : null, inputField, placeholder);
    }

    protected void SetMediaInputSettings(TMP_InputField inputField, string propertyName, string placeholder)
    {
        SetMediaInput(properties.TryGetValue(propertyName, out JToken mediaJson)
            ? DataController.Instance.media[(long)mediaJson]
            : null, inputField, placeholder);
        inputField.onSelect.AddListener(_ => SelectionPopup.Instance.Init(Media.MediaType.Video,
            () => SelectMedia(inputField, "Klicken zur Auswahl eines Videos")));
    }

    private void SetMediaInput(Media media, TMP_InputField inputField, string placeholder)
    {
        if (media != null)
        {
            // the id of the media file is saved in the placeholder of the input field to be able to find the media file when the json created
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = media.id.ToString();
            inputField.text = media.name;
        }
        else
        {
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = placeholder;
            inputField.text = "";
        }
    }
}