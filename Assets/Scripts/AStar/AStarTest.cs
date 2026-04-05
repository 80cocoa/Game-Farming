using System.Collections;
using System.Collections.Generic;
using MyGame.NPC;
using MyGame.Transition;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyGame.AStar
{
    public class AStarTest : MonoBehaviour
    {
        public Vector2Int startPos;
        public Vector2Int endPos;

        public Tilemap displayMap;
        public TileBase displayTile;

        public bool displayPath;

        private Stack<MovementStep> _movementSteps = new();

        [Header("测试NPC移动")] public NpcMovement npcMovement;
        public bool triggerNpcMovement;
        public SceneName targetScene;
        public Vector2Int targetPos;
        public AnimationClip stopClip;


        private void Awake()
        {
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                ShowPathOnGridMap();
            }

            if (triggerNpcMovement)
            {
                triggerNpcMovement = false;
                var schedule = new NpcScheduleDetails(0, 0, 0, 0, targetScene, targetPos, stopClip, true);
                npcMovement.BuildPath(schedule);
            }
        }

        private void ShowPathOnGridMap()
        {
            if (displayMap && displayTile)
            {
                displayMap.SetTile((Vector3Int)startPos, displayTile);
                displayMap.SetTile((Vector3Int)endPos, displayTile);
            }
            else
            {
                displayMap.SetTile((Vector3Int)startPos, null);
                displayMap.SetTile((Vector3Int)endPos, null);
            }

            if (displayPath)
            {
                var sceneName = SceneTransitionManager.Instance.CurrentSceneName;

                AStar.Instance.BuildPath(sceneName, startPos, endPos, _movementSteps);

                foreach (var step in _movementSteps)
                {
                    displayMap.SetTile((Vector3Int)step.gridCoordinate, displayTile);
                }
            }
            else
            {
                if (_movementSteps.Count > 0)
                {
                    foreach (var step in _movementSteps)
                    {
                        displayMap.SetTile((Vector3Int)step.gridCoordinate, null);
                    }

                    _movementSteps.Clear();
                }
            }
        }
    }
}