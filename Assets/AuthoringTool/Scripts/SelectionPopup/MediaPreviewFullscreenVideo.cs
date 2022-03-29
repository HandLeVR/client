using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Allows to display a video in the preview panel as fullscreen.
/// </summary>
public class MediaPreviewFullscreenVideo : MonoBehaviour
{
    public TextMeshProUGUI fileNameTMP;
    public VideoPlayer videoPlayer;
    public Scrollbar slider;
    public TextMeshProUGUI currentTimeTextField;
    public TextMeshProUGUI maxTimeTextField;
    public VideoPlayer originVideoPlayer;
    public Scrollbar originSlider;
    private float max;

    public void Init(string filename, bool wasPlaying, Scrollbar origlider)
    {
        originVideoPlayer.Pause();
        max = (float)originVideoPlayer.length;
        fileNameTMP.text = filename;
        try
        {
            StartCoroutine(LoadVideoFileCoroutine(originVideoPlayer.url, wasPlaying));
        }
        catch (Exception e)
        {
            Debug.LogWarning(" Fehler beim Laden - " + filename + "\n" + e.Message);
        }
        originSlider = origlider;
    }

    private IEnumerator LoadVideoFileCoroutine(string url, bool wasPlaying)
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;
        TimeSpan timespan = TimeSpan.FromSeconds(max);
        maxTimeTextField.text = $"{timespan.Hours:D2}:{timespan.Minutes:D2}:{timespan.Seconds:D2}";
        TimeSpan current_timespan = TimeSpan.FromSeconds(videoPlayer.time);
        currentTimeTextField.text = string.Format("{0:D2}:{1:D2}:{2:D2}", current_timespan.Hours, current_timespan.Minutes, current_timespan.Seconds);
        videoPlayer.time = originVideoPlayer.time;
        slider.value = originSlider.value;
        if (wasPlaying)
            videoPlayer.Play();
    }

    public void Close()
    {
        originVideoPlayer.time = videoPlayer.time;
        if (videoPlayer.isPlaying)
            originVideoPlayer.Play();
        originSlider.value = slider.value;
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
}