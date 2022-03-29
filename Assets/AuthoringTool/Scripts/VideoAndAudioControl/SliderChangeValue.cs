using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Allows to control a video with the help of a slider.
/// </summary>
public class SliderChangeValue : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [HideInInspector] public VideoPlayer videoPlayer;
    [HideInInspector] public AudioSource audioSource;
    private float percentage;
    private Slider slider;
    private Vector2 point;
    private RectTransform rectTransform;

    void Start()
    {
        slider = GetComponent<Slider>();
        rectTransform = slider.GetComponent<RectTransform>();
        Reset();
    }

    void Update()
    {
        if (videoPlayer)
        {
            if (videoPlayer.isPrepared)
                slider.value = (float)videoPlayer.frame / videoPlayer.frameCount;
        }
        else if (audioSource)
        {
            if (audioSource.clip)
                slider.value = (float)audioSource.timeSamples / audioSource.clip.samples;
        }
    }

    public void Reset()
    {
        if (slider != null)
            slider.value = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CalculateSliderValue(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        CalculateSliderValue(eventData);
    }

    private void CalculateSliderValue(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out point))
        {
            percentage = Mathf.InverseLerp(rectTransform.rect.xMin, rectTransform.rect.xMax, point.x);
            if (videoPlayer)
                videoPlayer.frame = (long)(videoPlayer.frameCount * percentage);
            else if (audioSource)
                audioSource.timeSamples = (int)(audioSource.clip.samples * percentage);
        }
    }
}