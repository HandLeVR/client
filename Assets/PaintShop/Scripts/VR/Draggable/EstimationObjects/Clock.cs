using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A clock that can be used to estimate a time span.
/// </summary>
public class Clock : EstimationObject
{
    public TextMeshProUGUI counter;
    public Image radialDisplay;
    public ClockHand minuteHand;
    public Transform hourHand;
    public CanvasGroup canvas;
    public Color handsColor;

    [HideInInspector] public int minimumValue;
    [HideInInspector] public int maximumValue;

    private List<Renderer> _renderers;
    private List<Renderer> _handsRenderer;
    private float _localRotation;
    private int _displayValue;
    private bool _showSolution;
    private int _rounds;
    private float _oldLocalRotation;
    private float _lerpValue;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>().ToList();
        _handsRenderer = new List<Renderer> { minuteHand.GetComponent<Renderer>(), hourHand.GetComponent<Renderer>() };
    }

    void OnEnable()
    {
        radialDisplay.gameObject.SetActive(false);
        _showSolution = false;
        _oldLocalRotation = Int32.MaxValue;
        _rounds = 0;
    }

    private void OnDisable()
    {
        _handsRenderer.ForEach(r => r.material.color = handsColor);
    }

    void Update()
    {
        // get local rotation from the lerp value if the solution is visible to animate the clock hands
        if (_showSolution)
            _localRotation = _lerpValue % 60 * (360 / 60f);
        // otherwise get local rotation from the minute hand
        else
            _localRotation = minuteHand.transform.localRotation.eulerAngles.y;

        // decrease tracked rounds if the minute hand is moved over the 12 backwards
        if (_localRotation >= 270 && _localRotation <= 360 &&
            _oldLocalRotation >= 0 && _oldLocalRotation <= 90 && _rounds >= 0)
            _rounds--;
        // increase tracked rounds if the minute hand is moved over the 12 forwards
        else if (_oldLocalRotation >= 270 && _oldLocalRotation <= 360 &&
                 _localRotation >= 0 && _localRotation <= 90 && _rounds <= 11)
            _rounds++;

        // set the rotation of the hour hand in dependence of the rounds and the rotation of the minute hand
        if (_rounds >= 0 && _rounds <= 12)
            hourHand.localEulerAngles = new Vector3(0, 30 * _rounds + 30 * _localRotation / 360, 0);
        // rotate the minute hand in dependence of the lerp value if the solution is visible
        if (_showSolution)
            minuteHand.transform.localEulerAngles = new Vector3(0, _localRotation, 0);

        // set the display value
        _displayValue = Convert.ToInt32(Mathf.Clamp(GetMinutes(), 0, 12 * 60));
        counter.text = _displayValue.ToString();

        // save ration for the next frame to track whether the minute hand moves over the 12
        _oldLocalRotation = _localRotation;
    }

    /// <summary>
    /// Resets the clock.
    /// </summary>
    public override void Reset()
    {
        minuteHand.isActive = true;
        radialDisplay.gameObject.SetActive(false);
        Quaternion angles = Quaternion.Euler(0, 1f, 0);
        minuteHand.transform.localRotation = angles;
    }

    /// <summary>
    /// Locks the clock when continue coin was selected and displays correct range.
    /// </summary>
    /// <returns>result of the task</returns>
    public override bool ShowSolution()
    {
        minuteHand.isActive = false;
        bool correct = _displayValue > minimumValue && _displayValue < maximumValue;
        DisplayMinMax();
        _showSolution = true;
        _lerpValue = GetMinutes();

        if (!correct)
        {
            _handsRenderer.ForEach(r =>
                StartCoroutine(Lerp.Color(r, r.material.color, new Color(1, 0, 0, 0.75f), 0.5f)));
            StartCoroutine(WaitFor.Seconds(0.75f,
                () => StartCoroutine(Lerp.Float(t => _lerpValue = t, _lerpValue,
                    (maximumValue - minimumValue) / 2f + minimumValue, 3))));
            StartCoroutine(WaitFor.Seconds(2.5f,
                () => StartCoroutine(Lerp.Color(_handsRenderer[0], _handsRenderer[0].material.color,
                    new Color(0, 1, 0, 0.75f), 1))));
            StartCoroutine(WaitFor.Seconds(2.5f,
                () => StartCoroutine(Lerp.Color(_handsRenderer[1], _handsRenderer[1].material.color,
                    new Color(0, 1, 0, 0.75f), 1))));
        }
        else
            _handsRenderer.ForEach(r =>
                StartCoroutine(Lerp.Color(r, r.material.color, new Color(0, 1, 0, 0.75f), 0.5f)));


        return correct;
    }

    /// <summary>
    /// Calculates the current minutes by considering the rounds (hours) an the local rotation of the minute hand.
    /// </summary>
    private float GetMinutes()
    {
        return _localRotation / 360 * 60 + _rounds * 60;
    }

    /// <summary>
    /// Toggles the display of the green overlay that shows min and max values.
    /// </summary>
    void DisplayMinMax()
    {
        radialDisplay.gameObject.SetActive(true);
        CanvasGroup canvasGroup = radialDisplay.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        StartCoroutine(Lerp.Alpha(canvasGroup, 0.5f, 0.5f));
        radialDisplay.fillAmount = ((float)maximumValue - minimumValue) / 60;

        float turnAmount = minimumValue * 6;
        Quaternion angles = Quaternion.Euler(radialDisplay.transform.localRotation.x,
            radialDisplay.transform.localRotation.y, turnAmount);
        radialDisplay.transform.localRotation = angles;
    }

    public override void FadeIn()
    {
        canvas.alpha = 0;
        StartCoroutine(Lerp.Alpha(canvas, 1, 0.5f));
        _renderers.ForEach(r => r.FadeMaterialsToOriginalAlpha(0.5f));
    }

    public override void FadeOut()
    {
        StartCoroutine(Lerp.Alpha(canvas, 0, 0.5f));
        _renderers.ToList().FadeOutMaterialsAndSetOriginalAlpha(0.5f, () => gameObject.SetActive(false));
    }
}