using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a texts support info.
/// </summary>
public class TextsSupportInfo : BaseSupportInfoContainer
{
    public GameObject textElements;
    public GameObject textElementPrefab;

    private const string labelTemplate = "Text {0}:";

    public override void SetupForDisplaying(SupportInfo supportInfo)
    {
        base.SetupForDisplaying(supportInfo);
        textElements.transform.DestroyImmediateAllChildren();
        List<string> texts = GetTexts(properties);
        foreach (string text in texts)
        {
            GameObject newElement = Instantiate(textElementPrefab, textElements.transform);
            TMP_InputField inputField = newElement.GetComponentInChildren<TMP_InputField>();
            inputField.text = text;
            inputField.interactable = false;
            newElement.GetComponentInChildren<TextMeshProUGUI>().text =
                string.Format(labelTemplate, newElement.transform.GetSiblingIndex() + 1);
            newElement.transform.Find("Buttons").gameObject.SetActive(false);
        }

        textElements.SetActive(texts.Count > 0);
    }

    public override void SetupForSettings(SupportInfo setup, bool saveSettings)
    {
        base.SetupForSettings(setup, saveSettings);
        textElements.transform.DestroyImmediateAllChildren();

        List<string> texts = GetTexts(properties);
        foreach (string text in texts)
        {
            GameObject newElement = Instantiate(textElementPrefab, textElements.transform);
            InitTextElement(newElement);
            TMP_InputField inputField = newElement.GetComponentInChildren<TMP_InputField>();
            inputField.text = text;
            inputField.interactable = true;
        }

        if (texts.Count == 0)
            InitTextElement(Instantiate(textElementPrefab, textElements.transform));
        UpdateAllTextElements();
    }

    protected override void SetJSON()
    {
        List<string> texts = new List<string>();
        foreach (Transform element in textElements.transform)
        {
            string text = element.GetComponentInChildren<TMP_InputField>().text;
            if (text != "")
                texts.Add(text);
        }

        JObject json = new JObject();
        json.Add("texts", new JArray { texts });
        supportInfoData.properties = json.ToString();
    }

    private List<string> GetTexts(JObject jObject)
    {
        List<string> texts = new List<string>();
        if (jObject.TryGetValue("texts", out JToken jToken))
            foreach (JToken text in jToken)
                texts.Add((string)text);
        return texts;
    }

    private void InitTextElement(GameObject textElement)
    {
        textElement.transform.FindDeepChild("Button Add").GetComponent<Button>().onClick.AddListener(AddTextElement);
        textElement.transform.FindDeepChild("Button Remove").GetComponent<Button>().onClick
            .AddListener(() => RemoveTextElement(textElement));
        textElement.transform.FindDeepChild("Text Content").GetComponent<TMP_InputField>().onEndEdit
            .AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());
    }

    private void UpdateAllTextElements()
    {
        foreach (Transform element in textElements.transform)
        {
            bool isLastElement = element.GetSiblingIndex() == textElements.transform.childCount - 1;
            element.FindDeepChild("Button Add").gameObject.SetActive(isLastElement);
            element.FindDeepChild("Button Remove").gameObject.SetActive(textElements.transform.childCount > 1);
            element.Find("Label").GetComponent<TextMeshProUGUI>().text =
                string.Format(labelTemplate, element.transform.GetSiblingIndex() + 1);
        }
    }

    private void AddTextElement()
    {
        GameObject newElement = Instantiate(textElementPrefab, textElements.transform);
        InitTextElement(newElement);
        UpdateAllTextElements();
        supportInfosSettingsPanel.SupportInfoContainerChanged();
    }

    private void RemoveTextElement(GameObject element)
    {
        DestroyImmediate(element);
        UpdateAllTextElements();
        supportInfosSettingsPanel.SupportInfoContainerChanged();
    }

    public override bool ValuesMissing()
    {
        return base.ValuesMissing() || GetTexts(GetProperties(supportInfoData)).Count == 0;
    }
}