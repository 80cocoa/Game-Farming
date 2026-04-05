using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
    public int seedItemID;
    [Header("不同生长阶段需要的天数")] public int[] growthDays;
    public int totalGrowthDays;
    [Header("不同生长阶段物品Prefab")] public Crop[] growthPrefabs;
    [Header("不同阶段的图片")] public Sprite[] growthSprites;
    [Header("可种植的季节")] public Season[] seasons;
    [Space] [Header("收割工具")] public int[] harvestToolItemID;
    [Header("工具所需使用次数")] public int[] requiredActionCount;
    [Header("收割后转换为的新物品ID")] public int transferItemID;
    [Space] [Header("收割果实信息")] public int[] producedItemID;
    public int[] produceMinAmount;
    public int[] produceMaxAmount;
    public Vector2 spawnRadius;
    [Header("再次生长")] public Vector2 spawnOffset;
    public int daysToRegrow;
    public int maxRegrowTimes;

    [Header("其他设置")] public bool generateAtPlayerPosition;
    public bool hasAnimation;

    //TODO:音效、特效


    public bool CheckToolAvailability(int toolID)
    {
        foreach (var tool in harvestToolItemID)
        {
            if (toolID == tool) return true;
        }
        return false;
    }

    /// <summary>
    /// 获取对应工具所需的使用次数
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns>返回使用次数，若工具不可用则返回-1</returns>
    public int GetRequiredActionCount(int toolID)
    {
        for (int i = 0; i < harvestToolItemID.Length; i++)
        {
            if (harvestToolItemID[i] == toolID)
            {
                return requiredActionCount[i];
            }
        }
        return -1;
    }
}