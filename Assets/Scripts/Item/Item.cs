using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;
        [SerializeField] private ItemDataList_SO itemDataListSO;

        private SpriteRenderer _sr;
        private BoxCollider2D _coll;
        private ItemDetails _itemDetails;

        public ItemDetails ItemDetails => _itemDetails;

        void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
            _coll = GetComponentInChildren<BoxCollider2D>();
        }

        void Start()
        {
            if (itemDataListSO.itemDetailsList.Exists(i => i.itemID == itemID))
                Initialize(itemID);
            else
                Debug.LogError("ItemID:" + itemID + " not found");
        }

        /// <summary>
        /// 通过ID实例化当前物体
        /// </summary>
        /// <param name="id">要实例化的物品ID</param>
        private void Initialize(int id)
        {
            itemID = id;
            // 通过InventoryManager实例化当前物体
            _itemDetails = InventoryManager.Instance.GetItemDetails(id);

            if (_itemDetails != null)
            {
                // 修改物品图片
                _sr.sprite = _itemDetails.itemOnWorldSprite == null
                    ? _itemDetails.itemIcon
                    : _itemDetails.itemOnWorldSprite;

                // 修改碰撞体尺寸！
                Vector2 newSize = new Vector2(_sr.sprite.bounds.size.x, _sr.sprite.bounds.size.y);
                _coll.size = newSize;
                _coll.offset = new Vector2(0, _sr.sprite.bounds.center.y);
            }
        }
    }
}