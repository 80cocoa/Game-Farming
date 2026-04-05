using System.Collections;
using System.Collections.Generic;
using MyGame.Map;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] private Sprite normal, tool, seed, commodity;

    private Sprite _currentSprite;
    private Image _cursorImage;
    private RectTransform _cursorCanvas;

    private ItemDetails _currentItemDetails;

    private bool _isMouseOverUI;

    // 鼠标检测
    private Camera _mainCamera;
    private Grid _currentGrid;
    private Transform _playerTransform;

    private Vector3 _mouseWorldPosition;
    public Vector3 MouseWorldPosition => _mouseWorldPosition;
    public Vector3Int MouseGridPosition { get; private set; }

    private Vector3 _lastMousePosition;
    private bool _isCursorPositionValid;

    private bool _isMouseMoving;
    private bool _isMouseNormal;

    private void OnEnable()
    {
        EventBus.OnItemSelected += OnItemSelected;
        EventBus.OnSceneChanged += OnSceneChanged;
        EventBus.OnItemRemoved += OnItemRemoved;
    }

    private void OnDisable()
    {
        EventBus.OnItemSelected -= OnItemSelected;
        EventBus.OnSceneChanged -= OnSceneChanged;
        EventBus.OnItemRemoved -= OnItemRemoved;
    }

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        GameObject.FindWithTag("CursorCanvas")?.TryGetComponent(out _cursorCanvas);
        _cursorCanvas?.GetChild(0)?.TryGetComponent(out _cursorImage);

        _currentSprite = normal;
    }

    private void Update()
    {
        if (!_cursorCanvas) return;
        
        var mPos = Input.mousePosition;
        if (float.IsInfinity(mPos.x) || float.IsInfinity(mPos.y)) return;
        
        // 鼠标图样跟随指针移动
        _cursorImage.transform.position = mPos;

        // 判断指针是否在UI上
        bool newValue = IsMouseOverUI();
        if (_isMouseOverUI != newValue)
        {
            _isMouseOverUI = newValue;
            SetCursorImage(_isMouseOverUI ? normal : _currentSprite);
            if (_isMouseOverUI) SetCursorValid();
        }

        if (!_isMouseOverUI)
        {
            CheckMouseValidation();
            CheckPlayerInput();
        }
        
        // 获取鼠标世界坐标
        _mouseWorldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y, -_mainCamera.transform.position.z));

        // 检测鼠标是否移动
        _isMouseMoving = IsMouseMoving();
        // 检测鼠标是否处于正常模式
        _isMouseNormal = _currentSprite == normal;
    }

    private void LateUpdate()
    {
        _lastMousePosition = Input.mousePosition;
    }

    private void OnItemRemoved(int remainingAmount)
    {
        if (remainingAmount == 0)
        {
            _currentItemDetails = null;
            _currentSprite = normal;
            SetCursorImage(_currentSprite);
        }
    }

    private void OnSceneChanged()
    {
        _currentGrid = GameObject.FindWithTag("TileMap").GetComponent<Grid>();
    }

    /// <summary>
    /// 当物品被选中时，更新currentSprite
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelected(ItemDetails itemDetails, bool isSelected)
    {
        if (isSelected)
        {
            //Debug.Log($"{itemDetails.itemName} is selected");
            // 设置鼠标样式
            _currentSprite = itemDetails.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => commodity,
                ItemType.ChopTool => tool,
                ItemType.HoeTool => tool,
                ItemType.WaterTool => tool,
                ItemType.BreakTool => tool,
                ItemType.CollectTool => tool,
                _ => normal,
            };

            // 获取选中物品的信息
            _currentItemDetails = itemDetails;
        }
        else
        {
            //Debug.Log($"{itemDetails.itemName} is not selected");
            _currentSprite = normal;
            _currentItemDetails = null;
        }

        if (!_isMouseOverUI)
            SetCursorImage(_currentSprite);
    }


    #region 鼠标状态检测

    /// <summary>
    /// 判断指针是否在UI上方
    /// </summary>
    /// <returns></returns>
    private bool IsMouseOverUI()
    {
        // EventSystem存在 && 鼠标指在UI上方
        return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 判断鼠标是否在移动
    /// </summary>
    /// <returns></returns>
    private bool IsMouseMoving()
    {
        return Input.mousePosition != _lastMousePosition;
    }

    /// <summary>
    /// 判断指针是否指向有效tile，切换指针的Valid状态
    /// </summary>
    private void CheckMouseValidation()
    {
        if (_isMouseNormal) SetCursorValid();
        if (!_currentGrid || _currentItemDetails == null) return;

        // 获取鼠标格子坐标
        MouseGridPosition = _currentGrid.WorldToCell(_mouseWorldPosition);

        // 如果鼠标距离大于物品可操作距离，不执行逻辑
        if ((Mathf.Abs(_mouseWorldPosition.x - _playerTransform.position.x) > _currentItemDetails.itemUseRadius ||
             Mathf.Abs(_mouseWorldPosition.y - _playerTransform.position.y) > _currentItemDetails.itemUseRadius)
            && !_isMouseNormal)
        {
            SetCursorInvalid();
            return;
        }

        // // 如果鼠标没有移动，就不重复检测格子属性
        // if (!_isMouseMoving)
        //     return;

        // 通过GridManager获取指针指中的格子属性
        var currentTile = GridMapManager.Instance.GetTileDetailsOnGridPosition(MouseGridPosition);
        if (currentTile != null)
        {
            // 根据选中物品的类型，匹配可操作的tile，如果tile不可操作，鼠标指针变红
            switch (_currentItemDetails.itemType)
            {
                case ItemType.Commodity:
                    if (currentTile.isDroppable) SetCursorValid();
                    else SetCursorInvalid();
                    break;
                case ItemType.HoeTool:
                    if (currentTile.isDiggable) SetCursorValid();
                    else SetCursorInvalid();
                    break;
                case ItemType.WaterTool:
                    if (currentTile.daysSinceDug >= 0 && currentTile.daysSinceWatered == -1) SetCursorValid();
                    else SetCursorInvalid();
                    break;
                case ItemType.Seed:
                    if (currentTile.daysSinceDug >= 0 && currentTile.seedItemID == -1) SetCursorValid();
                    else SetCursorInvalid();
                    break;
                case ItemType.CollectTool:
                case ItemType.ChopTool:
                    if (!CropManager.Instance.TryGetCropDetails(currentTile.seedItemID, out var currentCropDetails))
                    {
                        SetCursorInvalid();
                        return;
                    }

                    if (currentTile.growthDays >= currentCropDetails.totalGrowthDays
                        && currentCropDetails.CheckToolAvailability(_currentItemDetails.itemID))
                        SetCursorValid();
                    else SetCursorInvalid();
                    break;
            }
        }
        else
        {
            SetCursorInvalid();
        }
    }

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && _isCursorPositionValid && !_isMouseNormal && !_isMouseOverUI)
        {
            EventBus.RaiseValidMouseClicked(_mouseWorldPosition, _currentItemDetails);
        }
    }

    #endregion

    #region 设置鼠标样式

    private void SetCursorImage(Sprite sprite)
    {
        _cursorImage.sprite = sprite;
    }

    private void SetCursorValid()
    {
        if (_isCursorPositionValid) return;
        _isCursorPositionValid = true;
        _cursorImage.color = new Color(1f, 1f, 1f, 1f);
    }

    private void SetCursorInvalid()
    {
        if (!_isCursorPositionValid) return;
        _isCursorPositionValid = false;
        _cursorImage.color = new Color(1f, 0f, 0f, 0.5f);
    }

    #endregion
}