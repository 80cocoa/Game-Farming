using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MyGame.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [Header("物品数据")] [SerializeField] private ItemDataList_SO itemDataListSO;
        [Header("背包数据")] [SerializeField] private InventoryBag_SO playerBagSO;

        private void Start()
        {
            // 游戏开始时也要更新背包UI
            EventBus.CallUpdateInventoryUI(InventoryLocation.Player, playerBagSO.itemList);
        }


        /// <summary>
        /// 查找对应id的元素
        /// </summary>
        /// <param name="id">所查找元素的ID</param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int id)
        {
            // 找出itemID与id相同的元素
            return itemDataListSO.GetItemDetailsFromID(id);
        }

        public void HarvestItem(int itemID, int amount)
        {
            var index = playerBagSO.GetItemIndexInBag(itemID);
            
            playerBagSO.AddItemAtIndex(itemID, index, 1);
            
            EventBus.CallUpdateInventoryUI(InventoryLocation.Player, playerBagSO.itemList);
        }

        /// <summary>
        /// 拾取物品到玩家背包
        /// </summary>
        /// <param name="item">要拾取的物品</param>
        /// <param name="shouldDestroy">拾取后是否从世界销毁</param>
        public void PickUpItem(Item item, bool shouldDestroy)
        {
            var index = playerBagSO.GetItemIndexInBag(item.itemID);

            playerBagSO.AddItemAtIndex(item.itemID, index, 1);

            if (shouldDestroy)
            {
                Destroy(item.gameObject);
            }

            // 通知UI更新事件启用
            EventBus.CallUpdateInventoryUI(InventoryLocation.Player, playerBagSO.itemList);
        }

        /// <summary>
        /// 丢弃一个背包物品
        /// </summary>
        /// <param name="itemID">物品ID</param>
        /// <param name="position">丢弃点位</param>
        public void DropItem(int itemID, Vector3 position)
        {
            if (playerBagSO.TryRemoveItemAtIndex(itemID, 1))
            {
                ItemManager.Instance.DropItem(itemID, position);
                EventBus.CallUpdateInventoryUI(InventoryLocation.Player, playerBagSO.itemList);

                // 判断丢弃后的物品数量是否为零
                RaiseItemRemoved(itemID);
            }
        }

        public void RemovePlayerBagItem(int itemID,int amount)
        {
            if (playerBagSO.TryRemoveItemAtIndex(itemID, amount))
            {
                EventBus.CallUpdateInventoryUI(InventoryLocation.Player, playerBagSO.itemList);
                
                RaiseItemRemoved(itemID);
            }
        }

        private void RaiseItemRemoved(int itemID)
        {
            var removedItemIndex = playerBagSO.GetItemIndexInBag(itemID);
            EventBus.RaiseItemRemoved(removedItemIndex == -1
                ? 0
                : playerBagSO.itemList[removedItemIndex].itemAmount);
        }

        /// <summary>
        /// 交换库存中两个物品
        /// </summary>
        /// <param name="fromIndex">下标1</param>
        /// <param name="toIndex">下标2</param>
        public void SwapItem(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;
            InventoryItem currentItem = playerBagSO.itemList[fromIndex];
            InventoryItem targetItem = playerBagSO.itemList[toIndex];

            playerBagSO.itemList[fromIndex] = targetItem;
            playerBagSO.itemList[toIndex] = currentItem;

            EventBus.CallUpdateInventoryUI(InventoryLocation.Player, playerBagSO.itemList);
        }
    }
}