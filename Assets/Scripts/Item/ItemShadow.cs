using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Inventory
{
    public class ItemShadow : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer itemSprite;
        private SpriteRenderer _shadowSprite;
        
        [SerializeField] private float shadowAlpha = 0.3f;

        private void Awake()
        {
            _shadowSprite = GetComponent<SpriteRenderer>();
        }
        
        private void Start()
        {
            _shadowSprite.sprite = itemSprite.sprite;
            _shadowSprite.color = new Color(0,0,0,shadowAlpha);
        }
    }
}
