using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UISettings", menuName = "Settings/UISettings")]
public class UISettings_SO : ScriptableObject
{
    #region 背包相关

    public float tooltipOffsetMultiplier = 5f;

    #endregion
    
    #region 转场相关
    
    public float fadeOutDuration = 0.5f;
    public float fadeInDuration = 0.5f;
    public float fadeDuration = 1.5f;
    
    #endregion
}
