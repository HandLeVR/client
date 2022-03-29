using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Allows to display a video on the monitor.
/// </summary>
public class VideoPlayerPanel : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Slider slider;
    public GameObject videoListPanel;
    
    public Sprite playSprite;
    public Sprite pauseSprite;
    public Image playButtonImage;

    public Text currentMinutes;
    public Text currentSeconds;
    public Text totalMinutes;
    public Text totalSeconds;

    public UnityEvent<float> onSeek;
    public UnityEvent onPause;

    private bool _seeking;

    private double Time => videoPlayer.time;
    private ulong Duration => (ulong) (videoPlayer.frameCount / videoPlayer.frameRate);
    private double NTime => Time / Duration;
    
    private void Start()
    {
        // resets the video screen texture
        videoPlayer.targetTexture.Release();
    }

    private void LateUpdate()
    {
        // sets the time display
        SetCurrentTimeUI();
        if (videoPlayer.isPlaying)
        {
            // moves the handle along the progress bar
            if (!_seeking) 
                slider.value = (float) NTime;
            playButtonImage.sprite = pauseSprite;
        }
        else
        {
            playButtonImage.sprite = playSprite;
        }
    }

    private void OnEnable()
    {
        videoPlayer.errorReceived += ErrorReceived;
        videoPlayer.prepareCompleted += PrepareCompleted;
        videoPlayer.seekCompleted += SeekCompleted;
        
        onSeek ??= new UnityEvent<float>();
    }
    
    public void PlayClip(String path)
    {
        videoPlayer.url = path;
        PlayClip();
    }
    
    public void PlayClip(VideoClip clip)
    {
        videoPlayer.clip = clip;
        PlayClip();
    }
    
    private void PlayClip()
    {
        SetRendering(true);
        videoPlayer.Play();
        playButtonImage.sprite = pauseSprite;  
        onSeek.Invoke(0f);
    }
    
    public void PlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            onPause.Invoke();
        }
        else
        {
            videoPlayer.Play();
            // condition needed because videoPlayer.time will return the time of the last frame if video is replayed
            onSeek.Invoke(videoPlayer.length <= 0 ? 0 : (float) videoPlayer.time);
        }
    }
    
    public void ClosePlayer()
    {
        videoPlayer.Pause();
        playButtonImage.sprite = playSprite;
        SetRendering(false);
        videoPlayer.targetTexture.Release();
        videoPlayer.Pause();
        onPause.Invoke();
        onSeek.RemoveAllListeners();
        onPause.RemoveAllListeners();
    }

    public void SetSlider(Vector3 hitPosition)
    {
        float sliderWidth = slider.GetComponent<RectTransform>().rect.width;
        Vector3 localPos = slider.transform.InverseTransformPoint(hitPosition);
        float handlePosPixel = (sliderWidth / 2) + localPos.x;
        float handlePosPercent = Mathf.Clamp(handlePosPixel / sliderWidth, 0, 1);
        slider.value = handlePosPercent;
        Seek(handlePosPercent);
    }

    private void OnDisable()
    {
        videoPlayer.errorReceived -= ErrorReceived;
        videoPlayer.prepareCompleted -= PrepareCompleted;
        videoPlayer.seekCompleted -= SeekCompleted;
    }

    private void ErrorReceived(VideoPlayer v, string msg)
    {
        Debug.Log("Video Player Error: "+msg);    
    }

    private void PrepareCompleted(VideoPlayer v)
    {
        SetTotalTimeUI();
    }
    
    private void SeekCompleted(VideoPlayer v)
    {
        StartCoroutine(SeekingActivator());
    }

    /// <summary>
    /// Seeking = false with a bit of a delay to ensure that the UI can keep up with properly displaying the slider
    /// </summary>
    private IEnumerator SeekingActivator()
    {
        yield return new WaitForSeconds(0.1f);
        _seeking = false;
    }

    private void SetCurrentTimeUI()
    {
        string minutes = Mathf.Floor((int) videoPlayer.time / 60).ToString("00");
        string seconds = ((int) videoPlayer.time % 60).ToString("00");

        currentMinutes.text = minutes;
        currentSeconds.text = seconds;

    }

    private void SetTotalTimeUI()
    {
        string minutes = ((Duration-(Duration % 60))/60).ToString("00");
        string seconds = (Duration % 60).ToString("00");
        totalMinutes.text = minutes;
        totalSeconds.text = seconds;
    }

    /// <summary>
    /// Reads the slider information and sets the video to that time.
    /// </summary>
    private void Seek(float nTime)
    {
        nTime = Mathf.Clamp(nTime, 0, 1);
        if (!_seeking)
        {
            videoPlayer.time = nTime * Duration;
            _seeking = true;
            onSeek.Invoke(nTime * Duration);
        }
    }

    /// <summary>
    /// Objects can't all just be deactivated because SprayGun.cs needs to find some of them at runtime to
    /// function and interact properly.
    /// </summary>
    private void SetRendering(bool active)
    {
        if (active)
        {
            gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(true);
            if (videoListPanel)
                videoListPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
            videoPlayer.Pause();
            videoPlayer.gameObject.SetActive(false);
            if (videoListPanel)
                videoListPanel.SetActive(true);
        }
    }
}