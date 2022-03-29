using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Settings panel for single/multiple choice questions.
/// </summary>
public class QuestionSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField questionInput;
    public Toggle shuffleToggle;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_InputField finalAudioInput;
    public TMP_Dropdown skippableDropdown;

    public GameObject simpleChoiceSelectionElements;
    public GameObject simpleChoiceDisplayElements;
    public GameObject simpleChoiceSelectionPrefab;

    public Slider minSlider;
    public TextMeshProUGUI minSliderMinValue;
    public TextMeshProUGUI minSliderMaxValue;
    public TextMeshProUGUI minSliderValue;
    public Slider maxSlider;
    public TextMeshProUGUI maxSliderMinValue;
    public TextMeshProUGUI maxSliderMaxValue;
    public TextMeshProUGUI maxSliderValue;

    private int _childCount = 1;
    private int _maxChildCount = 6;

    private const string labelTemplate = "Antwort {0}:";

    public void Awake()
    {
        InitSkippableDropdown(skippableDropdown);
        questionInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        shuffleToggle.onValueChanged.AddListener(delegate { SaveSettings(); });
        SetSliderMinMax(minSlider, minSliderMinValue, minSliderMaxValue, minSliderValue);
        SetSliderMinMax(maxSlider, maxSliderMinValue, maxSliderMaxValue, maxSliderValue);
        minSlider.onValueChanged.AddListener(delegate
        {
            if (minSlider.value > maxSlider.value)
                minSlider.value = maxSlider.value;
            minSliderValue.text = minSlider.value.ToString();
            SaveSettings();
        });
        maxSlider.onValueChanged.AddListener(delegate
        {
            if (maxSlider.value < minSlider.value)
                maxSlider.value = minSlider.value;
            maxSliderValue.text = maxSlider.value.ToString();
            SaveSettings();
        });
        InitAudioInput(finalAudioInput);
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    public void Update()
    {
        if (_childCount != CountSimpleChoicesWithContent())
        {
            _childCount = CountSimpleChoicesWithContent();
            SetSliderMinMax(minSlider, minSliderMinValue, minSliderMaxValue, minSliderValue);
            SetSliderMinMax(maxSlider, maxSliderMinValue, maxSliderMaxValue, maxSliderValue);
        }
    }

    protected override void SetUpByProperties()
    {
        SetSkippableDropdown(skippableDropdown);
        SetTextInputField(questionInput, "question");
        SetToggle(shuffleToggle, "shuffle");
        SetAudioInput(finalAudioInput, "finalAudioId");
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
        simpleChoiceDisplayElements.SetActive(false);
        simpleChoiceSelectionElements.transform.DestroyImmediateAllChildren();
        List<SimpleChoice> simpleChoices = GetSimpleChoices("answersText", "answersCorrect");
        if (!properties.HasValues || simpleChoices.Count == 0)
        {
            InitSelectionElement(Instantiate(simpleChoiceSelectionPrefab, simpleChoiceSelectionElements.transform));
            UpdateAllSelectionElements();
            return;
        }

        foreach (SimpleChoice simpleChoice in simpleChoices)
        {
            GameObject newElement = Instantiate(simpleChoiceSelectionPrefab, simpleChoiceSelectionElements.transform);
            InitSelectionElement(newElement);
            SetAnswer(newElement.transform, simpleChoice);
        }

        UpdateAllSelectionElements();
    }

    /// <summary>
    /// Adds listeners to the modifiable elements and the buttons of a selection element.
    /// </summary>
    private void InitSelectionElement(GameObject selectionElement)
    {
        selectionElement.transform.Find("Simple Choice Container").Find("Input Simple Choice")
            .GetComponentInChildren<TMP_InputField>().onEndEdit.AddListener(delegate { SaveSettings(); });
        selectionElement.transform.Find("Toggle").GetComponent<Toggle>().onValueChanged
            .AddListener(delegate { SaveSettings(); });
        selectionElement.transform.Find("Simple Choice Container").Find("Button Add").GetComponent<Button>().onClick
            .AddListener(AddSelectionElement);
        selectionElement.transform.Find("Simple Choice Container").Find("Button Remove").GetComponent<Button>().onClick
            .AddListener(() => RemoveSelectionElement(selectionElement));
    }

    private void AddSelectionElement()
    {
        InitSelectionElement(Instantiate(simpleChoiceSelectionPrefab, simpleChoiceSelectionElements.transform));
        UpdateAllSelectionElements();
    }

    private void RemoveSelectionElement(GameObject element)
    {
        DestroyImmediate(element);
        UpdateAllSelectionElements();
        SaveSettings();
    }

    /// <summary>
    /// Updates all selection elements to ensure that the indices are correct and only the last element has an
    /// add button.
    /// </summary>
    private void UpdateAllSelectionElements()
    {
        foreach (Transform element in simpleChoiceSelectionElements.transform)
        {
            bool isLastElement = element.GetSiblingIndex() == simpleChoiceSelectionElements.transform.childCount - 1;
            // add button is only active if it is the last element and if the max number of elements is not reached 
            element.Find("Simple Choice Container").Find("Button Add").gameObject
                .SetActive(element.GetSiblingIndex() < _maxChildCount - 1 && isLastElement);
            // the last element cannot be removed
            element.Find("Simple Choice Container").Find("Button Remove").gameObject
                .SetActive(simpleChoiceSelectionElements.transform.childCount > 1);
            element.Find("Simple Choice Container").Find("Label").GetComponent<TextMeshProUGUI>().text =
                string.Format(labelTemplate, element.transform.GetSiblingIndex() + 1);
        }
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        JObject json = new JObject
        {
            { "question", questionInput.text },
            { "shuffle", shuffleToggle.isOn }
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetAudioInputJSON(json, finalAudioInput, "finalAudioId");
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        if ((int)maxSlider.value != 0) json.Add("maxAnswers", (int)maxSlider.value);
        if ((int)minSlider.value != 0) json.Add("minAnswers", (int)minSlider.value);
        List<string> simpleChoiceTexts = new List<string>();
        List<bool> simpleChoiceCorrect = new List<bool>();
        foreach (Transform element in simpleChoiceSelectionElements.transform)
        {
            string text = element.GetComponentInChildren<TMP_InputField>().text;
            bool correct = element.GetComponentInChildren<Toggle>().isOn;
            // prevent adding empty simpleChoices
            if (text != "")
            {
                simpleChoiceTexts.Add(text);
                simpleChoiceCorrect.Add(correct);
            }
        }

        json.Add("answersText", new JArray { simpleChoiceTexts });
        json.Add("answersCorrect", new JArray { simpleChoiceCorrect });
        subTask.properties = json.ToString();
    }

    private int CountSimpleChoicesWithContent()
    {
        int i = 0;
        foreach (Transform child in simpleChoiceSelectionElements.transform)
            if (child.GetComponentInChildren<TMP_InputField>().text != "")
                i++;
        return i;
    }

    /// <summary>
    /// Controls the min and max answers slider.
    /// </summary>
    private void SetSliderMinMax(Slider slider, TextMeshProUGUI minDisplay, TextMeshProUGUI maxDisplay,
        TextMeshProUGUI valueDisplay)
    {
        isSettingUp = true;
        int tmpMin = (int)slider.minValue;
        int tmpMax = (int)slider.maxValue;
        int tmpValue = (int)slider.value;
        if (CountSimpleChoicesWithContent() > 0)
        {
            if ((int)slider.value > tmpMax)
                slider.value = tmpMax;
            else if ((int)slider.value < tmpMin)
                slider.value = tmpMin;
            else
                slider.value = tmpValue;

            slider.maxValue = CountSimpleChoicesWithContent();
            maxDisplay.text = CountSimpleChoicesWithContent().ToString();
            slider.minValue = 1;
            minDisplay.text = 1.ToString();
            valueDisplay.text = slider.value.ToString();
            slider.interactable = true;
        }
        else
        {
            slider.maxValue = 0;
            maxDisplay.text = 0.ToString();
            slider.minValue = 0;
            minDisplay.text = 0.ToString();
            slider.value = 0;
            valueDisplay.text = 0.ToString();
            slider.interactable = false;
        }

        isSettingUp = false;
    }
}