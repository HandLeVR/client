using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the single/multi choice questions sub task. The user can select the correct answers and after the continue
/// coin is selected, the correct answers are marked.
/// </summary>
public class QuestionController : VRSubTaskController
{
    public SelectionPanel selectionPanel;

    private bool displayingCorrectAnswers;
    private bool oldDisplayingCorrectAnswers;
    private bool oldCanSkipSpeech;
    private ChoiceInteraction choiceInteraction;
    private bool multiCorrect;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        List<SimpleChoice> simpleChoices = new List<SimpleChoice>();
        JToken answersText = jsonObject.GetValue("answersText");
        JToken answersCorrect = jsonObject.GetValue("answersCorrect");
        for (int i = 0; i < answersText.Count(); i++)
            simpleChoices.Add(new SimpleChoice((string)answersText[i], (bool)answersCorrect[i]));
        multiCorrect = simpleChoices.Count(choice => choice.correct) > 1;

        string text = (string)jsonObject.GetValue("question");
        bool shuffle = GetBoolFromJSON("shuffle", jsonObject);
        int maxAnswers = (int)jsonObject.GetValue("maxAnswers");
        int minAnswers = (int)jsonObject.GetValue("minAnswers");
        choiceInteraction = new ChoiceInteraction(text, shuffle, maxAnswers, minAnswers, "", simpleChoices);

        LearningScenariosMonitorController.Instance.ChangePanel("");
        selectionPanel.FadeIn(choiceInteraction.label, choiceInteraction.maxChoices);
        List<SimpleChoice> choices = choiceInteraction.simpleChoices;
        if (choiceInteraction.shuffle)
            choices = new List<SimpleChoice>(choiceInteraction.simpleChoices.OrderBy(a => Random.value));
        choices.ForEach(choice => selectionPanel.AddItem(choice.label, choice.correct));
        displayingCorrectAnswers = false;

        SetEducationMasterAndCoins(dontShowGoldenCoin: true);
        oldCanSkipSpeech = canSkipSpeech;
    }

    public override void ContinueCoinFadedOut()
    {
        if (oldDisplayingCorrectAnswers)
            base.ContinueCoinFadedOut();
        else
            SpawnCoins(!displayingCorrectAnswers || finalAudioClip == null || oldCanSkipSpeech, true,
                !displayingCorrectAnswers);
    }

    public override void ContinueCoinSelected()
    {
        oldDisplayingCorrectAnswers = displayingCorrectAnswers;
        if (displayingCorrectAnswers)
            selectionPanel.FadeOut();
        else
            CheckAnswers();
    }

    public override void ReturnCoinSelected()
    {
        selectionPanel.FadeOut();
    }

    protected override void AfterEducationMasterSpeech()
    {
        base.AfterEducationMasterSpeech();
        SpawnCoins(true, true, !displayingCorrectAnswers);
    }

    /// <summary>
    /// Checks which answers are correct.
    /// </summary>
    private void CheckAnswers()
    {
        bool allCorrect = true;
        int selected = 0;
        foreach (ChoiceItem item in selectionPanel.items)
        {
            allCorrect &= item.isCorrect == item.HasBall;
            if (item.HasBall)
                selected++;
        }

        if (selected < choiceInteraction.minChoices && !canSkipAll)
            ShowWarning();
        else
            ShowCorrectAnswers(allCorrect);
    }

    private void ShowCorrectAnswers(bool isCorrect)
    {
        if (isCorrect)
            selectionPanel.ShowInfo("Richtig geantwortet!");
        else
            selectionPanel.ShowInfo("Falsch geantwortet! Die " +
                                    (multiCorrect ? "richtigen Antworten werden" : "richtige Antwort wird") +
                                    " jetzt angezeigt.");
        selectionPanel.EvaluateAnswers();
        displayingCorrectAnswers = true;

        PlayFinalAudioIfSet();
    }

    private void ShowWarning()
    {
        selectionPanel.ShowWarning(choiceInteraction.minChoices == 1
            ? $"Mindestens {choiceInteraction.minChoices} Antwort muss ausgewählt werden!"
            : $"Mindestens {choiceInteraction.minChoices} Antworten müssen ausgewählt werden!");
        displayingCorrectAnswers = false;
    }
}