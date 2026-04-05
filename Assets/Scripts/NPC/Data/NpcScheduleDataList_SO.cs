using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Npc/NpcScheduleList_NPC", fileName = "NpcScheduleDataList")]
public class NpcScheduleDataList_SO : ScriptableObject
{
    public List<NpcScheduleDetails> ScheduleList = new();
}
