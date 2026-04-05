using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Map;

namespace MyGame.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes _gridNodes;
        private Node _startNode;
        private Node _targetNode;
        private int _gridWidth;
        private int _gridHeight;
        private int _originX;
        private int _originY;

        private List<Node> _openNodeList; //当前选中Node周围的8个节点
        private HashSet<Node> _closedNodeList; //所有被选中的点（查找时速度更快）

        private bool _pathFound;
        
        /// <summary>
        /// 构建路径，更新npcMovementStepStack的每一步
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="npcMovementSteps"></param>
        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int endPos, Stack<MovementStep> npcMovementSteps)
        {
            _pathFound = false;
            //Debug.Log("Successfully built path");
            if (GenerateGridNodes(sceneName, startPos, endPos))
            {
                //Debug.Log("Successfully generated path");
                // 1.查找最短路径
                if (TryFindShortestPath())
                {
                    //Debug.Log("Successfully found shortest path");
                    // 2.构建NPC移动路径
                    UpdatePathOnMovementStepStack(sceneName, npcMovementSteps);
                }
            }
        }

        /// <summary>
        /// 找到最短路径所有节点，添加到closedList
        /// </summary>
        /// <returns></returns>
        private bool TryFindShortestPath()
        {
            if (_openNodeList == null || _closedNodeList == null) return false;

            // 添加起点
            _openNodeList.Add(_startNode);

            while (_openNodeList.Count > 0)
            {
                // 节点排序，借助Node内比较函数（继承IComparable)
                _openNodeList.Sort();

                Node closestNode = _openNodeList[0];
                _openNodeList.Remove(closestNode);
                _closedNodeList.Add(closestNode);

                // 找到路径则跳出循环
                if (closestNode == _targetNode)
                {
                    _pathFound = true;
                    break;
                }

                // 未找到路径，继续向openList添加节点
                EvaluateNeighbourNodes(closestNode);
            }

            return _pathFound;
        }

        /// <summary>
        /// 评估周围8个节点的Cost值
        /// </summary>
        /// <param name="currentNode"></param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            var currentNodePos = currentNode.gridPosition;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    if (TryGetValidNode(currentNode.gridPosition.x + x, currentNode.gridPosition.y + y,
                            out var validNeighbourNode))
                    {
                        if (!_openNodeList.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, _targetNode);
                            // 连接父节点
                            validNeighbourNode.parentNode = currentNode;
                            _openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 通过网格坐标获取有效的Node（非障碍、非已选择）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="validNode"></param>
        /// <returns></returns>
        private bool TryGetValidNode(int x, int y, out Node validNode)
        {
            if (x >= _gridWidth || x < 0 || y >= _gridHeight || y < 0)
            {
                validNode = null;
                return false;
            }

            Node neighbourNode = _gridNodes.GetGridNode(x, y);

            if (neighbourNode == null || neighbourNode.isObstacle || _closedNodeList.Contains(neighbourNode))
            {
                validNode = null;
                return false;
            }
            else
            {
                validNode = neighbourNode;
                return true;
            }
        }

        /// <summary>
        /// 返回两点距离值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>距离值（14的倍数+10的倍数）</returns>
        private int GetDistance(Node a, Node b)
        {
            int xDistance = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
            int yDistance = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);

            if (xDistance > yDistance)
            {
                // 先走对角线，再走直线
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            else
            {
                return 14 * xDistance + 10 * (yDistance - xDistance);
            }
        }

        /// <summary>
        /// 构建网格节点信息，初始化两个列表
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        private bool  GenerateGridNodes(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            if (GridMapManager.Instance.TryGetGridDimensions(sceneName, out var gridDimensions, out var gridOrigin))
            {
                //Debug.Log("Successfully Generated grid");
                _gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                _gridWidth = gridDimensions.x;
                _gridHeight = gridDimensions.y;
                _originX = gridOrigin.x;
                _originY = gridOrigin.y;

                _openNodeList = new List<Node>();
                _closedNodeList = new HashSet<Node>();
            }
            else
            {
                return false;
            }

            // gridNodes的范围是从(0,0)开始，需要减去原点坐标得到实际位置
            _startNode = _gridNodes.GetGridNode(startPos.x - _originX, startPos.y - _originY);
            _targetNode = _gridNodes.GetGridNode(endPos.x - _originX, endPos.y - _originY);

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    var tile = GridMapManager.Instance.GetTileDetailsOnGridPosition(new Vector3Int(x + _originX,
                        y + _originY, 0));
                    // 查找属性为isNpcObstacle的格子
                    if (tile == null) continue;
                    if(tile.isNpcObstacle)
                    {
                        _gridNodes.gridNodes[x, y].isObstacle = true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 更新路径坐标
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="npcMovementSteps"></param>
        private void UpdatePathOnMovementStepStack(string sceneName, Stack<MovementStep> npcMovementSteps)
        {
            Node nextNode = _targetNode;
            while (nextNode != null)
            {
                MovementStep newStep = new()
                {
                    sceneName = sceneName,
                    gridCoordinate = new Vector2Int(nextNode.gridPosition.x + _originX,
                        nextNode.gridPosition.y + _originY),
                };
                // 压入堆栈
                npcMovementSteps.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }
}