using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.AStar;
using MyGame.Map;
using MyGame.Transition;
using UnityEngine.SceneManagement;

namespace MyGame.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class NpcMovement : MonoBehaviour
    {
        // 组件
        private Rigidbody2D _rb;
        private BoxCollider2D _coll;
        private SpriteRenderer[] _srs;
        private Animator _anim;
        private Grid _grid;

        // 日程相关
        [SerializeField] private TimeSystemSettings_SO timeSettings;
        [SerializeField] private NpcScheduleDataList_SO scheduleData;
        private SortedSet<NpcScheduleDetails> _scheduleSet;
        private NpcScheduleDetails _currentSchedule;
        private readonly Stack<MovementStep> _movementSteps = new();

        // 寻路相关
        private Vector3Int _currentGridPos;
        private Vector3Int _targetGridPos;
        private Vector3Int _nextGridPos;
        private string _currentScene;
        private string _targetScene;
        [field: SerializeField] public SceneName StartScene { get; set; }

        [Header("移动相关")] [SerializeField] private float normalSpeed = 4f;
        [SerializeField] private float minSpeed = 3f;
        [SerializeField] private float maxSpeed = 8f;
        private Vector2 _direction;

        private bool _isNpcMoving;
        private bool _isOnMovementRoutine;

        // 初始化相关
        private bool _isInitialized;
        private bool _isSceneLoading;

        // 动画相关
        [SerializeField] private float breakTime = 5f;
        private float _breakTimeCount = 0f;
        private bool _canPlayStopAnim;

        private AnimatorOverrideController _animOverrideController;
        [SerializeField] private AnimationClip originEventClip;
        private AnimationClip _currentScheduleStopClip;

        private readonly int _animBoolIsMoving = Animator.StringToHash("isMoving");
        private readonly int _animFloatDirX = Animator.StringToHash("dirX");
        private readonly int _animFloatDirY = Animator.StringToHash("dirY");
        private readonly int _animBoolIsOnEvent = Animator.StringToHash("isOnEvent");


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _coll = GetComponent<BoxCollider2D>();
            _srs = GetComponentsInChildren<SpriteRenderer>();
            _anim = GetComponentInChildren<Animator>();
            _animOverrideController = new AnimatorOverrideController(_anim.runtimeAnimatorController);
            _anim.runtimeAnimatorController = _animOverrideController;

            _scheduleSet = new SortedSet<NpcScheduleDetails>(scheduleData.ScheduleList);
        }

        private void Start()
        {
            _currentScene = EnumUtils.MapToString(StartScene);
        }

        private void Update()
        {
            CheckNpcState();
            SwitchAnimation();
        }

        private void FixedUpdate()
        {
            if (_isSceneLoading) return;
            Move();
        }

        private void OnEnable()
        {
            EventBus.OnSceneUnloading += OnSceneUnLoading;
            EventBus.OnSceneChanged += OnSceneChanged;
            EventBus.OnTimeChanged += OnTimeChanged;
        }

        private void OnDisable()
        {
            EventBus.OnSceneUnloading -= OnSceneUnLoading;
            EventBus.OnSceneChanged -= OnSceneChanged;
            EventBus.OnTimeChanged -= OnTimeChanged;
        }

        private void InitNpc()
        {
            _targetScene = _currentScene;

            // 保持在当前网格坐标的中心点
            _currentGridPos = _grid.WorldToCell(transform.position);
            _targetGridPos = _currentGridPos;
            transform.position = new Vector3(_currentGridPos.x + 0.5f, _currentGridPos.y + 0.5f, 0);

            _isInitialized = true;
        }

        /// <summary>
        /// 根据Schedule构建路径
        /// </summary>
        public void BuildPath(NpcScheduleDetails schedule)
        {
            _movementSteps.Clear();
            _currentSchedule = schedule;
            _currentScheduleStopClip = schedule.animAtStop;

            var targetScene = EnumUtils.MapToString(schedule.targetScene);
            //Debug.Log($"CurrentScene:{_currentScene}, targetScene:{targetScene}");
            if (targetScene == _currentScene)
            {
                _targetGridPos = new Vector3Int(schedule.targetGridPosition.x, schedule.targetGridPosition.y, 0);
                //Debug.Log("正在构建NPC路径");
                AStar.AStar.Instance.BuildPath(targetScene, (Vector2Int)_currentGridPos,
                    schedule.targetGridPosition,
                    _movementSteps);
                //Debug.Log($"MovementSteps: {_movementSteps.Count}");
            }
            // 跨场景移动
            else
            {
                var sceneRoute = NpcManager.Instance.GetSceneRoute(_currentScene, targetScene);
                if (sceneRoute != null)
                {
                    foreach (var path in sceneRoute.paths)
                    {
                        Vector2Int fromGridPos, toGridPos;
                        if (path.fromGridCoordinates.sqrMagnitude >= 999999)
                        {
                            fromGridPos = (Vector2Int)_currentGridPos;
                        }
                        else
                        {
                            fromGridPos = path.fromGridCoordinates;
                        }

                        if (path.toGridCoordinates.sqrMagnitude >= 999999)
                        {
                            toGridPos = schedule.targetGridPosition;
                        }
                        else
                        {
                            toGridPos = path.toGridCoordinates;
                        }
                        
                        //Debug.Log($"from:{fromGridPos}, to:{toGridPos}");
                        AStar.AStar.Instance.BuildPath(EnumUtils.MapToString(path.sceneName), fromGridPos, toGridPos,_movementSteps);
                        //Debug.Log($"movementSteps: {_movementSteps.Count}");
                    }
                }
            }
            
            if (_movementSteps.Count > 1)
            {
                // 更新每一步对应的时间戳
                UpdateTimeOnPath();
            }
        }

        /// <summary>
        /// 主要移动方法
        /// </summary>
        private void Move()
        {
            if (_isOnMovementRoutine) return;
            // 当移动进行时
            if (_movementSteps.Count > 0)
            {
                _isNpcMoving = true;
                MovementStep step = _movementSteps.Pop();
                //Debug.Log($"Hour:{step.hour}, Minute:{step.minute}, Second:{step.second}");
                _currentScene = step.sceneName;

                CheckVisibility();

                _nextGridPos = (Vector3Int)step.gridCoordinate;
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(_nextGridPos, stepTime);
            }
            else
            {
                _isNpcMoving = false;
            }
        }

        private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
        {
            StartCoroutine(MoveRoutine(gridPos, stepTime));
        }

        private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
        {
            //Debug.Log("开始移动协程");
            _isOnMovementRoutine = true;
            Vector3 nextWorldPos = GetWorldPosition(gridPos);

            if (stepTime > TimeManager.Instance.GameTime)
            {
                // 用来移动的时间，以秒为单位
                float timeToMove = (float)(stepTime.TotalSeconds - TimeManager.Instance.GameTime.TotalSeconds);
                // 实际移动距离
                float distance = Vector3.Distance(transform.position, nextWorldPos);
                // 实际移动速度
                float speed = Mathf.Max(minSpeed, distance / timeToMove / timeSettings.secondThreshold);

                if (speed <= maxSpeed)
                {
                    while (Vector3.Distance(transform.position, nextWorldPos) > 0.1f)
                    {
                        //Debug.Log("开始移动");
                        // 方向
                        _direction = (nextWorldPos - transform.position).normalized;
                        // 位移
                        var posOffset = new Vector2(_direction.x, _direction.y) * (speed * Time.fixedDeltaTime);

                        _rb.MovePosition(_rb.position + posOffset);
                        //等待下次更新
                        yield return new WaitForFixedUpdate();
                    }
                }
            }

            // 时间不够位移，瞬移到目标位置
            _rb.position = nextWorldPos;
            _currentGridPos = gridPos;
            _nextGridPos = _currentGridPos;

            _isOnMovementRoutine = false;
        }

        /// <summary>
        /// 根据移动状态切换动画
        /// </summary>
        private void SwitchAnimation()
        {
            _anim.SetBool(_animBoolIsMoving, _isNpcMoving);
            if (_isNpcMoving)
            {
                _anim.SetFloat(_animFloatDirX, _direction.x);
                _anim.SetFloat(_animFloatDirY, _direction.y);
            }
            else if (!_isNpcMoving && _canPlayStopAnim)
            {
                StartCoroutine(SetStopAnimationRoutine());
                _canPlayStopAnim = false;
            }
        }

        private IEnumerator SetStopAnimationRoutine()
        {
            // 强制面向镜头
            _anim.SetFloat(_animFloatDirX, 0);
            _anim.SetFloat(_animFloatDirY, -1);

            if (_currentScheduleStopClip)
            {
                _animOverrideController[originEventClip] = _currentScheduleStopClip;

                _anim.SetBool(_animBoolIsOnEvent, true);
                yield return null;
                _anim.SetBool(_animBoolIsOnEvent, false);

                _animOverrideController[_currentScheduleStopClip] = originEventClip;
            }
        }

        private void CheckNpcState()
        {
            // 判断canPlayStopAnim状态
            if (!_isNpcMoving)
            {
                if (!(_breakTimeCount > 0)) return;
                _breakTimeCount -= Time.deltaTime;
                if (_breakTimeCount <= 0)
                {
                    _canPlayStopAnim = true;
                }
            }
            else
            {
                if (_breakTimeCount <= 0)
                {
                    _canPlayStopAnim = false;
                    _breakTimeCount = breakTime;
                }
            }
        }

        /// <summary>
        /// 更新路径时间戳
        /// </summary>
        private void UpdateTimeOnPath()
        {
            MovementStep previousStep = null;
            TimeSpan currentGameTime = TimeManager.Instance.GameTime;

            foreach (var step in _movementSteps)
            {
                previousStep ??= step;

                step.hour = currentGameTime.Hours;
                step.minute = currentGameTime.Minutes;
                step.second = currentGameTime.Seconds;

                TimeSpan gridMovementStepTime;
                // 走过每1个格子所需要的时间长度：距离（格子大小）/速度/时间阈值
                // 这里的"1"是写死的GridSize，后续可以修改放入GridSettings便于调整
                if (!IsMoveInDiagonal(step, previousStep))
                {
                    gridMovementStepTime = new TimeSpan(0, 0, (int)(1 / normalSpeed / timeSettings.secondThreshold));
                }
                else
                {
                    gridMovementStepTime = new TimeSpan(0, 0, (int)(1.41 / normalSpeed / timeSettings.secondThreshold));
                }

                //Debug.Log($"gridMovementStepTime:{gridMovementStepTime.TotalSeconds}");

                // 累加 获得下一步的时间戳
                currentGameTime = currentGameTime.Add(gridMovementStepTime);
                //Debug.Log(
                //$"currrentGameTime:Hour:{currentGameTime.Hours},Minute:{currentGameTime.Minutes},Second:{currentGameTime.Seconds}");
                // 循环赋值下一步
                previousStep = step;
            }
            //Debug.Log("时间戳更新完毕");
        }

        /// <summary>
        /// 判断两格之间路径是否为斜线
        /// </summary>
        /// <param name="currentStep">当前MovementStep</param>
        /// <param name="previousStep">上一个MovementStep</param>
        /// <returns></returns>
        private bool IsMoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
        {
            return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) &&
                   (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
        }

        private Vector3 GetWorldPosition(Vector3Int gridPos)
        {
            Vector3 worldPos = _grid.CellToWorld(gridPos);
            return new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, 0);
        }

        #region 事件订阅

        private void OnSceneUnLoading()
        {
            _isSceneLoading = true;
        }

        private void OnSceneChanged()
        {
            _isSceneLoading = false;

            CheckVisibility();
            _grid = GameObject.FindWithTag("TileMap").GetComponent<Grid>();

            if (!_isInitialized) InitNpc();
        }

        private void OnTimeChanged(object sender, TimeArgs e)
        {
            // 由NpcScheduleDetails内部定义
            int time = e.CurrentHour * 100 + e.CurrentMinute;
            //Debug.Log($"Day:{e.CurrentDay}, Time: {time}");

            NpcScheduleDetails matchedSchedule = null;

            foreach (var schedule in _scheduleSet)
            {
                if (e.CurrentDay == schedule.day && time == schedule.Time)
                {
                    matchedSchedule = schedule;
                    break;
                }

                if (e.CurrentDay < schedule.day)
                {
                    break;
                }
            }

            if (matchedSchedule != null) BuildPath(matchedSchedule);
        }

        #endregion

        #region NPC显示相关

        private void CheckVisibility()
        {
            if (_currentScene == SceneTransitionManager.Instance.CurrentSceneName) 
                SetActiveInScene();
            else 
                SetInactiveInScene();
        }

        private void SetActiveInScene()
        {
            foreach (var sr in _srs)
            {
                sr.enabled = true;
            }

            _coll.enabled = true;
        }

        private void SetInactiveInScene()
        {
            foreach (var sr in _srs)
            {
                sr.enabled = false;
            }

            _coll.enabled = false;
        }

        #endregion
    }
}