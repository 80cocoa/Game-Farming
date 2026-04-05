using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyGame.Inventory;
using UnityEngine;

public class CropManager : Singleton<CropManager>
{
    [SerializeField] private CropDataList_SO cropData;
    [SerializeField] private Crop testCropPrefab;

    private Transform _cropParent;
    public Transform CropParent => _cropParent;
    private Grid _currentGrid;
    private Season _currentSeason;
    private Vector3Int? _lastCropPosition;
    private Crop _lastCrop;

    private readonly Dictionary<Vector3Int, Crop> _cropDict = new();

    private void OnEnable()
    {
        EventBus.OnSceneChanged += OnSceneChanged;
        EventBus.OnTimeChanged += OnTimeChanged;
        EventBus.OnSeedPlanted += OnSeedPlanted;
        EventBus.OnCropHarvested += OnCropHarvested;
    }

    private void OnDisable()
    {
        EventBus.OnSeedPlanted -= OnSeedPlanted;
        EventBus.OnTimeChanged -= OnTimeChanged;
        EventBus.OnSceneChanged -= OnSceneChanged;
        EventBus.OnCropHarvested -= OnCropHarvested;
    }

    private void OnCropHarvested(int itemID, bool isGeneratedOnPlayer)
    {
        _lastCropPosition = null;
    }

    private void OnTimeChanged(object sender, TimeArgs e)
    {
        _currentSeason = e.CurrentSeason;
    }

    private void OnSceneChanged()
    {
        _currentGrid = GameObject.FindWithTag("TileMap").GetComponent<Grid>();
        _cropParent = GameObject.FindWithTag("CropParent").transform;
    }

    private void OnSeedPlanted(int seedID, TileDetails tileDetails)
    {
        TryGetCropDetails(seedID, out var currentCrop);
        // 用于初次种植（存在种子信息 && 季节合适）
        if (currentCrop != null && CheckSeasonAvailability(currentCrop) && tileDetails.seedItemID == -1)
        {
            tileDetails.seedItemID = seedID;
            tileDetails.growthDays = 0;

            InventoryManager.Instance.RemovePlayerBagItem(seedID, 1);

            DisplayCropPlant(tileDetails, currentCrop);
        }
        // 用于刷新
        else if (tileDetails.seedItemID != -1)
        {
            DisplayCropPlant(tileDetails, currentCrop);
        }
    }

    public void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
    {
        // 成长阶段
        int growthStages = cropDetails.growthDays.Length;
        int currentStage = 0;

        // 倒序计算当前成长阶段
        int dayCounter = cropDetails.totalGrowthDays;
        for (int i = growthStages - 1; i >= 0; i--)
        {
            if (tileDetails.growthDays >= dayCounter)
            {
                currentStage = i;
                break;
            }

            dayCounter -= cropDetails.growthDays[i];
        }

        // // 正序计算
        // int dayCounter = 0;
        // for (int i = 0; i < growthStages; i++)
        // {
        //     if (tileDetails.growthDays < cropDetails.totalGrowthDays)
        //     {
        //         dayCounter += cropDetails.growthDays[i];
        //         if (tileDetails.growthDays <= dayCounter)
        //         {
        //             currentStage = i + 1;
        //             break;
        //         }
        //     }
        //     else
        //     {
        //         currentStage = growthStages;
        //         break;
        //     }
        // }

        // 获取当前阶段的Prefab
        var gridPos = new Vector3Int(tileDetails.gridX, tileDetails.gridY, 0);
        Crop cropPrefab = cropDetails.growthPrefabs[currentStage];
        Sprite cropSprite = cropDetails.growthSprites[currentStage];
        // 获取对应网格的中心位置
        Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);

        var cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, _cropParent);
        // 初始化作物，并加入字典方便查询
        cropInstance.SetSprite(cropSprite);
        cropInstance.CropDetails = cropDetails;
        cropInstance.CropGridPosition = gridPos;
        _cropDict[gridPos] = cropInstance;
    }

    /// <summary>
    /// 对对应位置的作物使用工具
    /// </summary>
    /// <param name="pos">格子坐标</param>
    /// <param name="itemDetails">使用的工具ItemDetails</param>
    /// <returns></returns>
    public void ProcessToolAction(Vector3Int pos, ItemDetails itemDetails)
    {
        if (_lastCropPosition != pos)
        {
            _lastCropPosition = pos;
            TryGetCrop(pos, out _lastCrop);
            {
                _lastCrop.ProcessToolAction(itemDetails);
            }
        }
        else
        {
            _lastCrop.ProcessToolAction(itemDetails);
        }
    }

    /// <summary>
    /// 尝试获取目标Crop
    /// </summary>
    /// <param name="pos">格子坐标</param>
    /// <param name="crop">返回目标Crop</param>
    /// <returns>存在Crop返回true</returns>
    public bool TryGetCrop(Vector3Int pos, out Crop crop)
    {
        foreach (var cp in _cropDict)
        {
            if (cp.Key == pos)
            {
                crop = cp.Value;
                return true;
            }
        }

        crop = null;
        return false;
    }

    public void RemoveCropFromDict(Vector3Int pos)
    {
        _cropDict.Remove(pos);
    }

    /// <summary>
    /// 根据id查找种子信息
    /// </summary>
    /// <param name="seedID">种子id</param>
    /// <param name="cropDetails"></param>
    /// <returns></returns>
    public bool TryGetCropDetails(int seedID, out CropDetails cropDetails)
    {
        foreach (var t in cropData.cropDetailsList)
        {
            if (t.seedItemID == seedID)
            {
                cropDetails = t;
                return true;
            }
        }

        cropDetails = null;
        return false;
    }

    /// <summary>
    /// 检查该种子能否在当前季节种植
    /// </summary>
    /// <param name="cropDetails">种子信息</param>
    /// <returns></returns>
    private bool CheckSeasonAvailability(CropDetails cropDetails)
    {
        return Array.Exists(cropDetails.seasons, x => x == _currentSeason);
    }
}