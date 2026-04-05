using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MyGame.Inventory
{
//继承这些接口，可以实现点击后执行的方法
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("组件获取")]
        // 在Inspector窗口中拖拽比Awake中获取更快
        [SerializeField]
        private Image slotImage;

        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image slotHighlight;
        [SerializeField] private Button button;

        private InventoryUI _inventoryUI;

        [Header("格子属性")] public SlotType slotType;
        public int slotIndex;
        private bool _isSelected;
        private bool _isDragging;

        // 保存的物品数据信息
        private ItemDetails _itemDetails;
        private int _itemAmount;


        private void Awake()
        {
            _inventoryUI = GetComponentInParent<InventoryUI>();
        }

        private void Start()
        {
            // 开始时初始化空格子
            _isSelected = false;
            if (_itemDetails == null || _itemDetails.itemID == 0)
            {
                UpdateEmptySlot();
            }
        }

        /// <summary>
        /// 更新格子信息和UI
        /// </summary>
        /// <param name="itemDetails">ItemDetails</param>
        /// <param name="amount">物品数量</param>
        public void UpdateSlot(ItemDetails itemDetails, int amount)
        {
            if (itemDetails == null || amount == 0)
            {
                UpdateEmptySlot();
                return;
            }

            if (itemDetails.itemID == _itemDetails?.itemID && amount == _itemAmount)
            {
                return;
            }

            _itemDetails = itemDetails;
            slotImage.sprite = itemDetails.itemIcon;
            _itemAmount = amount;
            amountText.text = _itemAmount.ToString();

            slotImage.enabled = true;
            button.interactable = true;
        }

        /// <summary>
        /// 将Slot更新为空
        /// </summary>
        public void UpdateEmptySlot()
        {
            InactivateSlotHighlight();
            
            slotImage.enabled = false;
            _itemAmount = 0;
            amountText.text = "";
            button.interactable = false;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            HandleClickEvent();
        }

        public void HandleClickEvent()
        {
            if (_isDragging) return;
            if (_itemAmount == 0) return;
            //Debug.Log($"OnPointerClicked frame: {Time.frameCount}");
            
            _inventoryUI.UpdateSlotHighlight(slotIndex, true);
            if (slotType == SlotType.Bag)
            {
                EventBus.RaiseItemSelected(_itemDetails, _isSelected);
            }
        }

        /// <summary>
        /// 激活格子高亮显示，如果已经是高亮，则取消高亮
        /// </summary>
        public void ActivateSlotHighlight()
        {
            if (_isSelected) return;
            slotHighlight.gameObject.SetActive(true);
            _isSelected = true;
        }

        /// <summary>
        /// 取消激活格子高亮显示
        /// </summary>
        public void InactivateSlotHighlight()
        {
            if (!_isSelected) return;
            slotHighlight.gameObject.SetActive(false);
            _isSelected = false;
        }

        /// <summary>
        /// 开关格子高亮显示
        /// </summary>
        public void TriggerSlotHighlight()
        {
            if (_isSelected)
            {
                InactivateSlotHighlight();
            }
            else
            {
                ActivateSlotHighlight();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_itemAmount == 0) return;

            //UpdateEmptySlot();
            _isDragging = true;

            _inventoryUI.dragItemImage.sprite = slotImage.sprite;
            _inventoryUI.dragItemImage.enabled = true;
            _inventoryUI.dragItemImage.SetNativeSize(); //防止图像失真

            _inventoryUI.UpdateSlotHighlight(slotIndex, false);

            if (slotType == SlotType.Bag)
            {
                EventBus.RaiseItemSelected(_itemDetails, _isSelected);
                //Debug.Log($"BeginDragEvent raised{_isSelected}");
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_itemAmount == 0) return;
            _inventoryUI.dragItemImage.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_itemAmount == 0) return;
            _inventoryUI.dragItemImage.enabled = false;

            // 会返回鼠标最后碰撞的UI
            // 如果检测到UI
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent(out SlotUI targetSlot))
                {
                    // 执行交换格子物品逻辑
                    InventoryManager.Instance.SwapItem(slotIndex, targetSlot.slotIndex);
                    _inventoryUI.UpdateSlotHighlight(targetSlot.slotIndex, false);
                    
                }
            }
            //Debug.Log($"OnEndDrag frame: {Time.frameCount}");
            _isDragging = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_itemAmount == 0) return;
            _inventoryUI.ShowItemTooltip(_itemDetails, slotType, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _inventoryUI.HideItemTooltip();
        }
    }
}