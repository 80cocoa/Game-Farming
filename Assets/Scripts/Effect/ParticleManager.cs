using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleManager : Singleton<ParticleManager>
{
    [SerializeField] private List<ObjectPool_SO> particlePrefabList;

    private void Start()
    {
        CreatePool();
    }

    private void OnEnable()
    {
        EventBus.OnSpriteShaken += OnSpriteShaken;
    }

    private void OnDisable()
    {
        EventBus.OnSpriteShaken -= OnSpriteShaken;
    }

    private void OnSpriteShaken(ParticleEffectType type, Vector3 pos)
    {
        ActivateParticle(type, pos);
    }

    /// <summary>
    /// 生成对象池
    /// </summary>
    private void CreatePool()
    {
        foreach (var prefabs in particlePrefabList)
        {
            PoolManager.Instance.CreatePool(prefabs);
        }
    }

    private void ActivateParticle(ParticleEffectType type, Vector3 position)
    {
        var prefab = type switch
        {
            ParticleEffectType.LeavesFalling01 => particlePrefabList[0],
            // TODO：补全粒子对应prefab
            _ => null
        };

        if (!prefab) return;
        PoolManager.Instance.GetPoolObject(prefab, position);
    }
}