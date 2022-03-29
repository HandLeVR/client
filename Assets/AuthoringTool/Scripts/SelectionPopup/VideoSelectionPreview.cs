using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Provides a preview for videos.
/// </summary>
public class VideoSelectionPreview : MonoBehaviour
{
    public TMP_InputField videoNameInputField;
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public RenderTexture renderTexture;
    public TMP_InputField descriptionInputField;
    public TMP_InputField authorInputField;
    public Scrollbar slider;
    public TextMeshProUGUI currentTimeTextField;
    public TextMeshProUGUI maxTimeTextField;
    public Transform fullscreenContainer;
    public MediaPreviewFullscreenVideo fullscreenVideoPreview;
    public Button btn_zoom;

    private float max;
    
    private void OnEnable()
    {
        ClearPreview();
    }

    private void Start()
    {
        rawImage.texture = null;
    }

    void Update()
    {
        if (videoPlayer.isPlaying)
        {
            TimeSpan current_timespan = TimeSpan.FromSeconds(videoPlayer.time);
            currentTimeTextField.text =
                $"{current_timespan.Hours:D2}:{current_timespan.Minutes:D2}:{current_timespan.Seconds:D2}";
            slider.value = (float)videoPlayer.time / max;
        }
    }

    public void SetUpPreviewPanel(Media media)
    {
        videoNameInputField.text = media.name;
        rawImage.texture = null;
        descriptionInputField.gameObject.SetActive(media.description.Length > 0);
        descriptionInputField.text = media.description;
        descriptionInputField.textComponent.enableWordWrapping = true;
        authorInputField.text = media.permission.createdByFullName;
        btn_zoom.onClick.AddListener(ShowFullscreenPreview);
        btn_zoom.interactable = false;
        if (File.Exists(media.GetPath()))
            StartCoroutine(LoadVideoFileCoroutine(media.GetPath()));
    }

    private IEnumerator LoadVideoFileCoroutine(string filepath)
    {
        rawImage.texture = renderTexture;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = "file:///" + filepath;
        videoPlayer.Prepare();
        StartCoroutine(ShowStart());
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        max = (float)videoPlayer.length;
        TimeSpan timespan = TimeSpan.FromSeconds(max);
        maxTimeTextField.text = $"{timespan.Hours:D2}:{timespan.Minutes:D2}:{timespan.Seconds:D2}";
        slider.value = 0.0f;
        btn_zoom.interactable = true;
    }

    private void ClearPreview()
    {
        videoNameInputField.text = "";
        descriptionInputField.text = "";
        currentTimeTextField.text = "";
        maxTimeTextField.text = "";
        rawImage.texture = null;
        videoPlayer.clip = null;
        btn_zoom.interactable = false;
    }

    private void ShowFullscreenPreview()
    {
        if (videoPlayer.length > 0)
        {
            fullscreenContainer.gameObject.SetActive(true);
            foreach (Transform child in fullscreenContainer.GetChild(0))
                child.gameObject.SetActive(false);
            fullscreenVideoPreview.gameObject.SetActive(true);
            fullscreenVideoPreview.Init(videoNameInputField.text, videoPlayer.isPlaying, slider);
            videoPlayer.Pause();
        }
    }

    private IEnumerator ShowStart()
    {
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.Play();
        while (videoPlayer.frame == -1)
            yield return null;
        videoPlayer.Pause();
        videoPlayer.SetDirectAudioMute(0, false);
    }
}