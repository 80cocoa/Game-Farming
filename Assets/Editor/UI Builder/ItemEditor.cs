using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common.Update.Partial;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ItemEditor : EditorWindow
{
    // 用于获取ItemDataList源文件
    private ItemDataList_SO _dataBase;

    // 用于告诉UI ListView所要存放的源数据
    private List<ItemDetails> _itemList;

    // 一个写好的用于表示在List中的模板
    [SerializeField] private VisualTreeAsset itemRowTemplate;

    // 获取UI界面左侧ListView组件
    private ListView _itemListView;

    // 获取UI界面右侧ScrollView组件
    private ScrollView _itemDetailsSection;

    // 获取UI界面右侧Icon组件
    private VisualElement _iconPreview;

    // 用于记录当前物品信息
    private ItemDetails _activeItem;

    // 默认Icon
    [SerializeField] private Sprite defaultIcon;

    [SerializeField] private VisualTreeAsset m_VisualTreeAsset;

    [MenuItem("MyGame/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);


        // 变量赋值
        // Q表示Query，在该物体子物体中查找对应名称的元素
        _itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");

        _itemDetailsSection = root.Q<ScrollView>("ItemDetails");
        _itemDetailsSection.visible = false;

        // 绑定按键
        root.Q<Button>("AddButton").clicked += OnAddButtonClicked;
        root.Q<Button>("DeleteButton").clicked += OnDeleteButtonClicked;
        root.Q<Button>("UpButton").clicked += OnUpButtonClicked;
        root.Q<Button>("DownButton").clicked += OnDownButtonClicked;

        // 加载数据
        LoadDataBase();
        
        RegisterItemDetailsValueChangedCallback();
        
        GenerateListView();
    }

    #region 按键事件

    private void OnAddButtonClicked()
    {
        ItemDetails newItem = new ItemDetails(_itemList.Count, defaultIcon);
        _itemList.Add(newItem);

        _itemListView.Rebuild();
    }

    private void OnDeleteButtonClicked()
    {
        _itemList.Remove(_activeItem);

        _itemDetailsSection.visible = false;
        _itemListView.Rebuild();
    }

    private void OnUpButtonClicked()
    {
        // 获取当前选中元素的下标
        int index = _itemList.IndexOf(_activeItem);
        if (index <= 0) return;

        // 语法糖，交换列表中元素位置
        (_itemList[index], _itemList[index - 1]) =
            (_itemList[index - 1], _itemList[index]);
        // 交换元素ID
        (_itemList[index].itemID, _itemList[index - 1].itemID) =
            (_itemList[index - 1].itemID, _itemList[index].itemID);

        // 继续选中该元素
        _itemListView.selectedIndex = index - 1;

        _itemListView.RefreshItems();
        GetItemDetails();
    }

    private void OnDownButtonClicked()
    {
        int index = _itemList.IndexOf(_activeItem);
        if (index >= _itemList.Count) return;

        (_itemList[index], _itemList[index + 1]) =
            (_itemList[index + 1], _itemList[index]);

        (_itemList[index].itemID, _itemList[index + 1].itemID) =
            (_itemList[index + 1].itemID, _itemList[index].itemID);

        _itemListView.selectedIndex = index + 1;
        
        _itemListView.RefreshItems();
        GetItemDetails();
    }

    #endregion


    /// <summary>
    /// 加载SO文件的数据
    /// </summary>
    private void LoadDataBase()
    {
        // 返回一个GUID
        var dataArray = AssetDatabase.FindAssets("t:ItemDataList_SO"); //"t:"表示要查找的对应类型

        if (dataArray.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            _dataBase = AssetDatabase.LoadAssetAtPath<ItemDataList_SO>(path);
        }

        // 将查找到的数据列表赋值给之后要用的_itemList
        _itemList = _dataBase.itemDetailsList;

        // 如果不标记则无法保存数据！
        EditorUtility.SetDirty(_dataBase);
    }


    /// <summary>
    /// 用于把SO文件内的数据生成在UI列表中
    /// </summary>
    private void GenerateListView()
    {
        // 官方方法，makeItem用于告诉ListView装载的项目类型
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        // bindItem用于告诉ListView装载时的规则   e表示element，i表示index，内部自动封装好了遍历方法
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i < _itemList.Count)
            {
                if (_itemList[i] == null) return;
                e.Q<VisualElement>("Icon").style.backgroundImage = _itemList[i].itemIcon.texture;
                e.Q<Label>("Name").text = _itemList[i].itemName == null ? "" : _itemList[i].itemName;
            }
        };

        // 配置最终ListView
        _itemListView.fixedItemHeight = 60;
        _itemListView.itemsSource = _itemList;
        _itemListView.makeItem = makeItem;
        _itemListView.bindItem = bindItem;

        // 注册函数，当列表选中对象改变时执行
        _itemListView.selectionChanged += OnListSelectionChange;
    }

    // 绑定在_itemListView的事件函数上
    // obj会返回当前所有被选中的项的集合
    private void OnListSelectionChange(IEnumerable<object> obj)
    {
        var selectedItem = obj.FirstOrDefault();
        if (selectedItem == null) return;
        _activeItem = selectedItem as ItemDetails;

        GetItemDetails();
    }

    /// <summary>
    /// 获取当前选中元素_activeItem的详细数据展示在界面右侧
    /// </summary>
    private void GetItemDetails()
    {
        if (_activeItem == null) return;

        // 标记界面为脏，才能修改界面数据
        _itemDetailsSection.MarkDirtyRepaint();

        // 显示ID
        _itemDetailsSection.Q<IntegerField>("ItemID").value = _activeItem.itemID;
        
        // 显示Name
        _itemDetailsSection.Q<TextField>("ItemName").value = _activeItem.itemName;
        
        // 显示IconPreview
        _itemDetailsSection.Q<VisualElement>("Icon").style.backgroundImage = _activeItem.itemIcon.texture;
        // 显示Icon
        _itemDetailsSection.Q<ObjectField>("ItemIcon").value =
            _activeItem.itemIcon == null ? defaultIcon : _activeItem.itemIcon;
        
        // 显示OnWorldSprite
        _itemDetailsSection.Q<ObjectField>("ItemSprite").value = _activeItem.itemOnWorldSprite;
        
        // 显示Type
        _itemDetailsSection.Q<EnumField>("ItemType").value = _activeItem.itemType;
        
        // 显示Description
        _itemDetailsSection.Q<TextField>("ItemDescription").value = _activeItem.itemDescription;
        
        // 显示UseRadius
        _itemDetailsSection.Q<IntegerField>("UseRadius").value = _activeItem.itemUseRadius;
        
        // 显示Bool值
        _itemDetailsSection.Q<Toggle>("IsPickable").value = _activeItem.isPickable;
        _itemDetailsSection.Q<Toggle>("IsDroppable").value = _activeItem.isDroppable;
        _itemDetailsSection.Q<Toggle>("IsCarriable").value = _activeItem.isCarriable;
        
        // 显示Price
        _itemDetailsSection.Q<IntegerField>("ItemPrice").value = _activeItem.itemPrice;
        
        // 显示SellPercentage
        _itemDetailsSection.Q<Slider>("SellPercentage").value = _activeItem.sellPercentage;
        
        // 显示右侧界面
        _itemDetailsSection.visible = true;
    }

    /// <summary>
    /// 注册所有数值改变的事件，在UI中改变数据时能反馈回SO文件当中
    /// </summary>
    private void RegisterItemDetailsValueChangedCallback()
    {
        // 注册回调函数，当IntegerField的值发生改变时，返回一个evt
        _itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemID = evt.newValue;
            _itemListView.RefreshItems();
        });
        
        _itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemName = evt.newValue;
            _itemListView.RefreshItems();
        });
        
        _itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
        {
            Sprite newIcon = evt.newValue as Sprite;
            _activeItem.itemIcon = newIcon;

            if (newIcon == null) return;
            _itemDetailsSection.Q<VisualElement>("Icon").style.backgroundImage = newIcon.texture;
            _itemListView.RefreshItems();
        });
        
        _itemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemOnWorldSprite = evt.newValue as Sprite;
        });
        
        _itemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemType = (ItemType)evt.newValue;
        });
        
        _itemDetailsSection.Q<TextField>("ItemDescription").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemDescription = evt.newValue;
        });

        _itemDetailsSection.Q<IntegerField>("UseRadius").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemUseRadius = evt.newValue;
        });
        
        _itemDetailsSection.Q<Toggle>("IsPickable").RegisterValueChangedCallback(evt =>
        {
            _activeItem.isPickable = evt.newValue;
        });
        
        _itemDetailsSection.Q<Toggle>("IsDroppable").RegisterValueChangedCallback(evt =>
        {
            _activeItem.isDroppable = evt.newValue;
        });
        
        _itemDetailsSection.Q<Toggle>("IsCarriable").RegisterValueChangedCallback(evt =>
        {
            _activeItem.isCarriable = evt.newValue;
        });
        
        _itemDetailsSection.Q<IntegerField>("ItemPrice").RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemPrice = evt.newValue;
        });
        
        _itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt =>
        {
            _activeItem.sellPercentage = evt.newValue;
        });
    }
}