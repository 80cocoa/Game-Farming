using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventBus
{
    #region 背包相关

    // 当库存变动时，调用事件
    public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;

    /// <summary>
    /// 执行库存UI更新事件
    /// </summary>
    /// <param name="location">库存类型(Player, Box)</param>
    /// <param name="items">库存物品列表</param>
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> items)
    {
        UpdateInventoryUI?.Invoke(location, items);
    }

    //
    public static event Action<int, Vector3> InstantiateItemInScene;

    /// <summary>
    /// 在鼠标位置生成对应ID的物品
    /// </summary>
    /// <param name="itemID">生成的物品ID</param>
    /// <param name="initPos">生成位置</param>
    public static void CallInstantiateItemInScene(int itemID, Vector3 initPos)
    {
        InstantiateItemInScene?.Invoke(itemID, initPos);
    }

    //
    public static event Action<ItemDetails, bool> OnItemSelected;

    /// <summary>
    /// 物品被选中时发布广播
    /// </summary>
    /// <param name="itemDetails">物品信息</param>
    /// <param name="isSelected">是否被选中</param>
    public static void RaiseItemSelected(ItemDetails itemDetails, bool isSelected)
    {
        OnItemSelected?.Invoke(itemDetails, isSelected);
    }

    #endregion

    #region 时间系统相关

    public static event EventHandler<TimeArgs> OnTimeChanged;

    public static void RaiseTimeChanged(GameObject sender, TimeArgs args)
    {
        OnTimeChanged?.Invoke(sender, args);
    }

    #endregion

    #region 种植系统相关

    public static event Action<int, TileDetails> OnSeedPlanted;

    public static void RaiseSeedPlanted(int seedID, TileDetails tileDetails)
    {
        OnSeedPlanted?.Invoke(seedID, tileDetails);
    }

    public static event Action<int,bool> OnCropHarvested;

    public static void RaiseCropHarvested(int itemID,bool isGeneratedOnPlayer)
    {
        OnCropHarvested?.Invoke(itemID,isGeneratedOnPlayer);
    }
    
    public static event Action<ParticleEffectType,Vector3> OnSpriteShaken;

    public static void RaiseSpriteShaken(ParticleEffectType effectType, Vector3 shakePos)
    {
        OnSpriteShaken?.Invoke(effectType, shakePos);
    }
    
    #endregion

    #region 场景相关

    public static event Action OnSceneUnloading;

    public static void RaiseSceneUnloading()
    {
        OnSceneUnloading?.Invoke();
    }

    public static event Action OnSceneChanged;

    public static void RaiseSceneChanged()
    {
        OnSceneChanged?.Invoke();
    }

    public static event Action OnFadedOut;

    public static void RaiseFadedOut()
    {
        OnFadedOut?.Invoke();
    }

    public static event Action OnFadedIn;

    public static void RaiseFadedIn()
    {
        OnFadedIn?.Invoke();
    }

    #endregion

    #region 点击相关

    public static event Action<Vector3, ItemDetails> OnValidMouseClicked;

    public static void RaiseValidMouseClicked(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        OnValidMouseClicked?.Invoke(mouseWorldPos, itemDetails);
    }

    public static event Action<Vector3, ItemDetails> OnPlayerClickAnimFinished;

    public static void RaisePlayerClickAnimFinished(Vector3 mousePos, ItemDetails itemDetails)
    {
        OnPlayerClickAnimFinished?.Invoke(mousePos, itemDetails);
    }

    public static event Action<int> OnItemRemoved;

    public static void RaiseItemRemoved(int remainingAmount)
    {
        OnItemRemoved?.Invoke(remainingAmount);
    }

    #endregion
}