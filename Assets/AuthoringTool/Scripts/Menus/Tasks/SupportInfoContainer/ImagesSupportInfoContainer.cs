using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents an images support info.
/// </summary>
public class ImagesSupportInfoContainer : BaseSupportInfoContainer
{
    public GameObject imageSelectionElements;
    public GameObject imageDisplayElements;
    public GameObject imageSelectionPrefab;
    public GameObject imageDisplayPrefab;

    private const string labelImageTemplate = "Bild {0}:";
    private const string labelAudioTemplate = "Sprachausgabe {0}:";

    public override void SetupForDisplaying(SupportInfo supportInfo)
    {
        base.SetupForDisplaying(supportInfo);
        imageSelectionElements.SetActive(false);
        imageDisplayElements.transform.DestroyImmediateAllChildren();
        List<Tuple<Media, Media>> images = GetImages(properties);
        foreach (Tuple<Media, Media> image in images)
        {
            GameObject element = Instantiate(imageDisplayPrefab, imageDisplayElements.transform);
            TMP_InputField selectedImage = element.transform.FindDeepChild("Selected Image")
                .GetComponentInChildren<TMP_InputField>();
            TextMeshProUGUI labelImage = element.transform.FindDeepChild("Label Image").GetComponent<TextMeshProUGUI>();
            TMP_InputField selectedAudio = element.transform.FindDeepChild("Selected Audio")
                .GetComponentInChildren<TMP_InputField>();
            TextMeshProUGUI labelAudio = element.transform.FindDeepChild("Label Audio").GetComponent<TextMeshProUGUI>();
            selectedImage.text = image.Item1.name;
            labelImage.text = string.Format(labelImageTemplate, element.transform.GetSiblingIndex() + 1);
            if (image.Item2 != null)
            {
                selectedAudio.text = image.Item2.name;
                labelAudio.text = string.Format(labelAudioTemplate, element.transform.GetSiblingIndex() + 1);
                selectedAudio.gameObject.SetActive(true);
                labelAudio.gameObject.SetActive(true);
            }
            else
            {
                selectedAudio.gameObject.SetActive(false);
                labelAudio.gameObject.SetActive(false);
            }
        }

        imageDisplayElements.SetActive(images.Count > 0);
    }

    public override void SetupForSettings(SupportInfo supportInfo, bool saveSettings)
    {
        base.SetupForSettings(supportInfo, saveSettings);
        imageDisplayElements.SetActive(false);
        imageSelectionElements.transform.DestroyImmediateAllChildren();

        if (!properties.HasValues)
        {
            InitSelectionElement(Instantiate(imageSelectionPrefab, imageSelectionElements.transform));
            UpdateAllSelectionElements();
            return;
        }

        List<Tuple<Media, Media>> images = GetImages(properties);
        // when dragging, properties has values, but they may are empty
        if (images.Count > 0)
        {
            foreach (Tuple<Media, Media> image in images)
            {
                GameObject newElement = Instantiate(imageSelectionPrefab, imageSelectionElements.transform);
                InitSelectionElement(newElement);
                TMP_InputField input;
                if (image.Item1 != null)
                {
                    input = newElement.transform.FindDeepChild("InputField Image").GetComponent<TMP_InputField>();
                    input.text = image.Item1.name;
                    input.placeholder.GetComponent<TextMeshProUGUI>().text = image.Item1.id.ToString();
                }

                if (image.Item2 != null)
                {
                    input = newElement.transform.FindDeepChild("InputField Audio").GetComponent<TMP_InputField>();
                    input.text = image.Item2.name;
                    input.placeholder.GetComponent<TextMeshProUGUI>().text = image.Item2.id.ToString();
                }
            }
        }
        // List was empty due to dragging created an empty json
        else 
            InitSelectionElement(Instantiate(imageSelectionPrefab, imageSelectionElements.transform));

        UpdateAllSelectionElements();

    }

    public override bool ValuesMissing()
    {
        return base.ValuesMissing() || GetImages(GetProperties(supportInfoData)).Count == 0;
    }

    protected override void SetJSON()
    {
        JArray images = new JArray();
        foreach (Transform element in imageSelectionElements.transform)
        {
            TMP_InputField imageInputField = element.FindDeepChild("InputField Image").GetComponent<TMP_InputField>();
            if (!imageInputField.text.Equals(string.Empty))
            {
                JObject image = new JObject();
                SetMediaInputJSON(image, imageInputField, "imageId");
                SetMediaInputJSON(image, element.FindDeepChild("InputField Audio").GetComponent<TMP_InputField>(),
                    "audioId");
                images.Add(image);
            }
        }

        JObject json = new JObject { { "images", images } };
        supportInfoData.properties = json.ToString();
    }

    /// <summary>
    /// Gets the media objects from the ids in the json.
    /// </summary>
    private List<Tuple<Media, Media>> GetImages(JObject jObject)
    {
        List<Tuple<Media, Media>> images = new List<Tuple<Media, Media>>();
        if (jObject.TryGetValue("images", out JToken jToken))
            foreach (JToken imageId in jToken)
            {
                Media image = null;
                if (imageId["imageId"] != null)
                    image = DataController.Instance.availableImages[(long)imageId["imageId"]];  
                else 
                    continue;
                Media audioElement = null;
                if (imageId["audioId"] != null)
                    audioElement = DataController.Instance.availableAudios[(long)imageId["audioId"]];
                images.Add(new Tuple<Media, Media>(image, audioElement));
            }
        return images;
    }

    private void InitSelectionElement(GameObject selectionElement)
    {
        TMP_InputField inputImage =
            selectionElement.transform.FindDeepChild("InputField Image").GetComponent<TMP_InputField>();
        inputImage.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl eines Bildes";
        inputImage.onSelect.AddListener(_ => SelectionPopup.Instance.Init(Media.MediaType.Image,
            () => SelectMedia(inputImage, "Klicken zur Auswahl eines Bildes")));
        inputImage.onValueChanged.AddListener(_ => UpdateAllSelectionElements());
        inputImage.onValueChanged.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());

        TMP_InputField inputAudio =
            selectionElement.transform.FindDeepChild("InputField Audio").GetComponent<TMP_InputField>();
        inputAudio.placeholder.GetComponent<TextMeshProUGUI>().text = "Klicken zur Auswahl einer Audiodatei";
        inputAudio.onSelect.AddListener(_ => SelectionPopup.Instance.Init(Media.MediaType.Audio,
            () => SelectMedia(inputAudio, "Klicken zur Auswahl einer Audiodatei")));
        inputAudio.onValueChanged.AddListener(_ => UpdateAllSelectionElements());
        inputAudio.onValueChanged.AddListener(_ => supportInfosSettingsPanel.SupportInfoContainerChanged());

        selectionElement.transform.FindDeepChild("Button Add").GetComponent<Button>().onClick
            .AddListener(AddSelectionElement);
        selectionElement.transform.FindDeepChild("Button Remove").GetComponent<Button>().onClick
            .AddListener(() => RemoveSelectionElement(selectionElement));
    }

    private void UpdateAllSelectionElements()
    {
        foreach (Transform element in imageSelectionElements.transform)
        {
            bool isLastElement = element.GetSiblingIndex() == imageSelectionElements.transform.childCount - 1;
            element.FindDeepChild("Button Add").gameObject.SetActive(isLastElement);
            element.FindDeepChild("Button Remove").gameObject
                .SetActive(imageSelectionElements.transform.childCount > 1);
            element.FindDeepChild("Label Image").GetComponent<TextMeshProUGUI>().text =
                string.Format(labelImageTemplate, element.transform.GetSiblingIndex() + 1);
            element.FindDeepChild("Label Audio").GetComponent<TextMeshProUGUI>().text =
                string.Format(labelAudioTemplate, element.transform.GetSiblingIndex() + 1);
        }
    }

    private void AddSelectionElement()
    {
        GameObject newElement = Instantiate(imageSelectionPrefab, imageSelectionElements.transform);
        InitSelectionElement(newElement);
        UpdateAllSelectionElements();
        supportInfosSettingsPanel.SupportInfoContainerChanged();
    }

    private void RemoveSelectionElement(GameObject element)
    {
        DestroyImmediate(element);
        UpdateAllSelectionElements();
        supportInfosSettingsPanel.SupportInfoContainerChanged();
    }
}