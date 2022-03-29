using System.Collections.Generic;
using TMPro;
using translator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generates the list of buttons based on input files in /recording/.
/// </summary>
public class RecorderPanel : MonoBehaviour
{
    public GameObject scrollViewContent;
    public GameObject loadingScreen;
    public Button createRecordingButton;
    public Button startRecordingButton;
    public Button stopPlayingRecordingButton;
    public Button abortRecordingButton;
    public GameObject existingRecordingsPanel;
    public GameObject createRecordingPanel;
    public TMP_Dropdown deviceDropdown;
    public Image levelDisplay;
    public Button playRecordingListButtonPrefab;
    
    [HideInInspector] public string deviceName;

    private AudioClip _microphoneInput;
    private AudioSource _audioSource;

    public void ShowCreateRecordingPanel(bool show)
    {
        existingRecordingsPanel.SetActive(!show);
        createRecordingPanel.SetActive(show);
    }

    public void StartRecording(bool start)
    {
        startRecordingButton.GetComponentInChildren<Text>().text =
            TranslationController.Instance.Translate(start
                ? "paint-shop-start-recording"
                : "paint-shop-stop-recording");
        abortRecordingButton.interactable = !start;
        deviceDropdown.interactable = !start;
        if (!start)
        {
            loadingScreen.SetActive(false);
            ShowCreateRecordingPanel(false);
            ReloadList();
        }
    }

    public void StopPlayingRecordButtonPressed()
    {
        PlayRecordingController.Instance.StopAnimation();
        createRecordingButton.interactable = true;
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
        {
            new(TranslationController.Instance.Translate("paint-shop-no-microphone"))
        };
        foreach (var device in Microphone.devices)
            options.Add(new TMP_Dropdown.OptionData(device));
        deviceDropdown.options = options;
        LoadDevice();
    }

    public void OnEnable()
    {
        ReloadList();
    }

    void Update()
    {
        levelDisplay.fillAmount = GetVolume();
    }

    public void OnDisable()
    {
        // destroy the previously generated button list when the user leaves the panel
        var children = new List<GameObject>();
        foreach (Transform child in scrollViewContent.transform) children.Add(child.gameObject);
        children.ForEach(Destroy);
    }

    private void ReloadList()
    {
        scrollViewContent.transform.DestroyAllChildren();

        // regenerate a new button list with the new button
        DataController.Instance.UpdateLocalRecordings();
        foreach (Recording recording in DataController.Instance.localRecordings)
        {
            Button listButton = Instantiate(playRecordingListButtonPrefab, scrollViewContent.transform);
            listButton.GetComponentInChildren<Text>().text = recording.name;
            listButton.onClick.AddListener(() => OnPlayRecordingButtonPressed(recording));
        }
    }

    private void OnPlayRecordingButtonPressed(Recording recording)
    {
        loadingScreen.SetActive(true);
        stopPlayingRecordingButton.interactable = true;
        createRecordingButton.interactable = false;
        PlayRecordingController.Instance.PlayAnimation(recording, () => loadingScreen.SetActive(false));
        PlayRecordingController.Instance.onFinish = OnFinishPlayingRecording;
    }

    private void OnFinishPlayingRecording()
    {
        PlayRecordingController.Instance.onFinish = null;
        stopPlayingRecordingButton.interactable = false;
        createRecordingButton.interactable = true;
    }

    public void LoadDevice()
    {
        if (deviceDropdown.value == 0)
        {
            deviceName = new(TranslationController.Instance.Translate("paint-shop-no-microphone"));
            _audioSource.Stop();
        }
        else
        {
            deviceName = Microphone.devices[deviceDropdown.value - 1];

            _audioSource.Stop();
            _microphoneInput = Microphone.Start(deviceName, true, 999, 44100);
            _audioSource.clip = _microphoneInput;
            _audioSource.loop = true;

            if (Microphone.IsRecording(deviceName))
            {
                // check that the mic is recording, otherwise you'll get stuck in an infinite loop waiting for it to start
                while (!(Microphone.GetPosition(deviceName) > 0))
                {
                    // wait until the recording has started. 
                } 

                Debug.Log("recording started with " + deviceName);

                // start playing the audio source
                _audioSource.Play();
            }
            else
            {
                // microphone doesn't work for some reason
                Debug.Log(deviceName + " doesn't work!");
            }
        }
    }

    private float GetVolume()
    {
        if (deviceName == TranslationController.Instance.Translate("paint-shop-no-microphone"))
            return 0;

        int dec = 128;
        float[] waveData = new float[dec];

        int micPosition = Microphone.GetPosition(deviceName) - (dec + 1); // null means the first microphone
        _microphoneInput.GetData(waveData, micPosition);

        float levelMax = 0;
        for (int i = 0; i < dec; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }

        float level = Mathf.Sqrt(Mathf.Sqrt(levelMax));
        return level;
    }
}