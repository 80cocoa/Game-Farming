using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ConfinerBoundSwitch : MonoBehaviour
{
    [SerializeField]private CinemachineConfiner2D confiner;
    
    void Start()
    {
        //SwitchConfiner();
    }

    void OnEnable()
    {
        EventBus.OnSceneChanged += SwitchConfiner;
    }

    void OnDisable()
    {
        EventBus.OnSceneChanged -= SwitchConfiner;
    }

    private void SwitchConfiner()
    {
        PolygonCollider2D targetConfiner =
            GameObject.FindGameObjectWithTag("BoundConfiner").GetComponent<PolygonCollider2D>();
        
        confiner.m_BoundingShape2D = targetConfiner;

        //改变目标边界时，清理缓存
        confiner.InvalidateCache();
    }
}