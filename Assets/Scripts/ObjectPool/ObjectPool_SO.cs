using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pool_Type_Name",menuName = "ObjectPool/Object Pool")]
public class ObjectPool_SO : ScriptableObject
{
    public string poolName;
    public GameObject prefab;
}
