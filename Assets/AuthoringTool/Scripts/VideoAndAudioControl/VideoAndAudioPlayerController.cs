using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Controller for a video and audio player.
/// </summary>
public class VideoAndAudioPlayerController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public Button playButton;
    public Button pauseButton;
    public Button muteButton;
    public SliderChangeValue sliderPlayTime;
    public TextMeshProUGUI currentTimeTMP;
    public TextMeshProUGUI maxTimeTMP;
    public Image displayMuteStatus;
    public Sprite musicOnSymbol;
    public Sprite musicOffSymbol;
    
    private Media.MediaType currentMediaType;
    
    void Start()
    {
        playButton.onClick.AddListener(Play);
        pauseButton.onClick.AddListener(Pause);
        muteButton.onClick.AddListener(ToggleMusic);
    }

    private void OnDisable()
    {
        videoPlayer.transform.parent.gameObject.SetActive(false);
        audioSource.gameObject.SetActive(false);
    }

    public void ChangeMediaType(Media.MediaType mediaType)
    {
        currentMediaType = mediaType;
        sliderPlayTime.audioSource = null;
        sliderPlayTime.videoPlayer = null;
        sliderPlayTime.Reset();
        videoPlayer.transform.parent.gameObject.SetActive(false);
        audioSource.gameObject.SetActive(false);
        muteButton.gameObject.SetActive(mediaType == Media.MediaType.Video);
        switch (currentMediaType)
        {
            case Media.MediaType.Video:
                sliderPlayTime.videoPlayer = videoPlayer;
                SetVideoActive(false);
                break;
            case Media.MediaType.Audio:
                sliderPlayTime.audioSource = audioSource;
                audioSource.gameObject.SetActive(true);
                break;
        }
    }

    public void SetVideoActive(bool active)
    {
        videoPlayer.transform.parent.gameObject.SetActive(active);
    }

    private void Update()
    {
        maxTimeTMP.text = "00:00:00";
        currentTimeTMP.text = "00:00:00";
        if (currentMediaType == Media.MediaType.Video)
        {
            if (videoPlayer.isPrepared)
                maxTimeTMP.text = TimeSpan.FromSeconds(videoPlayer.length).ToString(@"hh\:mm\:ss");
            if (videoPlayer.isPlaying)
                currentTimeTMP.text = TimeSpan.FromSeconds(videoPlayer.time).ToString(@"hh\:mm\:ss");
        }
        else if (currentMediaType == Media.MediaType.Audio)
        {
            if (audioSource.clip)
                maxTimeTMP.text = TimeSpan.FromSeconds(audioSource.clip.length).ToString(@"hh\:mm\:ss");
            if (audioSource.isPlaying)
                currentTimeTMP.text = TimeSpan.FromSeconds(audioSource.time).ToString(@"hh\:mm\:ss");
        }
    }

    /// <summary>
    /// (Un)mutes the sound of the currently playing video.
    /// </summary>
    public void ToggleMusic()
    {
        displayMuteStatus.sprite = videoPlayer.GetDirectAudioMute(0) == true ? musicOnSymbol : musicOffSymbol;
        videoPlayer.SetDirectAudioMute(0, !videoPlayer.GetDirectAudioMute(0));
    }

    public void Play()
    {
        switch (currentMediaType)
        {
            case Media.MediaType.Video:
                if (!videoPlayer.isPlaying)
                    videoPlayer.Play();
                break;
            case Media.MediaType.Audio:
                if (!audioSource.isPlaying)
                    audioSource.Play();
                break;
        }
    }
    
    public void Stop()
    {
        switch (currentMediaType)
        {
            case Media.MediaType.Video:
                videoPlayer.Stop();
                break;
            case Media.MediaType.Audio:
                audioSource.Stop();
                break;
        }
    }

    public void Pause()
    {
        switch (currentMediaType)
        {
            case Media.MediaType.Video:
                if (videoPlayer.isPlaying)
                    videoPlayer.Pause();
                break;
            case Media.MediaType.Audio:
                if (audioSource.isPlaying)
                    audioSource.Pause();
                break;
        }
    }

    /// <summary>
    /// The video player loads the file via url (given by filepath (if not empty)) and gets ready to play the clip.
    /// </summary>
    /// <param name="filepath">Absolute path to the video file</param>
    public void LoadVideo(string filepath)
    {
        SetVideoActive(true);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = "file:///" + filepath;
        videoPlayer.Prepare();
        StartCoroutine(ShowStart());
    }
    
    public void LoadAudio(string filepath)
    {
        StartCoroutine(LoadAudioFileCoroutine(filepath));
    }
    
    private IEnumerator LoadAudioFileCoroutine(string filepath)
    {
        string url = "file:///" + filepath;
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            uwr.useHttpContinue = false;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log(uwr.error);
            else
                audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
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
