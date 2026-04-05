using System.Collections;
using System.Collections.Generic;
using MyGame.Transition;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D _rb;

    [Header("移动相关")] [SerializeField] private float speed = 5f;
    private float _inputX;
    private float _inputY;
    private Vector2 _movementInput;

    //[Header("动画相关")]
    private Animator[] _animators;
    private bool _isMoving;
    private bool _isPaused;
    private float _mouseX;
    private float _mouseY;
    private bool _isUsingTool;

    [SerializeField] private float toolUsingDuration = 0.45f;
    [SerializeField] private float toolUsingBackswing = 0.25f;

    private Vector3 _teleportTarget;


    private readonly int _animParamInputX = Animator.StringToHash("inputX");
    private readonly int _animParamInputY = Animator.StringToHash("inputY");
    private readonly int _animParamIsMoving = Animator.StringToHash("isMoving");
    private readonly int _animParamUseTool = Animator.StringToHash("useTool");
    private readonly int _animParamMouseX = Animator.StringToHash("mouseX");
    private readonly int _animParamMouseY = Animator.StringToHash("mouseY");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animators = GetComponentsInChildren<Animator>();
    }

    private void OnEnable()
    {
        EventBus.OnSceneUnloading += OnSceneUnloading;
        EventBus.OnFadedOut += OnFadedOut;
        EventBus.OnFadedIn += OnFadedIn;
        EventBus.OnValidMouseClicked += OnValidMouseClicked;
    }

    private void OnDisable()
    {
        EventBus.OnSceneUnloading -= OnSceneUnloading;
        EventBus.OnFadedOut -= OnFadedOut;
        EventBus.OnFadedIn -= OnFadedIn;
        EventBus.OnValidMouseClicked -= OnValidMouseClicked;
    }

    private void Update()
    {
        if (_isPaused || _isUsingTool)
        {
            _isMoving = false;
        }
        else
        {
            CatchPlayerInput();
            
        }

        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        if (_isPaused || _isUsingTool) return;
        ApplyMovement();
    }

    private void OnValidMouseClicked(Vector3 mousePos, ItemDetails itemDetails)
    {
        if (_isPaused || _isUsingTool) return;
        if (itemDetails.itemType switch
            {
                ItemType.Seed or ItemType.Commodity or ItemType.Furniture or ItemType.ReapableScenery => false,
                _ => true,
            })
        {
            if (_isUsingTool) return;
            _mouseX = mousePos.x - transform.position.x;
            _mouseY = mousePos.y - transform.position.y;
            if (Mathf.Abs(_mouseX) > Mathf.Abs(_mouseY))
            {
                _mouseY = 0;
            }
            else
            {
                _mouseX = 0;
            }

            StartCoroutine(UseToolRoutine(mousePos, itemDetails));
        }
        else
        {
            EventBus.RaisePlayerClickAnimFinished(mousePos, itemDetails);
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mousePos, ItemDetails itemDetails)
    {
        _isUsingTool = true;
        yield return null;
        foreach (var anim in _animators)
        {
            anim.SetTrigger(_animParamUseTool);
            // 这里修改人物朝向
            anim.SetFloat(_animParamInputX, _mouseX);
            anim.SetFloat(_animParamInputY, _mouseY);
        }

        yield return new WaitForSeconds(toolUsingDuration);
        EventBus.RaisePlayerClickAnimFinished(mousePos, itemDetails);
        yield return new WaitForSeconds(toolUsingBackswing);

        _isUsingTool = false;
    }

    private void OnSceneUnloading()
    {
        _isPaused = true;
    }

    private void OnFadedOut()
    {
        Teleport();
    }

    private void OnFadedIn()
    {
        _isPaused = false;
    }

    /// <summary>
    /// 记录玩家的输入值
    /// </summary>
    private void CatchPlayerInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _inputX *= 0.5f;
            _inputY *= 0.5f;
        }

        _movementInput = new Vector2(_inputX, _inputY);
        _movementInput = Vector2.ClampMagnitude(_movementInput, 1); //限制向量模长上限为1！

        _isMoving = _movementInput != Vector2.zero;
    }

    /// <summary>
    /// 控制玩家移动
    /// </summary>
    /// <returns></returns>
    private void ApplyMovement()
    {
        //_rb.velocity = _movementInput * speed;
        _rb.MovePosition(_rb.position + _movementInput * (speed * Time.fixedDeltaTime));
    }

    private void SwitchAnimation()
    {
        foreach (var anim in _animators)
        {
            anim.SetBool(_animParamIsMoving, _isMoving);
            // 这里修改工具朝向
            anim.SetFloat(_animParamMouseX, _mouseX);
            anim.SetFloat(_animParamMouseY, _mouseY);
            if (_isMoving)
            {
                anim.SetFloat(_animParamInputX, _inputX);
                anim.SetFloat(_animParamInputY, _inputY);
            }
        }
    }

    /// <summary>
    /// 传送到指定位置
    /// </summary>
    public void Teleport()
    {
        transform.position = _teleportTarget;
    }

    public void SetTeleportTarget(Vector3 position)
    {
        _teleportTarget = position;
    }
}