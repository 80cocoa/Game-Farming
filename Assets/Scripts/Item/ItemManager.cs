using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGame.Inventory
{
    public class ItemManager : Singleton<ItemManager>
    {
        [SerializeField] Item itemPrefab;
        [SerializeField] Item bounceItemPrefab;
        private Transform _itemParent;
        private Transform _playerTransform;

        // 通过字典存储键值对：场景-物品列表
        private readonly Dictionary<string, List<SceneItem>> _itemDict = new();

        protected override void Awake()
        {
            base.Awake();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void OnEnable()
        {
            EventBus.InstantiateItemInScene += HandleInstantiateItemInScene;

            EventBus.OnSceneUnloading += GetSceneItems;

            EventBus.OnSceneChanged += GetItemParent;
            EventBus.OnSceneChanged += RecreateSceneItems;
        }

        private void OnDisable()
        {
            EventBus.InstantiateItemInScene -= HandleInstantiateItemInScene;

            EventBus.OnSceneUnloading -= GetSceneItems;

            EventBus.OnSceneChanged -= GetItemParent;
            EventBus.OnSceneChanged -= RecreateSceneItems;
        }

        private void HandleInstantiateItemInScene(int id, Vector3 pos)
        {
            InstantiateItem(id,pos);
        }

        public void InstantiateItem(int id, Vector3 pos)
        {
            var item = Instantiate(bounceItemPrefab, pos, Quaternion.identity, _itemParent);
            item.itemID = id;
            
            item.GetComponent<ItemBounce>().InitBounceItem(pos);
        }

        public void DropItem(int id, Vector3 targetPos)
        {
            // 扔东西的效果
            var item = Instantiate(bounceItemPrefab, _playerTransform.transform.position, Quaternion.identity, _itemParent);
            item.itemID = id;
            
            item.GetComponent<ItemBounce>().InitBounceItem(targetPos);
            
        }

        /// <summary>
        /// 获取场景中的物品父类
        /// </summary>
        private void GetItemParent()
        {
            _itemParent = GameObject.FindWithTag("ItemParent").transform;
            if (!_itemParent)
            {
                Debug.LogWarning("ItemParent is null");
            }
        }

        /// <summary>
        /// 获取场景中所有物品并存入字典
        /// </summary>
        private void GetSceneItems()
        {
            List<SceneItem> currentSceneItems = new();

            // 获取场景中所有物品并存入新的列表
            foreach (var item in _itemParent.GetComponentsInChildren<Item>())
            {
                SceneItem sceneItem = new SceneItem
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };
                currentSceneItems.Add(sceneItem);
            }

            // 更新字典
            _itemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
        }

        /// <summary>
        /// 根据字典重新生成场景物品
        /// </summary>
        private void RecreateSceneItems()
        {
            if (_itemDict.TryGetValue(SceneManager.GetActiveScene().name, out var currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    // 清场
                    foreach (var item in _itemParent.GetComponentsInChildren<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                    // 重新生成
                    foreach (var item in currentSceneItems)
                    {
                        InstantiateItem(item.itemID,item.position.ToVector3());
                    }
                }
            }
        }
    }
}