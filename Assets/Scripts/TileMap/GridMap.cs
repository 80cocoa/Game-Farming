using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyGame.Map
{
    [ExecuteAlways] // 在编辑模式下运行

    public class GridMap : MonoBehaviour
    {
        public TileMapData_SO tileMapData;
        public GridType gridType;
        private Tilemap _currentTilemap;

        // 在脚本重新编译后就会执行
        private void OnEnable()
        {
            // 当脚本不在运行时才获取Tilemap组件
            if (!Application.IsPlaying(this))
            {
                _currentTilemap = GetComponent<Tilemap>();

                tileMapData?.tileProperties.Clear();
            }
        }

        private void OnDisable()
        {
            if (!Application.IsPlaying(this))
            {
                _currentTilemap = GetComponent<Tilemap>();

                UpdateTileProperties();

#if UNITY_EDITOR
                if (tileMapData != null)
                    EditorUtility.SetDirty(tileMapData);
#endif
            }
        }

        /// <summary>
        /// 更新tile属性
        /// </summary>
        private void UpdateTileProperties()
        {
            // 获取一个刚好包住该物体下所有可见tile的最小矩形范围
            _currentTilemap.CompressBounds();

            if (!Application.IsPlaying(this))
            {
                if (tileMapData != null)
                {
                    // 已绘制范围的左下角坐标（这里的坐标采用格子坐标系，单位是“格”）
                    Vector3Int startPos = _currentTilemap.cellBounds.min;
                    // 右上角
                    Vector3Int endPos = _currentTilemap.cellBounds.max;

                    for (int x = startPos.x; x < endPos.x; x++)
                    {
                        for (int y = startPos.y; y < endPos.y; y++)
                        {
                            // 循环获取每个tile
                            TileBase tile = _currentTilemap.GetTile(new Vector3Int(x, y, 0));

                            if (tile != null)
                            {
                                var newTile = new TileProperty
                                {
                                    tileCoordinate = new Vector2Int(x, y),
                                    gridType = this.gridType,
                                    boolTypeValue = true
                                };
                                tileMapData.tileProperties.Add(newTile);
                            }
                        }
                    }
                }
            }
        }
    }
}