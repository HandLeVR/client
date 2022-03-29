using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class BaseElementListSubTaskContainer : BaseSubTaskContainer
{
    public TMP_InputField monitorTextInputField;
    public GameObject sortableContainer;
    public Transform sortablesContent;
    public GameObject sortableObjectContainerPrefab;
    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public TMP_InputField skippableInputField;
    public Toggle automaticAudioToggle;
    
    protected override void SetUpByProperties()
    {
        SetTextInputField(monitorTextInputField, "textMonitor");
        sortablesContent.transform.DestroyImmediateAllChildren();
        List<JObject> objects = GetSortableObjects("items");
        foreach (JObject obj in objects)
        {
            GameObject newContainer = Instantiate(sortableObjectContainerPrefab, sortablesContent);
            newContainer.GetComponent<BaseListElement>().SetUpForDisplaying(obj);
        }
        sortableContainer.transform.parent.gameObject.SetActive(objects.Count > 0);
        SetSkippableInputField(skippableInputField);
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
    }

    public override bool ValuesMissing()
    {
        return sortablesContent.transform.childCount == 0;
    }

    private List<JObject> GetSortableObjects(string propertyName)
    {
        List<JObject> objects = new List<JObject>();
        if (properties.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken text in jToken)
                objects.Add((JObject)text);
        return objects;
    }
}
