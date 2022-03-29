using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a choice item in the selection panel.
/// </summary>
public class ChoiceItem : MonoBehaviour
{
    public Image circle;
    public TextMeshProUGUI text;
    public Image background;

    [HideInInspector] public Rigidbody circleRigidbody;
    [HideInInspector] public bool isCorrect;
    [HideInInspector] public UnityAction onSelection;

    public bool HasBall
    {
        get => _hasBall;
        set
        {
            StartCoroutine(WaitFor.Seconds(0.5f, () => onSelection?.Invoke()));
            _hasBall = value;
        }
    }
    private bool _hasBall;

    private void Awake()
    {
        circleRigidbody = GetComponentInChildren<Rigidbody>();
    }
}
