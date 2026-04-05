using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryBag_SO", menuName = "Inventory/InventoryBag")]
public class InventoryBag_SO : ScriptableObject
{
    public List<InventoryItem> itemList;

    #region SO封装方法

    /// <summary>
    /// 在指定位置添加指定数量的物体
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="index">下标位置</param>
    /// <param name="amount">物品数量</param>
    public void AddItemAtIndex(int id, int index, int amount)
    {
        // 拾取物品逻辑：
        // 1.背包存在相同物体
        if (index != -1)
        {
            int currentAmount = itemList[index].itemAmount + amount;

            var item = new InventoryItem(id, currentAmount);
            itemList[index] = item;
        }
        // 2.背包不存在相同物体
        else
        {
            var item = new InventoryItem(id, amount);
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].itemID == 0)
                {
                    itemList[i] = item;
                    return; //存完数据后立刻跳出循环
                }
                // 如果没找到itemID为0的格子，则代表背包已满
            }
        }
    }

    /// <summary>
    /// 丢弃库存内指定数量的物品
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="amount">数量</param>
    /// <returns>库存数量是否够丢</returns>
    public bool TryRemoveItemAtIndex(int id, int amount)
    {
        var index = GetItemIndexInBag(id);
        if (index == -1) return false;

        var newAmount = itemList[index].itemAmount - amount;
        switch (newAmount)
        {
            case 0:
            {
                var newItem = new InventoryItem();
                itemList[index] = newItem;
                break;
            }
            case > 0:
            {
                var newItem = new InventoryItem(id, newAmount);
                itemList[index] = newItem;
                break;
            }
            case < 0:
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 检查背包是否已满
    /// </summary>
    /// <returns>已满返回true</returns>
    private bool IsBagFull()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 通过ID找到已有物品的位置
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <returns>返回物品下标，未找到返回-1</returns>
    public int GetItemIndexInBag(int id)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].itemID == id)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion
}