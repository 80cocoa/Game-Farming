using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Transition
{
    public class Teleporter : MonoBehaviour
    {
        [SerializeField] private SceneName sceneName;
        [SerializeField] private Vector3 targetPosition;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var targetScene = EnumUtils.MapToString(sceneName);
                //Debug.Log($"Enter {targetScene}");
                
                // 场景切换
                SceneTransitionManager.Instance.Transition(targetScene);
                
                // 玩家传送
                if (other.TryGetComponent<Player>(out var player))
                {
                    player.SetTeleportTarget(targetPosition);
                }
            }
        }
    }
}