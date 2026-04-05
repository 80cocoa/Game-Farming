using System;
using UnityEngine;
using UnityEngine.PlayerLoop;


public class TimeArgs : EventArgs
{
    public int CurrentMinute { get; set; }
    public int CurrentHour { get; set; }
    public int CurrentDay { get; set; }
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
    public Season CurrentSeason { get; set; }

    public TimeArgs(int currentMinute, int currentHour, int currentDay, int currentMonth, int currentYear,
        Season currentSeason)
    {
        CurrentMinute = currentMinute;
        CurrentHour = currentHour;
        CurrentDay = currentDay;
        CurrentMonth = currentMonth;
        CurrentYear = currentYear;
        CurrentSeason = currentSeason;
    }

    public void UpdateArgs(int currentMinute, int currentHour, int currentDay, int currentMonth, int currentYear,
        Season currentSeason)
    {
        CurrentMinute = currentMinute;
        CurrentHour = currentHour;
        CurrentDay = currentDay;
        CurrentMonth = currentMonth;
        CurrentYear = currentYear;
        CurrentSeason = currentSeason;
    }
}