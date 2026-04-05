using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemFader : MonoBehaviour
{
    private SpriteRenderer[] _srs;
    public EnvironmentSettings_SO envSettings;

    private void Awake()
    {
        _srs = GetComponentsInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// 淡入，恢复颜色
    /// </summary>
    public void FadeIn()
    {
        foreach (SpriteRenderer sr in _srs)
        {
            sr.DOFade(1f, envSettings.fadeDuration);
        }
    }

    /// <summary>
    /// 淡出，逐渐透明
    /// </summary>
    public void FadeOut()
    {
        foreach (SpriteRenderer sr in _srs)
        {
            sr.DOFade(envSettings.targetAlpha, envSettings.fadeDuration);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FadeOut();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FadeIn();
        }
    }
}