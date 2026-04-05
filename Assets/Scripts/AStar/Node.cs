using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MyGame.AStar
{
    public class Node : IComparable<Node> //System命名空间提供的用于比较的接口
    {
        public Vector2Int gridPosition;  //网格坐标
        public int gCost = 0;  //距离Start格子的距离
        public int hCost = 0;  //距离Target格子的距离
        public int FCost => gCost + hCost;  //当前格子的值

        public bool isObstacle = false;  //当前格子是否是障碍
        
        public Node parentNode;

        public Node(Vector2Int gridPosition)
        {
            this.gridPosition = gridPosition;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            // 通过比较选出最低的F值，返回-1,0,1
            int result = FCost.CompareTo(other.FCost);
            // 如果F值相等，比较H值
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }
            return result;
        }
    }
}
