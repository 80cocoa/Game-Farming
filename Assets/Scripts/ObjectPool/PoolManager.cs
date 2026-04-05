using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private Transform poolParent;
    [SerializeField] private float defaultReleaseTime = 3f;

    private readonly Dictionary<string, ObjectPool<GameObject>> _poolDict = new();


    /// <summary>
    /// 创建一个对象池
    /// </summary>
    /// <param name="prefabName">对象命名规则：TYPE_DESCRIPTION_01</param>
    /// <param name="pool">ObjectPoolSO文件</param>
    public void CreatePool(ObjectPool_SO pool)
    {
        // 每个创建好的对象池，都放进各自父物体下，方便管理
        Transform parent = new GameObject(pool.name).transform;
        parent.SetParent(poolParent);

        // 为每个prefab创建对象池
        var newPool = new ObjectPool<GameObject>(
            () => Instantiate(pool.prefab, parent), // 1.creatFunc在实例化物体时调用 
            e => { e.SetActive(true); }, // 2.actionOnGet在获取物体时调用
            e => { e.SetActive(false); }, // 3.actionOnRelease在回收物体时调用
            e => Destroy(e) // 4.actionOnDestroy在销毁物体时调用
        );

        _poolDict.Add(pool.poolName, newPool);
    }

    /// <summary>
    /// 在对应位置生成指定poolObj
    /// </summary>
    /// <param name="pool">pool类型</param>
    /// <param name="pos">生成位置</param>
    public void GetPoolObject(ObjectPool_SO pool, Vector3 pos)
    {
        var currentPool = _poolDict.GetValueOrDefault(pool.poolName);
        if (currentPool == null) return;

        var obj = currentPool.Get();
        obj.transform.position = pos;

        StartCoroutine(ReleaseRoutine(currentPool, obj, defaultReleaseTime));
    }

    private IEnumerator ReleaseRoutine(ObjectPool<GameObject> pool, GameObject obj, float releaseTime)
    {
        yield return new WaitForSeconds(releaseTime);
        pool.Release(obj);
    }
}