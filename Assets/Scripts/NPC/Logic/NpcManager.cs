using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.NPC
{
    public class NpcManager : Singleton<NpcManager>
    {
        [SerializeField]private SceneRouteDataList_SO routeData;
        private readonly Dictionary<(string fromScene, string toScene), SceneRoute> _sceneRouteDict = new();
        
        [SerializeField]private List<NpcPosition> npcPositionList = new();

        protected override void Awake()
        {
            base.Awake();
            InitSceneRouteDict();    
        }

        private void InitSceneRouteDict()
        {
            foreach (var route in routeData.routes)
            {
                var key = (EnumUtils.MapToString(route.fromScene), EnumUtils.MapToString(route.toScene));
                _sceneRouteDict.TryAdd(key, route);
            }
        }

        /// <summary>
        /// 获得两个场景间的路径
        /// </summary>
        /// <param name="fromScene">当前场景</param>
        /// <param name="toScene">目标场景</param>
        /// <returns></returns>
        public SceneRoute GetSceneRoute(string fromScene, string toScene)
        {
            return  _sceneRouteDict[(fromScene, toScene)];
        }
    }
}
