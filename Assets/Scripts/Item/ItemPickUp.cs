using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 判断该Trigger是否挂载Item脚本
            if (other.TryGetComponent(out Item item))
            {
                // 如果物品可以被拾取 isPickable=true
                if (item.ItemDetails.isPickable)
                {
                    // 拾取物品到背包
                    InventoryManager.Instance.PickUpItem(item, true);
                    //Debug.Log($"Picked up {item.ItemDetails.itemName}");
                }
            }
        }
    }
}