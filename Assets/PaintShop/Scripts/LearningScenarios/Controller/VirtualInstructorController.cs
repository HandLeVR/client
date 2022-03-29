using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using QuickOutline;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Controller for the virtual instructor controlling his position and what he says.
/// </summary>
public class VirtualInstructorController : Singleton<VirtualInstructorController>
{
    public GameObject speechBubble;
    [Tooltip("The default position of the instructor.")]
    public Transform introductionPosition;
    [Tooltip("The position of the instructor during a painting task.")]
    public Transform paintPosition;

    [HideInInspector] public UnityEvent speechStarted;
    [HideInInspector] public UnityEvent speechComplete;
    [HideInInspector] public bool active;
    [HideInInspector] public InstructorPosition currentPosition;
    [HideInInspector] public AudioSource audioSource;

    private Animator _animator;
    private List<Outline> _outlines;
    private bool _highlighted;
    private Coroutine _currenCoroutine;
    private TextMeshProUGUI _speechBubbleText;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _speechBubbleText = speechBubble.GetComponentInChildren<TextMeshProUGUI>();
        _outlines = GetComponentsInChildren<Outline>().ToList();
    }

    void FixedUpdate()
    {
        // needs to be done in FixedUpdate to work together with the update of the spray gun
        if (_highlighted)
        {
            _outlines.ForEach(outline => outline.enabled = true);
            _highlighted = false;
        }
        else
        {
            _outlines.ForEach(outline => outline.enabled = false);
        }
    }

    /// <summary>
    /// Initializes the virtual instructor.
    /// </summary>
    public void Init(AudioClip audioClip, InstructorPosition position = InstructorPosition.Introduction,
        string speechBubbleText = null, UnityAction afterSpeech = null)
    {
        Stop();
        audioSource.time = 0;
        audioSource.clip = audioClip;

        Transform pos = position == InstructorPosition.Introduction ? introductionPosition : paintPosition;
        transform.parent.position = pos.position;
        transform.parent.rotation = pos.rotation;

        speechComplete.RemoveAllListeners();
        if (afterSpeech != null)
            speechComplete.AddListener(afterSpeech);

        speechBubble.gameObject.SetActive(!String.IsNullOrEmpty(speechBubbleText));
        if (!String.IsNullOrEmpty(speechBubbleText))
            _speechBubbleText.text = speechBubbleText;

        gameObject.SetActive(true);
        currentPosition = position;
    }

    public void SpeakOrStop()
    {
        speechBubble.gameObject.SetActive(false);
        if (audioSource.isPlaying)
            Stop();
        else
            Speak();
    }

    public void Speak()
    {
        Stop();
        _currenCoroutine = StartCoroutine(SpeakCoroutine());
        speechStarted.Invoke();
    }

    /// <summary>
    /// Plays the current audio file and invokes speechComplete after finishing the audio file.
    /// </summary>
    private IEnumerator SpeakCoroutine()
    {
        speechStarted.Invoke();
        _animator.Play("smiling_end", 1);
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        speechComplete.Invoke();
    }

    public void Play(float time)
    {
        audioSource.Stop();
        if (time > audioSource.clip.length)
            return;
        _animator.Play("smiling_end", 1);
        audioSource.time = time;
        audioSource.Play();
    }

    public void Pause()
    {
        audioSource.Stop();
    }

    public void Speak(AudioClip audioClip, UnityAction afterSpeech = null)
    {
        Init(audioClip, afterSpeech: afterSpeech);
        Speak();
    }

    public void Highlight()
    {
        _highlighted = true;
    }

    public void Stop()
    {
        if (_currenCoroutine != null)
        {
            audioSource.Stop();
            StopCoroutine(_currenCoroutine);
            if (_animator.gameObject.activeSelf)
                _animator.Play("smiling_begin", 1);
        }
    }

    /// <summary>
    /// The position of the instructor. Introduction means on the right of the monitor and paint means on the left of
    /// the monitor.
    /// </summary>
    public enum InstructorPosition
    {
        Introduction,
        Paint
    }
}
