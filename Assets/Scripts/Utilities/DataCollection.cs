using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemDetails
{
    public int itemID;
    public string itemName;
    
    public ItemType itemType;

    public Sprite itemIcon;

    public Sprite itemOnWorldSprite;

    public string itemDescription;
    public int itemUseRadius; //可使用的网格范围

    public bool isPickable;
    public bool isDroppable;
    public bool isCarriable;

    public int itemPrice;
    [Range(0, 1)] public float sellPercentage; //售卖折损百分比

    public ItemDetails()
    {
        itemID = 1000;
        itemName = "New Item";
        itemType = ItemType.Commodity;
        itemIcon = null;
        itemOnWorldSprite = null;
        itemDescription = "This is a New Item";
        itemUseRadius = 1;
        isPickable = true;
        isDroppable = true;
        isCarriable = true;
        itemPrice = 0;
        sellPercentage = 0;
    }

    public ItemDetails(int currentIndex, Sprite defaultIcon) : this()
    {
        itemID = currentIndex + 1001;
        itemIcon = defaultIcon;
    }
}

// 数据驱动的背包，使用struct数据类型
[System.Serializable]
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;

    public InventoryItem(int id, int amount)
    {
        itemID = id;
        itemAmount = amount;
    }
}

//
[System.Serializable]
public class PlayerAnimatorType
{
    public HoldItemType holdItemType;
    public PlayerPartName playerPartName;
    public AnimatorOverrideController animOverrideController;
}

[System.Serializable]
public class SerializableVector3
{
    // 直接用Vector3不能被序列化，要换成float
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    // 返回整型Vector2
    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[System.Serializable]
public class TileProperty
{
    // tile坐标
    public Vector2Int tileCoordinate;

    // 该tile的类型是否已被设置完毕
    public bool boolTypeValue;
    
    public GridType gridType;
}

[System.Serializable]
public class TileDetails
{
    public int gridX, gridY;
    public bool isDiggable;
    public bool isDroppable;
    public bool isFurniturePlaceable;
    public bool isNpcObstacle;
    
    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;

    public int seedItemID = -1;
    public int growthDays = -1;
    public int harvestTimes = -1;

}

[System.Serializable]
public class NpcPosition
{
    public Transform npcTrans;
    public SceneName startScene;
    public Vector3 position;
}

[System.Serializable]
public class ScenePath
{
    public SceneName sceneName;
    public Vector2Int fromGridCoordinates;
    public Vector2Int toGridCoordinates;
}

[System.Serializable]
public class SceneRoute
{
    public SceneName fromScene;
    public SceneName toScene;
    public List<ScenePath> paths;
}