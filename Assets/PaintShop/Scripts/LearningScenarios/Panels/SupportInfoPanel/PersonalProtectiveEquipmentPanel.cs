using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that shows information about the personal protective equipment.
/// </summary>
public class PersonalProtectiveEquipmentPanel : MonoBehaviour
{
    public GameObject singleSlidesContainer;
    public GameObject lastSlideContainer;
    public Slider progressBar;
    public Image icon;
    public Sprite[] symbols;
    public Button prevPageButton;
    public Button nextPageButton;
    public AudioClip[] audioClips;

    private int _count;
    private CanvasGroup _lastSlideCanvasGroup;
    private CanvasGroup _iconCanvasGroup;
    private Coroutine _currentCoroutine;

    private const float _fadeTime = 0.5f;
    private const int _slideCount = 4;

    void Update()
    {
        if (VirtualInstructorController.Instance.audioSource.isPlaying && _count < 4)
            progressBar.value = VirtualInstructorController.Instance.audioSource.time / audioClips[_count].length;
    }

    private void OnEnable()
    {
        _lastSlideCanvasGroup = lastSlideContainer.GetComponent<CanvasGroup>();
        _iconCanvasGroup = icon.GetComponent<CanvasGroup>();
        _iconCanvasGroup.alpha = 0;
        _count = 0;
        UpdateButtons();
        _currentCoroutine = StartCoroutine(PlaySlide());
    }

    private void OnDisable()
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
    }

    /// <summary>
    /// Shows the next slide and plays the corresponding audio.
    /// </summary>
    private IEnumerator PlaySlide()
    {
        singleSlidesContainer.SetActive(true);
        lastSlideContainer.SetActive(false);
        icon.sprite = symbols[_count];
        progressBar.value = 0;
        VirtualInstructorController.Instance.Speak(audioClips[_count]);
        StartCoroutine(Lerp.Alpha(_iconCanvasGroup, 1, _fadeTime));
        yield return new WaitForSeconds(audioClips[_count].length - _fadeTime);
        StartCoroutine(Lerp.Alpha(_iconCanvasGroup, 0, _fadeTime));
        yield return new WaitForSeconds(_fadeTime);
        _count++;
        PlayNextSlide();
    }

    private void StopCoroutine()
    {
        StopCoroutine(_currentCoroutine);
        VirtualInstructorController.Instance.Stop();
        StartCoroutine(Lerp.Alpha(singleSlidesContainer.activeSelf ? _iconCanvasGroup : _lastSlideCanvasGroup, 0,
            _fadeTime, PlayNextSlide));
    }

    private void PlayNextSlide()
    {
        if (_count < _slideCount)
            _currentCoroutine = StartCoroutine(PlaySlide());
        else
            PlayLastSlide();
        UpdateButtons();
    }

    private void PlayLastSlide()
    {
        singleSlidesContainer.SetActive(false);
        lastSlideContainer.SetActive(true);
        _lastSlideCanvasGroup.alpha = 0;
        StartCoroutine(Lerp.Alpha(_lastSlideCanvasGroup, 1, _fadeTime));
    }

    public void PreviousSlide()
    {
        if (_count > 0 && _count < _slideCount)
            _count--;
        else
            _count = _slideCount - 1;

        StopCoroutine();
    }

    public void NextSlide()
    {
        if (_count >= 0 && _count < _slideCount)
            _count++;

        StopCoroutine();
    }

    public void Return()
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
    }


    private void UpdateButtons()
    {
        nextPageButton.interactable = _count != _slideCount;
        prevPageButton.interactable = _count != 0;
    }
}