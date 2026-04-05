using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TimeUI : MonoBehaviour
{
    [SerializeField] private RectTransform dayNightImage;
    [SerializeField] private RectTransform clockParent;
    [SerializeField] private Image seasonImage;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI timeText;

    [SerializeField] private Sprite[] seasonSprites;

    private readonly List<Image> _clockBlocks = new();

    [SerializeField] private TimeSystemSettings_SO settings;

    void Awake()
    {
        for (int i = 0; i < clockParent.childCount; i++)
        {
            if (clockParent.GetChild(i).TryGetComponent<Image>(out var image))
            {
                _clockBlocks.Add(image);
                _clockBlocks[i].DOFade(0, 0f);
            }
        }
    }

    void OnEnable()
    {
        EventBus.OnTimeChanged += OnTimeChanged;
    }

    void OnDisable()
    {
        EventBus.OnTimeChanged -= OnTimeChanged;
    }

    private void OnTimeChanged(object sender, TimeArgs e)
    {
        timeText.SetText($"{e.CurrentHour:00}时{e.CurrentMinute:00}分"); 
        
        SwitchClockImage(e.CurrentHour);
        SwitchDayNightImage(e.CurrentHour);
        
        dateText.SetText($"{e.CurrentYear}年{e.CurrentMonth:00}月{e.CurrentDay:00}日");
        seasonImage.sprite = seasonSprites[(int)e.CurrentSeason];
    }
    

    /// <summary>
    /// 根据小时显示对应数量时间块
    /// </summary>
    /// <param name="hour">当前小时</param>
    private void SwitchClockImage(int hour)
    {
        int index = (hour + 1) / 4;

        for (int i = 0; i < _clockBlocks.Count; i++)
        {
            if (i < index)
            {
                _clockBlocks[i].DOFade(1f, 1f);
            }
            else
            {
                _clockBlocks[i].DOFade(0f, 1f);
            }
        }
    }

    private void SwitchDayNightImage(int hour)
    {
        var target = new Vector3(0, 0, hour * 15 - 90);
        dayNightImage.DORotate(target, 1f);
    }
}