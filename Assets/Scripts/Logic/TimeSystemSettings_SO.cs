using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeSystemSettings", menuName = "Settings/TimeSystemSettings")]
public class TimeSystemSettings_SO : ScriptableObject
{
    public float secondThreshold = 0.12f;//数值越小时间越快
    public int secondHold = 59;
    public int minuteHold = 59;
    public int hourHold = 23;
    public int dayHold = 30;
    public int monthHold = 12;
    public int seasonHold = 3;
}