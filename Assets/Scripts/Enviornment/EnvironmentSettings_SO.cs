using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentSettings_SO", menuName = "Settings/EnvironmentSettingAsset")]
public class EnvironmentSettings_SO : ScriptableObject
{
    // 背景遮挡相关变量
    public float fadeDuration = 0.35f;
    public float targetAlpha = 0.45f;
}