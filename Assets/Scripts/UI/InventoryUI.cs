using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Tooltip组件")] [SerializeField] private ItemTooltipUI itemTooltipUI;

        [Header("拖拽图片")] public Image dragItemImage;

        [Header("玩家背包UI")] [SerializeField] private GameObject playerBagUI;
        private bool _isBagOpened;

        [SerializeField] private SlotUI[] playerSlots;


        private void OnEnable()
        {
            // 订阅更新库存UI事件
            EventBus.UpdateInventoryUI += HandleUpdateInventoryUI;
        }


        private void OnDisable()
        {
            EventBus.UpdateInventoryUI -= HandleUpdateInventoryUI;
        }

        private void Start()
        {
            // 为背包每个格子初始化序号
            for (int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i;
            }

            // 判断玩家背包是否为打开状态
            _isBagOpened = playerBagUI.activeInHierarchy;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                TriggerBagUI();
            }
        }

        /// <summary>
        /// 用于订阅库存UI更新的方法
        /// </summary>
        /// <param name="location">库存类型</param>
        /// <param name="list">要更新的库存列表</param>
        private void HandleUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                // 执行玩家背包UI更新逻辑
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        // 将背包内的格子更新为对应物品UI
                        var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                        playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                    }

                    break;
                default:
                    Debug.Log("尚未实现对应库存更新方法");
                    break;
            }
        }

        /// <summary>
        /// 当格子被选中时执行的激活高亮显示的方法
        /// </summary>
        /// <param name="index">格子下标</param>
        /// <param name="isTrigger">是否是trigger激活</param>
        public void UpdateSlotHighlight(int index, bool isTrigger)
        {
            foreach (SlotUI slot in playerSlots)
            {
                if (slot.slotIndex == index)
                {
                    if (isTrigger)
                        slot.TriggerSlotHighlight();
                    else
                        slot.ActivateSlotHighlight();
                }
                else
                {
                    slot.InactivateSlotHighlight();
                }
            }
        }

        /// <summary>
        /// 开关背包UI
        /// </summary>
        public void TriggerBagUI()
        {
            _isBagOpened = !_isBagOpened;
            playerBagUI.SetActive(_isBagOpened);
        }

        public void ShowItemTooltip(ItemDetails itemDetails, SlotType slotType, Vector3 pos)
        {
            itemTooltipUI.SetUpTooltip(itemDetails, slotType);
            itemTooltipUI.Show(pos);
        }

        public void HideItemTooltip()
        {
            itemTooltipUI.Hide();
        }
    }
}