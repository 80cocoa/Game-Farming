using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    [SerializeField] private TimeSystemSettings_SO settings;

    private int _gameSecond, _gameMinute, _gameHour, _gameDay, _gameMonth, _gameYear;
    private Season _gameSeason;

    //private int _monthInSeason = 3;
    private bool _isClockPaused;
    private float _tikTime;

    private TimeArgs _cachedTimeArgs;

    public TimeSpan GameTime => new(_gameHour, _gameMinute, _gameSecond);

    protected override void Awake()
    {
        base.Awake();
        InitGameTime();
    }

    private void OnEnable()
    {
        EventBus.OnSceneUnloading += () => _isClockPaused = true;
        EventBus.OnSceneChanged += () => _isClockPaused = false;
    }

    private void OnDisable()
    {
        
    }

    private void Start()
    {
        _cachedTimeArgs = new TimeArgs(_gameMinute, _gameHour, _gameDay, _gameMonth, _gameYear, _gameSeason);

        EventBus.RaiseTimeChanged(gameObject, _cachedTimeArgs);
    }

    private void Update()
    {
        if (!_isClockPaused)
        {
            _tikTime += Time.deltaTime;

            if (_tikTime >= settings.secondThreshold)
            {
                _tikTime -= settings.secondThreshold;
                UpdateGameTime();
            }
        }

        // 作弊器
        // 快进10min
        if (Input.GetKeyDown(KeyCode.T))
        {
            for (int i = 0; i < 600; i++)
            {
                UpdateGameTime();
            }
        }

        // 快进1day
        if (Input.GetKeyDown(KeyCode.G))
        {
            for (int i = 0; i < 1; i++)
            {
                _gameDay++;
                _cachedTimeArgs.UpdateArgs(_gameMinute, _gameHour, _gameDay, _gameMonth, _gameYear, _gameSeason);
                EventBus.RaiseTimeChanged(gameObject, _cachedTimeArgs);
            }
        }
    }

    private void InitGameTime()
    {
        _gameSecond = 0;
        _gameMinute = 55;
        _gameHour = 7;
        _gameDay = 1;
        _gameMonth = 1;
        _gameYear = 1;
        _gameSeason = Season.Spring;
    }

    /// <summary>
    /// 更新游戏时间
    /// </summary>
    private void UpdateGameTime()
    {
        _gameSecond++;
        if (_gameSecond > settings.secondHold)
        {
            _gameMinute++;
            _gameSecond = 0;

            if (_gameMinute > settings.minuteHold)
            {
                _gameHour++;
                _gameMinute = 0;

                if (_gameHour > settings.hourHold)
                {
                    _gameDay++;

                    _gameHour = 0;

                    if (_gameDay > settings.dayHold)
                    {
                        _gameMonth++;
                        _gameDay = 1;

                        if (_gameMonth > settings.monthHold)
                        {
                            _gameYear++;
                            _gameMonth = 1;

                            _gameSeason = _gameMonth switch
                            {
                                12 or 1 or 2 => Season.Winter,
                                3 or 4 or 5 => Season.Spring,
                                6 or 7 or 8 => Season.Summer,
                                9 or 10 or 11 => Season.Autumn,
                                _ => Season.Spring,
                            };
                        }
                    }
                }
            }
            _cachedTimeArgs.UpdateArgs(_gameMinute, _gameHour, _gameDay, _gameMonth, _gameYear, _gameSeason);
            EventBus.RaiseTimeChanged(gameObject, _cachedTimeArgs);
        }
    }
}