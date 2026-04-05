using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    [Header("文本内容")] [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Text valueText;
    [SerializeField] private GameObject bottomPart;

    [Header("UI组件")] [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UISettings_SO uiSettingsSO;

    private RectTransform _rectTransform;


    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        Hide();
    }

    /// <summary>
    /// 设置Tooltip显示内容
    /// </summary>
    /// <param name="itemDetails">物品信息</param>
    /// <param name="slotType">格子种类</param>
    public void SetUpTooltip(ItemDetails itemDetails, SlotType slotType)
    {
        nameText.text = itemDetails.itemName;
        typeText.text = EnumUtils.MapToString(itemDetails.itemType);
        descriptionText.text = itemDetails.itemDescription;
        if (itemDetails.itemPrice != 0)
        {
            bottomPart.SetActive(true);

            var price = slotType switch
            {
                SlotType.Bag or SlotType.Box => itemDetails.itemPrice * itemDetails.sellPercentage,
                SlotType.Shop => itemDetails.itemPrice,
                _ => 0
            };
            valueText.text = price.ToString();
        }
        else
        {
            bottomPart.SetActive(false);
        }
        
        // 强制重构Canvas，防止Description行数不同造成的tooltip界面大小问题
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    /// <summary>
    /// 显示tooltip界面
    /// </summary>
    /// <param name="pos">物品位置</param>
    public void Show(Vector3 pos)
    {
        canvasGroup.alpha = 1;
        _rectTransform.position = pos + Vector3.up * uiSettingsSO.tooltipOffsetMultiplier;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
    }
    
}