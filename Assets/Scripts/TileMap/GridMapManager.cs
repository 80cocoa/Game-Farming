using System;
using System.Collections;
using System.Collections.Generic;
using MyGame.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace MyGame.Map
{
    public class GridMapManager : Singleton<GridMapManager>
    {
        [Header("地图信息")] [SerializeField] private List<TileMapData_SO> mapDataList;
        private string _currentSceneName;

        [Header("种地瓦片信息")] [SerializeField] private RuleTile digTile;
        [SerializeField] private RuleTile waterTile;
        [SerializeField] private int waterTileLastingDays = 1;
        [SerializeField] private int digTileLastingDays = 3;

        private Season _currentSeason;
        private Tilemap _digTilemap;
        private Tilemap _waterTilemap;

        private Vector3Int _capturedMouseGridPos;

        // 字典：地图坐标-tile信息  key：(GridPos.x,GridPos.y,SceneName)
        private readonly Dictionary<(int GridX, int GridY, string SceneName), TileDetails> _tileDetailsDict = new();

        private void OnEnable()
        {
            EventBus.OnSceneChanged += OnSceneChanged;

            EventBus.OnTimeChanged += OnTimeChanged;

            EventBus.OnValidMouseClicked += OnValidMouseClicked;
            EventBus.OnPlayerClickAnimFinished += OnPlayerClickAnimFinished;
        }

        private void OnDisable()
        {
            EventBus.OnSceneChanged -= OnSceneChanged;

            EventBus.OnTimeChanged -= OnTimeChanged;

            EventBus.OnValidMouseClicked -= OnValidMouseClicked;
            EventBus.OnPlayerClickAnimFinished -= OnPlayerClickAnimFinished;
        }

        private void Start()
        {
            foreach (var mapData in mapDataList)
            {
                InitTileDetailsDict(mapData);
            }
        }

        #region 事件订阅

        private void OnSceneChanged()
        {
            GameObject.FindWithTag("DigTilemap").TryGetComponent(out _digTilemap);
            GameObject.FindWithTag("WaterTilemap").TryGetComponent(out _waterTilemap);

            _currentSceneName = SceneManager.GetActiveScene().name;

            SetSceneTile(_currentSceneName);
        }

        private void OnTimeChanged(object sender, TimeArgs e)
        {
            _currentSeason = e.CurrentSeason;

            // 清空Crop，后面重新生成
            if (CropManager.Instance.CropParent)
            {
                for (int i = CropManager.Instance.CropParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(CropManager.Instance.CropParent.GetChild(i).gameObject);
                }
            }

            foreach (var tileDetail in _tileDetailsDict.Values)
            {
                if (tileDetail.daysSinceDug >= 0)
                {
                    tileDetail.daysSinceDug++;
                    if (tileDetail.daysSinceDug >= digTileLastingDays && tileDetail.seedItemID == -1)
                    {
                        RemoveDigTile(tileDetail);
                    }
                }

                if (tileDetail.daysSinceWatered >= 0)
                {
                    tileDetail.daysSinceWatered++;
                    if (tileDetail.daysSinceWatered >= waterTileLastingDays)
                    {
                        RemoveWaterTile(tileDetail);
                    }
                }

                if (tileDetail.growthDays >= 0)
                {
                    tileDetail.growthDays++;
                    EventBus.RaiseSeedPlanted(tileDetail.seedItemID, tileDetail);
                }
            }
        }

        private void OnValidMouseClicked(Vector3 mousePos, ItemDetails itemDetails)
        {
            _capturedMouseGridPos = CursorManager.Instance.MouseGridPosition;
        }

        private void OnPlayerClickAnimFinished(Vector3 mousePos, ItemDetails itemDetails)
        {
            // 通过CursorManager获取到网格坐标，获取格子信息
            var currentTile = GetTileDetailsOnGridPosition(_capturedMouseGridPos);

            if (currentTile != null)
            {
                switch (itemDetails.itemType)
                {
                    // Type商品 点击地面效果：扔掉
                    case ItemType.Commodity:
                        InventoryManager.Instance.DropItem(itemDetails.itemID,
                            CursorManager.Instance.MouseWorldPosition);
                        break;
                    case ItemType.HoeTool:
                        SetDigTile(currentTile);
                        currentTile.daysSinceDug = 0;
                        currentTile.isDiggable = false;
                        currentTile.isDroppable = false;
                        break;
                    case ItemType.WaterTool:
                        SetWaterTile(currentTile);
                        currentTile.daysSinceWatered = 0;
                        break;
                    case ItemType.Seed:
                        EventBus.RaiseSeedPlanted(itemDetails.itemID, currentTile);
                        break;
                    case ItemType.CollectTool:
                    case ItemType.ChopTool:
                        CropManager.Instance.ProcessToolAction(_capturedMouseGridPos, itemDetails);
                        break;
                }
            }

            UpdateTileDetailsDict(currentTile);
        }

        #endregion

        /// <summary>
        /// 初始化记录当前地图的字典
        /// </summary>
        /// <param name="mapData">要记录进字典的地图数据</param>
        private void InitTileDetailsDict(TileMapData_SO mapData)
        {
            foreach (TileProperty tileProp in mapData.tileProperties)
            {
                // 把 x坐标+y坐标+地图名 作为key
                var key = (tileProp.tileCoordinate.x, tileProp.tileCoordinate.y,
                    EnumUtils.MapToString(mapData.sceneName));
                // 取出已有的，或者创建新的
                if (!_tileDetailsDict.TryGetValue(key, out var tileDetails))
                {
                    tileDetails = new TileDetails
                    {
                        gridX = tileProp.tileCoordinate.x,
                        gridY = tileProp.tileCoordinate.y,
                    };
                }

                // 追加属性
                switch (tileProp.gridType)
                {
                    case GridType.Dig:
                        tileDetails.isDiggable = tileProp.boolTypeValue;
                        break;
                    case GridType.Drop:
                        tileDetails.isDroppable = tileProp.boolTypeValue;
                        break;
                    case GridType.Furniture:
                        tileDetails.isFurniturePlaceable = tileProp.boolTypeValue;
                        break;
                    case GridType.NpcObstacle:
                        tileDetails.isNpcObstacle = tileProp.boolTypeValue;
                        break;
                }

                // 赋值回字典
                _tileDetailsDict[key] = tileDetails;
            }
        }

        /// <summary>
        /// 根据网格坐标获取对应格子信息
        /// </summary>
        /// <param name="gridPosition">网格坐标</param>
        /// <returns>返回TileDetails，若找不到则返回null</returns>
        public TileDetails GetTileDetailsOnGridPosition(Vector3Int gridPosition)
        {
            var key = (gridPosition.x, gridPosition.y, _currentSceneName);
            return _tileDetailsDict.GetValueOrDefault(key);
        }

        /// <summary>
        /// 设置为挖坑tile
        /// </summary>
        /// <param name="tileDetails">指定的tile</param>
        private void SetDigTile(TileDetails tileDetails)
        {
            Vector3Int gridPos = new Vector3Int(tileDetails.gridX, tileDetails.gridY, 0);
            if (_digTilemap)
            {
                _digTilemap.SetTile(gridPos, digTile);
            }
        }

        private void RemoveDigTile(TileDetails tileDetails)
        {
            Vector3Int gridPos = new Vector3Int(tileDetails.gridX, tileDetails.gridY, 0);
            if (_digTilemap)
            {
                tileDetails.daysSinceDug = -1;
                tileDetails.isDiggable = true;
                tileDetails.seedItemID = -1;
                tileDetails.growthDays = -1;
                _digTilemap.SetTile(gridPos, null);
            }
        }

        /// <summary>
        /// 设置为浇水tile
        /// </summary>
        /// <param name="tileDetails">指定的tile</param>
        private void SetWaterTile(TileDetails tileDetails)
        {
            Vector3Int gridPos = new Vector3Int(tileDetails.gridX, tileDetails.gridY, 0);
            if (_waterTilemap)
            {
                tileDetails.daysSinceWatered = -1;
                _waterTilemap.SetTile(gridPos, waterTile);
            }
        }

        private void RemoveWaterTile(TileDetails tileDetails)
        {
            Vector3Int gridPos = new Vector3Int(tileDetails.gridX, tileDetails.gridY, 0);
            if (_waterTilemap)
            {
                _waterTilemap.SetTile(gridPos, null);
            }
        }


        /// <summary>
        /// 把对应tile数据记录回字典 (数据层)
        /// </summary>
        /// <param name="tileDetails">要记录的TileDetails</param>
        private void UpdateTileDetailsDict(TileDetails tileDetails)
        {
            var key = (tileDetails.gridX, tileDetails.gridY, SceneManager.GetActiveScene().name);
            _tileDetailsDict[key] = tileDetails;
        }

        /// <summary>
        /// 根据字典数据更新地图Tile形态 (表现层)
        /// </summary>
        /// <param name="sceneName">要更新的地图名</param>
        private void SetSceneTile(string sceneName)
        {
            foreach (var (key, tileDetails) in _tileDetailsDict)
            {
                if (key.SceneName == sceneName)
                {
                    if (tileDetails.daysSinceDug >= 0)
                    {
                        SetDigTile(tileDetails);
                    }

                    if (tileDetails.daysSinceWatered >= 0)
                    {
                        SetWaterTile(tileDetails);
                    }

                    if (tileDetails.seedItemID != -1)
                    {
                        EventBus.RaiseSeedPlanted(tileDetails.seedItemID, tileDetails);
                    }
                }
            }
        }

        public bool TryGetGridDimensions(string sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach (var mapData in mapDataList)
            {
                if ((EnumUtils.MapToString(mapData.sceneName)) == sceneName)
                {
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;
                    
                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                }
            }
            return false;
        }
    }
}