using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName =  "Npc/SceneRouteDataList",fileName = "SceneRouteDataList")]
public class SceneRouteDataList_SO : ScriptableObject
{
    public List<SceneRoute> routes;
}
