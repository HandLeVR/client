using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// Manages the support info summary sub task. This sub task can be used to display a single or multiple support infos.
/// Multiple support infos can be displayed sequentially or the users may select the next support info manually.
/// </summary>
public class SupportInfoSummaryController : VRSubTaskController
{
    public SupportInfoSummaryPanel supportInfoSummaryPanel;
    public VideoPlayerPanel videoPlayerPanel;
    public SelectionPanel selectionPanel;

    private long _currentTaskId;
    private string _monitorText;
    private int _minSupportInfo;
    private HashSet<int> _visitedSupportInfos;
    private AudioClip _reminder;
    private AudioClip _finalReminder;
    private List<SupportInfo> _supportInfos;
    private int _currentSupportInfoIndex;
    private bool _sequence;

    private void OnEnable()
    {
        JObject jsonObject = JObject.Parse(subTask.properties);
        _monitorText = GetStringFromJSON("textMonitor", jsonObject);
        _supportInfos = new List<SupportInfo>();
        foreach (JToken supportInfo in jsonObject.GetValue("supportInfos"))
            _supportInfos.Add(supportInfo.ToObject<SupportInfo>());
        _minSupportInfo = GetIntFromJSON("minSupportInfos", jsonObject);

        // reset visited support infos only if we are in a different task
        if (_currentTaskId != LearningScenariosTaskController.Instance.currentTask.id)
            _visitedSupportInfos = new HashSet<int>();
        _currentTaskId = LearningScenariosTaskController.Instance.currentTask.id;

        _reminder = null;
        jsonObject.TryGetValue("reminderAudioId", out JToken mediaJson);
        if (mediaJson != null)
            _reminder = TaskPreparationController.Instance.loadedAudioClips[(int)mediaJson];

        AudioClip finalReminderAudioClip = null;
        jsonObject.TryGetValue("finalReminderAudioId", out mediaJson);
        if (mediaJson != null)
            finalReminderAudioClip = TaskPreparationController.Instance.loadedAudioClips[(int)mediaJson];

        _finalReminder = mediaJson != null ? finalReminderAudioClip : _reminder;
        LearningScenariosMonitorController.Instance.ChangePanel(supportInfoSummaryPanel.gameObject);

        InitEducationMaster();
        SetSkippable();

        _currentSupportInfoIndex = -1;
        _sequence = jsonObject.TryGetValue("sequence", out JToken sequenceJson) && (bool)sequenceJson;
        if (_sequence)
        {
            // needed to show the continue coin immediately if the first support info contains audio
            VirtualInstructorController.Instance.speechStarted.AddListener(AfterEducationMasterSpeech);
            ShowSupportInfoPanel(0, false);
        }
        else
        {
            InitSelectionPanel();
            SpawnCoins(_minSupportInfo == 0 && canSkipSpeech, true);
        }
    }

    private void OnDisable()
    {
        if (videoPlayerPanel.gameObject.activeSelf)
            videoPlayerPanel.ClosePlayer();
    }

    protected override void AfterEducationMasterSpeech()
    {
        VirtualInstructorController.Instance.speechStarted.RemoveAllListeners();
        base.AfterEducationMasterSpeech();
        if (_sequence)
            SpawnCoins(canSkipSpeech, true, !_sequence || !LastSupportInfo());
        else
            ChangeToCurrentSupportInfo();
    }

    private void InitSelectionPanel()
    {
        selectionPanel.FadeIn(_monitorText, 1);
        for (var i = 0; i < _supportInfos.Count; i++)
        {
            int index = i;
            selectionPanel.AddItem(_supportInfos[i].name, onSelection: () => ChangeToSupportInfo(index));
        }
    }

    private void ChangeToSupportInfo(int supportInfoIndex)
    {
        _currentSupportInfoIndex = supportInfoIndex;
        ChangeToCurrentSupportInfo();
    }

    /// <summary>
    /// Only changes to the a support info if it is selected and the virtual instructor speech finished.
    /// </summary>
    private void ChangeToCurrentSupportInfo()
    {
        if (canSkipSpeech && _currentSupportInfoIndex >= 0)
            selectionPanel.FadeOut(false,
                () => ShowSupportInfoPanel(_currentSupportInfoIndex));
    }

    private void ReturnToSupportInfoSummaryPanel()
    {
        _currentSupportInfoIndex = -1;
        InitSelectionPanel();
        SpawnCoins(MinSupportInfosReached(), true);

        VirtualInstructorController.Instance.Pause();
        InitEducationMaster();
        if (videoPlayerPanel.gameObject.activeSelf)
            videoPlayerPanel.ClosePlayer();
        LearningScenariosMonitorController.Instance.ChangePanel(supportInfoSummaryPanel.gameObject);

        if (_visitedSupportInfos.Count < _minSupportInfo && _reminder)
            VirtualInstructorController.Instance.Speak(_reminder);
        else if (_visitedSupportInfos.Count >= _minSupportInfo)
        {
            if (_finalReminder)
                VirtualInstructorController.Instance.Speak(_finalReminder);
            else if (_reminder)
                VirtualInstructorController.Instance.Speak(_reminder);
        }
    }

    private void ShowSupportInfoPanel(int index, bool stopEducationMaster = true)
    {
        if (videoPlayerPanel.gameObject.activeSelf)
            videoPlayerPanel.ClosePlayer();

        _visitedSupportInfos.Add(index);
        _currentSupportInfoIndex = index;
        SpawnCoins(canSkipSpeech, true, !_sequence || !LastSupportInfo());
        supportInfoSummaryPanel.ShowSupportInfoPanel(_supportInfos[index], stopEducationMaster);
    }

    public override void ReturnCoinFadedOut()
    {
        if (_sequence)
        {
            if (_currentSupportInfoIndex == 0)
                base.ReturnCoinFadedOut();
            else
                ShowSupportInfoPanel(--_currentSupportInfoIndex);
        }
        else
        {
            if (_currentSupportInfoIndex == -1)
                base.ReturnCoinFadedOut();
            else
                ReturnToSupportInfoSummaryPanel();
        }
    }

    public override void ContinueCoinFadedOut()
    {
        if (_sequence)
        {
            if (LastSupportInfo())
                base.ContinueCoinFadedOut();
            else
                ShowSupportInfoPanel(++_currentSupportInfoIndex);
        }
        else
        {
            // currently in info summary panel
            if (_currentSupportInfoIndex == -1)
                base.ContinueCoinFadedOut();
            // currently viewing a support info
            else
                ReturnToSupportInfoSummaryPanel();
        }
    }

    public override void ContinueCoinSelected()
    {
        if (!_sequence && (LastSupportInfo() || MinSupportInfosReached()))
            selectionPanel.FadeOut();
    }

    public override void ReturnCoinSelected()
    {
        if (!_sequence && _currentSupportInfoIndex <= 0)
            selectionPanel.FadeOut();
    }

    private bool MinSupportInfosReached()
    {
        return _visitedSupportInfos.Count >= _minSupportInfo;
    }

    private bool LastSupportInfo()
    {
        return _currentSupportInfoIndex >= _supportInfos.Count - 1;
    }
}