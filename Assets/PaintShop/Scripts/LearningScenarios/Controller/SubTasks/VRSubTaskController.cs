using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Base class for all sub task controller. Contains methods used by multiple sub task controller.
/// </summary>
public class VRSubTaskController : MonoBehaviour
{
    public SubTask subTask;

    // determines whether the hole task can be skipped
    protected bool canSkipAll;

    // determines whether the speech of the virtual instructor can be skipped
    protected bool canSkipSpeech;

    // possible audio at the end of a task
    protected AudioClip finalAudioClip;

    // determines whether the task contains any audio
    protected bool containsAudio;

    // determines whether audio should be played automatically when the task starts
    protected bool automaticAudio;

    protected virtual void FinishSubTask()
    {
        Reset();
        LearningScenariosTaskController.Instance.FinishCurrentSubTask();
    }

    private void ReturnToLastSubTask()
    {
        Reset();
        LearningScenariosTaskController.Instance.ReturnToLastSubTask();
    }

    protected virtual void Reset()
    {
        if (VirtualInstructorController.Instance.gameObject.activeSelf)
        {
            VirtualInstructorController.Instance.Stop();
            VirtualInstructorController.Instance.speechComplete.RemoveAllListeners();
            VirtualInstructorController.Instance.gameObject.SetActive(false);
        }

        ApplicationController.Instance.sprayGun.isDisabled = true;
    }

    protected void SetEducationMasterAndCoins(bool dontShowContinueCoin = false, bool showContinueCoin = false,
        bool dontShowReturnCoin = false, bool dontShowGoldenCoin = false,
        VirtualInstructorController.InstructorPosition position =
            VirtualInstructorController.InstructorPosition.Introduction, bool showEducationMaster = true)
    {
        if (showEducationMaster)
            InitEducationMaster(position);
        SetSkippable();
        SpawnCoins(showContinueCoin || canSkipAll || !dontShowContinueCoin && canSkipSpeech, !dontShowReturnCoin,
            dontShowGoldenCoin);
    }

    /// <summary>
    /// Determines whether the speech of the virtual instructor or the hole task can be skipped.
    /// </summary>
    protected void SetSkippable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        int skippable = (int)jsonObject.GetValue("skippable");
        canSkipSpeech = skippable >= 1 || !containsAudio;
        canSkipAll = skippable == 2;
    }

    protected void InitEducationMaster(
        VirtualInstructorController.InstructorPosition position =
            VirtualInstructorController.InstructorPosition.Introduction)
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        automaticAudio = false;
        containsAudio = jsonObject.TryGetValue("audioId", out JToken mediaJson);
        if (containsAudio)
        {
            AudioClip audioClip = TaskPreparationController.Instance.loadedAudioClips[(int)mediaJson];
            string textSpeechBubble = jsonObject.TryGetValue("textSpeechBubble", out JToken textSpeechBubbleJson)
                ? (string)textSpeechBubbleJson
                : "";
            VirtualInstructorController.Instance.Init(audioClip, speechBubbleText: textSpeechBubble,
                afterSpeech: AfterEducationMasterSpeech, position: position);
            VirtualInstructorController.Instance.active = true;

            // let instructor speak immediately if the corresponding property is set
            automaticAudio = jsonObject.TryGetValue("automaticAudio", out JToken automaticAudioJson) &&
                             (bool)automaticAudioJson;
            if (automaticAudio)
                VirtualInstructorController.Instance.Speak();
        }

        finalAudioClip = jsonObject.TryGetValue("finalAudioId", out JToken finalAudioMediaJson)
            ? TaskPreparationController.Instance.loadedAudioClips[(int)finalAudioMediaJson]
            : null;
    }

    protected void PlayFinalAudioIfSet()
    {
        if (finalAudioClip != null)
        {
            VirtualInstructorController.Instance.Init(finalAudioClip, afterSpeech: AfterEducationMasterSpeech);
            VirtualInstructorController.Instance.active = true;
            VirtualInstructorController.Instance.Speak();
        }
        else
        {
            AfterEducationMasterSpeech();
        }
    }

    protected void SpawnCoins(bool spawnContinueCoin, bool spawnReturnCoin, bool dontShowGoldenCoin = false)
    {
        CoinController.Instance.FadeInOrOutCoins(
            spawnContinueCoin || LearningScenariosTaskController.Instance.SubTaskExecuted(), spawnReturnCoin,
            !dontShowGoldenCoin && LearningScenariosTaskController.Instance.IsLastSubTask());
    }

    /// <summary>
    /// Called when the return coin was faded out.
    /// </summary>
    public virtual void ReturnCoinFadedOut()
    {
        ReturnToLastSubTask();
    }

    /// <summary>
    /// Called when the continue coin was faded out.
    /// </summary>
    public virtual void ContinueCoinFadedOut()
    {
        FinishSubTask();
    }

    /// <summary>
    /// Called when the return coin was selected.
    /// </summary>
    public virtual void ReturnCoinSelected()
    {
    }

    /// <summary>
    /// Called when the continue coin was selected.
    /// </summary>
    public virtual void ContinueCoinSelected()
    {
    }

    protected bool GetBoolFromJSON(string propertyName, JObject jsonObject)
    {
        return jsonObject.TryGetValue(propertyName, out JToken value) && (bool)value;
    }

    protected string GetStringFromJSON(string propertyName, JObject jsonObject)
    {
        return jsonObject.TryGetValue(propertyName, out JToken value) ? (string)value : "";
    }

    protected long GetLongFromJSON(string propertyName, JObject jsonObject)
    {
        return jsonObject.TryGetValue(propertyName, out JToken value) ? (long)value : 0;
    }

    protected int GetIntFromJSON(string propertyName, JObject jsonObject)
    {
        return jsonObject.TryGetValue(propertyName, out JToken value) ? (int)value : 0;
    }

    protected List<JObject> GetItemsFromJSON(string propertyName, JObject jsonObject)
    {
        List<JObject> objects = new List<JObject>();
        if (jsonObject.TryGetValue(propertyName, out JToken jToken))
            foreach (JToken text in jToken)
                objects.Add((JObject)text);
        return objects;
    }

    protected void LoadCoat(long coatId)
    {
        if (coatId > 0)
            PaintController.Instance.LoadCoat(coatId);
        else if (coatId == -1)
        {
            if (LearningScenariosTaskController.Instance.selectedCoat != null)
                PaintController.Instance.LoadCoat(LearningScenariosTaskController.Instance.selectedCoat.name, false);
            else
                PaintController.Instance.LoadCoat(DataController.Instance.coats.Values.ToList()[0].id);
        }
    }

    protected void LoadBaseCoat(long baseCoatId, bool dry = true)
    {
        if (baseCoatId > 0)
            PaintController.Instance.LoadBaseCoat(baseCoatId, dry);
        else if (baseCoatId == -1)
        {
            if (LearningScenariosTaskController.Instance.selectedCoat != null)
                PaintController.Instance.LoadBaseCoat(LearningScenariosTaskController.Instance.selectedCoat.name, dry);
            else
                PaintController.Instance.LoadBaseCoat(0);
        }
        else if (baseCoatId == -3)
            PaintController.Instance.LoadBaseCoat(0);
    }

    protected virtual void AfterEducationMasterSpeech()
    {
        canSkipSpeech = true;
    }
}