using UnityEngine;
using DG.Tweening;

public class HarvestShake : MonoBehaviour
{
    [SerializeField] private Vector3 punchAmount = new Vector3(0f, 0f, 10f);
    [SerializeField] private float punchDuration = 0.5f;
    [SerializeField] private int punchVibrato = 10;// 频率：越高摇晃越剧烈
    [SerializeField] private float punchElasticity = 1f;// 弹性：1回到原点，0震动感较弱
    
    [SerializeField] private ParticleEffectType particleEffect;
    [SerializeField] private Vector3 effectPos;
    
    public void Play()
    {
        Vector3 originalRotation = transform.localEulerAngles;

        transform.DOKill();
        
        transform.DOPunchRotation(punchAmount, punchDuration, punchVibrato, punchElasticity)
            .OnComplete(() => transform.localEulerAngles = originalRotation);
        
        EventBus.RaiseSpriteShaken(particleEffect, transform.position + effectPos);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}