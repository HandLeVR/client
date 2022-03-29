using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single/multiple choice question sub task
/// </summary>
public class QuestionSubTaskContainer : BaseSubTaskContainer
{
    public GameObject SimpleChoiceSelectionElements;
    public GameObject SimpleChoiceDisplayElements;
    public GameObject SimpleChoiceDisplayPrefab;

    public TMP_InputField audioInputField;
    public TMP_InputField speechBubbleInputField;
    public Toggle automaticAudioToggle;
    public TMP_InputField skippableInputField;
    public TMP_InputField finalAudioInputField;
    public TMP_InputField questionInput;
    public Toggle shuffleToggle;
    public TMP_InputField minNumberInput;
    public TMP_InputField maxNumberInput;
    public TMP_InputField orientationInput;
    public TextMeshProUGUI simpleChoiceText;

    protected override void SetUpByProperties()
    {
        SetTextInputField(questionInput, "question");
        SetToggle(shuffleToggle, "shuffle", true);
        SetTextInputField(maxNumberInput, "maxAnswers");
        SetTextInputField(minNumberInput, "minAnswers");
        SetSkippableInputField(skippableInputField);
        SetTranslationTextInputField(orientationInput, "orientation");
        SetAudioInputField(finalAudioInputField, "finalAudioId");
        SetInstructorSettings(audioInputField, speechBubbleInputField, automaticAudioToggle);
        
        SimpleChoiceSelectionElements.SetActive(false);
        SimpleChoiceDisplayElements.transform.DestroyImmediateAllChildren();

        List<SimpleChoice> simpleChoices = GetSimpleChoices("answersText", "answersCorrect");
        for (int i = 0; i < simpleChoices.Count; i++)
        {
            GameObject newElement = Instantiate(SimpleChoiceDisplayPrefab, SimpleChoiceDisplayElements.transform);
            SetAnswer(i + 1, newElement.transform, simpleChoices[i]);
        }
        simpleChoiceText.transform.gameObject.SetActive(simpleChoices.Count > 0);
        SimpleChoiceDisplayElements.SetActive(simpleChoices.Count > 0);
    }

    public override bool ValuesMissing()
    {
        return questionInput.text == "" || maxNumberInput.text == "" || minNumberInput.text == "" || SimpleChoiceValueMissing() ;
    }

    private bool SimpleChoiceValueMissing()
    {
        if (SimpleChoiceDisplayElements.transform.childCount == 0) return true;
        return false;  
    }

    private void SetTranslationTextInputField(TMP_InputField inputField, string propertyName)
    {
        inputField.text = properties.TryGetValue(propertyName, out var text)
            ? TranslationController.Instance.Translate((string)text)
            : "";
        inputField.transform.parent.gameObject.SetActive(inputField.text != "");
    }

    private void SetAnswer(int i, Transform elementTransform, SimpleChoice simpleChoice)
    {
        elementTransform.GetComponentInChildren<TextMeshProUGUI>().text = "Antwort " + i + ":";
        elementTransform.GetComponentInChildren<TMP_InputField>().text = simpleChoice.label;
        elementTransform.GetComponentInChildren<Toggle>().isOn = simpleChoice.correct;
    }

    private List<SimpleChoice> GetSimpleChoices(string propertyAnswer, string propertyCorrect)
    {
        List<string> answers = GetAnswerTexts(propertyAnswer);
        List<bool> correctnessList = GetAnswerCorrectness(propertyCorrect);
        List<SimpleChoice> simpleChoices = new List<SimpleChoice>();
        if (answers.Count == correctnessList.Count)
        {
            for (int i = 0; i < answers.Count; i++)
            {
                SimpleChoice simpleChoice = new SimpleChoice(answers[i], correctnessList[i]);
                simpleChoices.Add(simpleChoice);
            }

            return simpleChoices;
        }

        return null;
    }

    private List<string> GetAnswerTexts(string propertyName)
    {
        List<string> answers = new List<string>();
        if (properties.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken text in jToken)
                answers.Add((string)text);
        return answers;
    }

    private List<bool> GetAnswerCorrectness(string propertyName)
    {
        List<bool> correctnessList = new List<bool>();
        if (properties.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken correct in jToken)
                correctnessList.Add((bool)correct);
        return correctnessList;
    }
}