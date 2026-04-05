using UnityEngine.SceneManagement;

public enum ItemType
{
    Seed,
    Commodity,
    Furniture, //物品
    HoeTool,
    ChopTool,
    BreakTool,
    ReapTool,
    WaterTool,
    CollectTool, //工具
    ReapableScenery
}

public enum SlotType
{
    Bag,
    Box,
    Shop
}

public enum InventoryLocation
{
    Player,
    Box
}

public enum HoldItemType
{
    None,
    Carry,
    Hoe,
    Break,
    Water,
    Collect,
    Chop
}

public enum PlayerPartName
{
    Body,
    Hair,
    Arm,
    Tool
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public enum SceneName
{
    PersistentScene,
    UI,
    Map01Field,
    Map02Home
}

public enum GridType
{
    Dig,
    Drop,
    Furniture,
    NpcObstacle
}

public enum ParticleEffectType
{
    None,LeavesFalling01,
    LeavesFalling02,
    Rock,
    ReapableScenery
}

public static class EnumUtils
{
    /// <summary>
    /// 映射为HoldItemType枚举
    /// </summary>
    /// <param name="itemType">一个ItemType枚举</param>
    /// <returns></returns>
    public static HoldItemType MapToHoldItemType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Seed or ItemType.Commodity => HoldItemType.Carry,
            ItemType.HoeTool => HoldItemType.Hoe,
            ItemType.WaterTool => HoldItemType.Water,
            ItemType.CollectTool => HoldItemType.Collect,
            ItemType.ChopTool  => HoldItemType.Chop,
            _ => HoldItemType.None
        };
    }

    /// <summary>
    /// 映射为中文string
    /// </summary>
    /// <param name="itemType">一个ItemType枚举</param>
    /// <returns></returns>
    public static string MapToString(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool or ItemType.ChopTool or ItemType.CollectTool or ItemType.HoeTool or
                ItemType.ReapTool or ItemType.WaterTool => "工具",
            _ => "无"
        };
    }

    public static string MapToString(Season season)
    {
        return season switch
        {
            Season.Spring => "春天",
            Season.Summer => "夏天",
            Season.Autumn => "秋天",
            Season.Winter => "冬天",
            _ => ""
        };
    }

    public static string MapToString(SceneName scene)
    {
        return scene switch
        {
            SceneName.PersistentScene => "PersistentScene",
            SceneName.UI => "UI",
            SceneName.Map01Field => "01.Field",
            SceneName.Map02Home => "02.Home",
            _ => "01.Field"
        };
    }
}