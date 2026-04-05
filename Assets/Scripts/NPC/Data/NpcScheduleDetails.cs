using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class NpcScheduleDetails : IComparable<NpcScheduleDetails>
{
    public int day, hour, minute;
    public int priority;
    // public Season season;

    public SceneName targetScene;
    public Vector2Int targetGridPosition;
    public AnimationClip animAtStop;

    public bool isInteractable;

    public NpcScheduleDetails(int priority, int minute, int hour, int day, SceneName targetScene,
        Vector2Int targetGridPosition, AnimationClip animAtStop, bool isInteractable)
    {
        this.priority = priority;
        this.minute = minute;
        this.hour = hour;
        this.day = day;
        this.targetScene = targetScene;
        this.targetGridPosition = targetGridPosition;
        this.animAtStop = animAtStop;
        this.isInteractable = isInteractable;
    }

    public int Time => (hour * 100) + minute;

    // 对比时间，进行优先级排序
    public int CompareTo(NpcScheduleDetails other)
    {
        if (day > other.day)
        {
            return 1;
        }
        else if (day < other.day)
        {
            return -1;
        }
        
        if (Time == other.Time)
        {
            return priority > other.priority ? 1 : -1;
        }
        else if (Time > other.Time)
        {
            return 1;
        }
        else if (Time < other.Time)
        {
            return -1;
        }

        return 0;
    }
}