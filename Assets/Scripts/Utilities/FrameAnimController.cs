using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameAnimController : MonoBehaviour
{
    [Header("组件获取")] 
    [SerializeField] private Sprite[] sprites;

    [Header("动画参数")] [SerializeField] private float frameRate = 0.1f;
    private float _timer;
    private int _currentIndex = 0;
    
    private event System.Action<Sprite> _updateFrameAction;
    
    void Awake()
    {
        // 自动判断组件挂载的动画对象是SpriteRenderer还是Image
        if (TryGetComponent(out SpriteRenderer sr))
        {
            _updateFrameAction = (s) =>
            {
                // 技巧：闭包，让长期存在的成员变量引用局部变量，会生成一个隐藏的类，让局部变量也长期存在
                sr.sprite = s;
            };
        }
        else if (TryGetComponent(out Image img))
        {
            _updateFrameAction = (s) =>
            {
                img.sprite = s;
            };
        }
        else
        {
            Debug.LogWarning("未找到渲染组件");
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= frameRate)
        {
            _timer -= frameRate;
            _currentIndex = (_currentIndex + 1) % sprites.Length;
            
            _updateFrameAction?.Invoke(sprites[_currentIndex]);
        }
    }
}