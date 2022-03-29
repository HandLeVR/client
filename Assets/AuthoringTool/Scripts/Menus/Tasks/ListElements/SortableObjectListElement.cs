using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a sortable object list element.
/// </summary>
public class SortableObjectListElement : BaseListElement
{
    public Image previewPic;
    public Sprite missingPreviewPic;
    public TMP_InputField labelingInputField;
    public Toggle insideToggle;

    private string currentObject;

    void Start()
    {
        dropdown.onValueChanged.AddListener(delegate { SetImage(); });
    }

    public override void SetUpForSettings(JObject itemJson)
    {
        base.SetUpForSettings(itemJson);
        string objectName = itemJson.GetValue("model").ToString();
        SetImage(objectName);
        labelingInputField.text = itemJson.GetValue("text").ToString();
        insideToggle.isOn = (bool) itemJson.GetValue("correct");
        dropdown.value =
            dropdown.options.FindIndex(x => x.text.Equals(TranslationController.Instance.Translate(objectName)));
    }

    public override void SetUpForDisplaying(JObject itemJson)
    {
        base.SetUpForDisplaying(itemJson);
        btnAdd.transform.parent.gameObject.SetActive(false);
        inputField.text = TranslationController.Instance.Translate((string)itemJson["model"]);
        SetImage((string)itemJson["model"]);
        labelingInputField.interactable = false;
        labelingInputField.text = (string)itemJson["text"];
        insideToggle.interactable = false;
        insideToggle.isOn = (bool)itemJson["correct"];
    }

    public override JObject SetItemJson()
    {
        JObject json = new JObject
            {
                { "model", DataController.Instance.sortableObjects[dropdown.value]},
                { "text",  labelingInputField.text},
                { "correct", insideToggle.isOn}
            };
        return json;
    }

    private void SetImage()
    {
       SetImage(DataController.Instance.sortableObjects[dropdown.value]);
    }

    private void SetImage(string objectName)
    {
        currentObject = objectName;
        previewPic.sprite = Resources.Load<Sprite>("Images/SortableObjects/" + objectName);
        previewPic.color = Color.white;
        if (previewPic.sprite == null) // no object selected or preview pic doesn't exists
        {
            previewPic.sprite = missingPreviewPic;
            previewPic.color = new Color(215/255f,11/255f,82/255f);
        }  
    }

    public override bool ValuesMissing()
    {
        return base.ValuesMissing() || labelingInputField.text.Equals("");
    }
}
