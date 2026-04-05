using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Map/Map Data")]
public class TileMapData_SO : ScriptableObject
{
    public SceneName sceneName;
    public List<TileProperty> tileProperties;
    [Header("地图信息")]
    public int gridWidth;
    public int gridHeight;
    [Header("左下角原点")]
    public int originX;
    public int originY;
    
}
