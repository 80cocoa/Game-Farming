using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyGame.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimatorOverride : MonoBehaviour
{
    private Animator[] _animators;
    //[SerializeField]private Dictionary<string, Animator> animatorNameDict;

    [SerializeField] private SpriteRenderer holdItemSR;

    [Header("动画列表")] [SerializeField] private List<PlayerAnimatorType> animatorTypeList;

    [SerializeField] private float showHeldItemDuration = 0.4f;
    private readonly Dictionary<string, Animator> _animNameDict = new();

    void Awake()
    {
        _animators = GetComponentsInChildren<Animator>();

        foreach (var anim in _animators)
        {
            _animNameDict.Add(anim.name, anim);
        }
    }

    void OnEnable()
    {
        EventBus.OnItemSelected += OnItemSelected;
        EventBus.OnItemRemoved += OnItemRemoved;
        EventBus.OnCropHarvested += OnCropHarvested;
    }

    void OnDisable()
    {
        EventBus.OnItemSelected -= OnItemSelected;
        EventBus.OnItemRemoved -= OnItemRemoved;
        EventBus.OnCropHarvested -= OnCropHarvested;
    }

    private void OnCropHarvested(int itemID, bool isGeneratedOnPlayer)
    {
        if (!isGeneratedOnPlayer) return;
        Sprite cropSprite = InventoryManager.Instance.GetItemDetails(itemID).itemOnWorldSprite;
        if (!holdItemSR.enabled)
        {
            StartCoroutine(ShowHeldItem(cropSprite));
        }
    }

    private IEnumerator ShowHeldItem(Sprite sprite)
    {
        holdItemSR.sprite = sprite;
        holdItemSR.enabled = true;
        
        yield return new WaitForSeconds(showHeldItemDuration);
        
        holdItemSR.enabled = false;
    }

    /// <summary>
    /// 当物品被选中时播放对应动画
    /// </summary>
    /// <param name="itemDetails">物品信息</param>
    /// <param name="isSelected">是否时被选中状态</param>
    private void OnItemSelected(ItemDetails itemDetails, bool isSelected)
    {
        var holdItemType = EnumUtils.MapToHoldItemType(itemDetails.itemType);

        if (isSelected)
        {
            if (holdItemType == HoldItemType.Carry)
            {
                holdItemSR.sprite =
                    itemDetails.itemOnWorldSprite == null ? itemDetails.itemIcon : itemDetails.itemOnWorldSprite;
                holdItemSR.enabled = true;
            }
            else
            {
                holdItemSR.enabled = false;
                SwitchAnimator(HoldItemType.None);
            }
        }
        else
        {
            holdItemType = HoldItemType.None;
            holdItemSR.enabled = false;
        }

        SwitchAnimator(holdItemType);
    }
    
    /// <summary>
    /// 根据举起物品的类型改变玩家动画
    /// </summary>
    /// <param name="holdItemType">举起物品的枚举类型</param>
    private void SwitchAnimator(HoldItemType holdItemType)
    {
        foreach (var animType in animatorTypeList)
        {
            if(animType.holdItemType == holdItemType)
            {
                _animNameDict[animType.playerPartName.ToString()].runtimeAnimatorController =
                    animType.animOverrideController;
            }
        }
    }
    
    private void OnItemRemoved(int remainingAmount)
    {
        if (remainingAmount == 0)
        {
            SwitchAnimator(HoldItemType.None);
            holdItemSR.enabled = false;
        }
    }
}