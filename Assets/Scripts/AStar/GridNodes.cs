using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.AStar
{
    public class GridNodes
    {
        private int _width;
        private int _height;
        public Node[,] gridNodes;  //二维数组

        /// <summary>
        /// 构造函数初始化节点范围数组
        /// </summary>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        public GridNodes(int width, int height)
        {
            this._width = width;
            this._height = height;
            
            gridNodes = new Node[_width, _height];
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    gridNodes[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }
        
        /// <summary>
        /// 通过网格坐标获取Node
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Node GetGridNode(int x, int y)
        {
            if (x < _width && y < _height)
            {
                return gridNodes[x, y];
            }
            Debug.LogWarning("超出网格范围");
            return null;
        }
    }
}
