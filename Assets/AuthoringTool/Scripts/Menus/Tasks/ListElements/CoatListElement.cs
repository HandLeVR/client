using TMPro;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a coat list element.
/// </summary>
public class CoatListElement : BaseListElement
{
    public TMP_InputField coatInputField;

    public override void SetUpForSettings()
    {
        coatInputField.gameObject.SetActive(true);
        inputField.gameObject.SetActive(false);
        btnAdd.transform.parent.gameObject.SetActive(true);
    }

    public override void SetUpForSettings(JObject itemJson)
    {
        SetUpForSettings();
        InputFieldUtil.SetCoatInputFieldSettings(coatInputField, itemJson, "coatId");
    }

    public override void SetUpForDisplaying(JObject itemJson)
    {
        coatInputField.gameObject.SetActive(false);
        btnAdd.gameObject.SetActive(false);
        btnRemove.gameObject.SetActive(false);
        InputFieldUtil.SetCoatInputFieldDisplaying(inputField, itemJson, "coatId");
    }

    public override JObject SetItemJson()
    {
        JObject json = new JObject
        {
            { "coatId", coatInputField.placeholder.GetComponent<TextMeshProUGUI>().text}
        };
        return json;
    }
    
    public override bool ValuesMissing()
    {
        return coatInputField.text == "" && inputField.text == "";
    }
}