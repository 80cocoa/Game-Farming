using System.Collections;
using System.Collections.Generic;
using MyGame.Inventory;
using MyGame.Map;
using TMPro;
using UnityEngine;

public class Crop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public CropDetails CropDetails { get; set; }
    public Vector3Int CropGridPosition { get; set; }
    private Vector3 _cropPosition;
    private Sprite _currentSprite;
    private int _harvestActionCount;

    private HarvestShake _harvestShake;
    private Animator _harvestAnimator;
    private Transform _playerTransform;

    private readonly int _animTriggerFallingLeft = Animator.StringToHash("FallingLeft");
    private readonly int _animTriggerFallingRight = Animator.StringToHash("FallingRight");

    private void Start()
    {
        _cropPosition = new Vector3(CropGridPosition.x + 0.5f, CropGridPosition.y + 0.5f, 0);

        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (CropDetails.hasAnimation)
        {
            _harvestShake = GetComponentInChildren<HarvestShake>();
            _harvestAnimator = GetComponentInChildren<Animator>();
            if (_harvestAnimator)
            {
                _harvestAnimator.enabled = false;
            }
        }
    }

    public void SetSprite(Sprite sprite)
    {
        _currentSprite = sprite;
        if (_spriteRenderer) _spriteRenderer.sprite = sprite;
    }

    public void ProcessToolAction(ItemDetails itemDetails)
    {
        int requiredActionCount = CropDetails.GetRequiredActionCount(itemDetails.itemID);
        if (requiredActionCount == -1) return;

        // 点击计数器
        if (_harvestActionCount < requiredActionCount - 1)
        {
            // 判断是否有动画
            if (CropDetails.hasAnimation)
            {
                _harvestShake?.Play();
                //TODO:播放音效
            }

            _harvestActionCount++;
        }
        else
        {
            if (!CropDetails.hasAnimation)
            {
                SpawnHarvestItems();
            }
            else
            {
                HarvestAfterAnimation();
            }
        }
    }

    private void HarvestAfterAnimation()
    {
        var currentTile = GridMapManager.Instance.GetTileDetailsOnGridPosition(CropGridPosition);
        currentTile.harvestTimes = -1;
        currentTile.growthDays = -1;
        currentTile.seedItemID = -1;
        CropManager.Instance.RemoveCropFromDict(CropGridPosition);

        if (_harvestAnimator)
        {
            _harvestAnimator.enabled = true;

            _harvestAnimator?.SetTrigger(_playerTransform.position.x - _cropPosition.x > 0
                ? _animTriggerFallingLeft
                : _animTriggerFallingRight);

            StartCoroutine(SpawnHarvestItemsAfterAnimation(currentTile));
        }
    }

    private IEnumerator SpawnHarvestItemsAfterAnimation(TileDetails tileDetails)
    {
        yield return null;
        float duration = _harvestAnimator.GetCurrentAnimatorStateInfo(0).length;

        // 等待动画结束
        yield return new WaitForSeconds(duration);

        Spawn();

        yield return null;

        // 执行动画结束后的逻辑
        if (CropDetails.transferItemID != 0)
        {
            EventBus.RaiseSeedPlanted(CropDetails.transferItemID, tileDetails);

            Destroy(gameObject);
        }
    }

    private void SpawnHarvestItems()
    {
        Spawn();

        var currentTile = GridMapManager.Instance.GetTileDetailsOnGridPosition(CropGridPosition);
        if (currentTile != null)
        {
            currentTile.harvestTimes++;

            // 判断是否可以重复生长
            if (CropDetails.daysToRegrow > 0 && currentTile.harvestTimes < CropDetails.maxRegrowTimes)
            {
                currentTile.growthDays = CropDetails.totalGrowthDays - CropDetails.daysToRegrow;
                CropManager.Instance.DisplayCropPlant(currentTile, CropDetails);
            }
            else
            {
                currentTile.harvestTimes = -1;
                currentTile.growthDays = -1;
                currentTile.seedItemID = -1;
                CropManager.Instance.RemoveCropFromDict(CropGridPosition);
            }

            Destroy(gameObject);
        }
    }

    private void Spawn()
    {
        for (int i = 0; i < CropDetails.producedItemID.Length; i++)
        {
            int amountToProduce = Random.Range(CropDetails.produceMinAmount[i], CropDetails.produceMaxAmount[i] + 1);

            for (int j = 0; j < amountToProduce; j++)
            {
                if (!CropDetails.generateAtPlayerPosition)
                {
                    var dirX = _cropPosition.x > _playerTransform.position.x ? 1 : -1;
                    var spawnPos = new Vector3(_cropPosition.x + Random.Range(dirX, CropDetails.spawnRadius.x * dirX),
                        _cropPosition.y + Random.Range(-CropDetails.spawnRadius.y, CropDetails.spawnRadius.y), 0);

                    // 掉落物散落在地
                    ItemManager.Instance.InstantiateItem(CropDetails.producedItemID[i], spawnPos);

                    continue;
                }

                InventoryManager.Instance.HarvestItem(CropDetails.producedItemID[i], 1);
            }
            EventBus.RaiseCropHarvested(CropDetails.producedItemID[i], CropDetails.generateAtPlayerPosition);
        }
    }
}