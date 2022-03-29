using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a calender where days can be selected as deadline for submission for task assignments.
/// </summary>
public class Calendar : MonoBehaviour
{
    public TextMeshProUGUI monthLabel;
    public Button previousMonthBtn;
    public Button nextMonthBtn;
    public Transform daysContainer;
    public GameObject dayPrefab;
    public TMP_Dropdown hoursDropdown;
    public TMP_Dropdown minutesDropdown;
    public TMP_InputField currentDateInputField;
    
    private DateTime _dateTime;
    private string _tooltipHoliday;
    private string _tooltipDeadline;

    void Awake()
    {
        hoursDropdown.options.Clear();
        minutesDropdown.options.Clear();
        for (int i = 0; i < 24; i++)
            hoursDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString("D2")));
        for (int i = 0; i < 60; i++)
            minutesDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString("D2")));
        hoursDropdown.value = 0;
        minutesDropdown.value = 0;
        hoursDropdown.onValueChanged.AddListener(delegate { SetHour(); });
        minutesDropdown.onValueChanged.AddListener(delegate { SetMinute(); });
        previousMonthBtn.onClick.AddListener(ShowPreviousMonth);
        nextMonthBtn.onClick.AddListener(ShowNextMonth);
    }

    /// <summary>
    /// Initializes the days of the given month.
    /// </summary>
    public void Init(DateTime dt)
    {
        daysContainer.DestroyAllChildren();
        _dateTime = dt;
        monthLabel.text = DeadlinePicker.Instance.cultureInfoGer.DateTimeFormat.GetMonthName(dt.Month) + " " + dt.Year;
        CreateDayElements();
        hoursDropdown.value = hoursDropdown.options.FindIndex(i => i.text.Equals(dt.Hour.ToString("D2")));
        minutesDropdown.value = minutesDropdown.options.FindIndex(i => i.text.Equals(dt.Minute.ToString("D2")));
        DisplayCurrentDate();
    }

    /// <summary>
    /// Displays the days for the current month.
    /// </summary>
    private void CreateDayElements()
    {
        int startIndex = 0;
        // looking for the child index of the first day that is not disabled (e.g. all days before 1st/after 31th August should be disabled.) 
        DayOfWeek dayOfWeek = new DateTime(_dateTime.Year, _dateTime.Month, 1).DayOfWeek;
        startIndex = (int)dayOfWeek > 0 ? (int)dayOfWeek - 1 : 6;
        int endIndex = DateTime.DaysInMonth(_dateTime.Year, _dateTime.Month) - 1 + startIndex;

        for (int i = 0; i < 42; i++)
        {
            GameObject dayContainer = Instantiate(dayPrefab, daysContainer);
            if (i < startIndex || i > endIndex)
            {
                dayContainer.transform.name = "emptyday_" + (i + 1);
                dayContainer.GetComponent<DayContainer>().DisableDay();
                dayContainer.GetComponent<TooltipObject>().tooltipText = i < startIndex
                    ? "Bitte in den vorherigen Monat wechseln."
                    : i > endIndex
                        ? "Bitte in den nächsten Monat wechseln."
                        : "";
            }
            else
            {
                int day = i + 1 - startIndex;
                dayContainer.transform.name = "day_" + day;
                dayContainer.GetComponent<DayContainer>().Init(day, isToday(day), IsPublicHoliday(_dateTime.Month, day),
                    _tooltipHoliday);
                dayContainer.GetComponent<Button>().onClick.AddListener(delegate { SetDay(day); });
            }
        }
    }

    /// <summary>
    /// Checks whether the given day is a holiday.
    /// </summary>
    private bool IsPublicHoliday(int m, int d)
    {
        if (m == 1 && d == 1)
            _tooltipHoliday = "Neujahr";
        else if (m == 5 && d == 1)
            _tooltipHoliday = "Tag der Arbeit";
        else if (m == 10 && d == 3)
            _tooltipHoliday = "Tag der deutschen Einheit";
        else if (m == 12 && d == 25)
            _tooltipHoliday = "1. Weihnachtsfeiertag";
        else if (m == 12 && d == 26)
            _tooltipHoliday = "2. Weihnachtsfeiertag";
        else
        {
            _tooltipHoliday = "";
            return false;
        }

        return true;
    }

    private void ShowPreviousMonth()
    {
        _dateTime = _dateTime.AddMonths(-1);
        Init(_dateTime);
    }

    private void ShowNextMonth()
    {
        _dateTime = _dateTime.AddMonths(1);
        Init(_dateTime);
    }

    private void SetMinute()
    {
        _dateTime = new DateTime(_dateTime.Year, _dateTime.Month,
            _dateTime.Day, _dateTime.Hour, minutesDropdown.value, 0, DateTimeKind.Local);
        DisplayCurrentDate();
    }

    private void SetHour()
    {
        _dateTime = new DateTime(_dateTime.Year, _dateTime.Month,
            _dateTime.Day, hoursDropdown.value, _dateTime.Minute, 0, DateTimeKind.Local);
        DisplayCurrentDate();
    }

    private void SetDay(int d)
    {
        _dateTime = new DateTime(_dateTime.Year, _dateTime.Month,
            d, _dateTime.Hour, _dateTime.Minute, 0, DateTimeKind.Local);
        DisplayCurrentDate();
    }

    private bool isToday(int d)
    {
        return new DateTime(_dateTime.Year, _dateTime.Month, d) == DateTime.Today;
    }

    private void DisplayCurrentDate()
    {
        currentDateInputField.text =
            _dateTime.ToString(DeadlinePicker.Instance.dateformat, DeadlinePicker.Instance.cultureInfoGer);
        DeadlinePicker.Instance.currentDate = _dateTime;
    }
}