using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Container for the calendar and responsible for setting the selected date.
/// </summary>
public class DeadlinePicker : Singleton<DeadlinePicker>
{
    public Button closeButton;
    public Button chooseDateButton;
    public Calendar calendar;
    
    [HideInInspector] public TMP_InputField callingInputField;
    [HideInInspector] public DateTime currentDate;

    public DateTime? selectedDate;
    public readonly string dateformat = "dddd, dd.MM.yyyy, HH:mm";
    public CultureInfo cultureInfoGer = new CultureInfo("de-DE", false);

    private void Awake()
    {
        chooseDateButton.onClick.AddListener(ChooseDate);
        closeButton.onClick.AddListener(Close);
    }

    public void Init(TMP_InputField input)
    {
        gameObject.SetActive(true);
        callingInputField = input;
        callingInputField.text = "";
        calendar.Init(selectedDate ?? DateTime.Now);
    }

    private void Close()
    {
        selectedDate = null;
        callingInputField.text = "";
        gameObject.SetActive(false);
    }

    private void ChooseDate()
    {
        selectedDate = currentDate;
        callingInputField.text = selectedDate.GetValueOrDefault().ToString(dateformat, cultureInfoGer);
        gameObject.SetActive(false);
    }
}
