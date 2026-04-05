using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        [SerializeField] private Transform spriteTrans;
        private Collider2D _coll;

        [SerializeField] private float gravity = -1f;
        [SerializeField] private float bounceSpeed = 1f;
        private float _verticalVelocity;

        [SerializeField] private float holdItemHeight = 1.5f;
        private float _distance;
        private Vector2 _direction;
        private Vector3 _targetPos;

        private bool _isOnGround;
        private bool _isStopped;


        private void Awake()
        {
            _coll = GetComponent<Collider2D>();
            _coll.enabled = false;
        }

        private void Update()
        {
            if (!_isStopped || !_isOnGround)
                Bounce();
            else
                _coll.enabled = true;
        }

        public void InitBounceItem(Vector3 targetPos)
        {
            _coll.enabled = false;
            _direction = (targetPos - transform.position).normalized;
            _targetPos = targetPos;
            _distance = Vector3.Distance(transform.position, targetPos);

            spriteTrans.position = transform.position + Vector3.up * holdItemHeight;
        }

        private void Bounce()
        {
            _isOnGround = spriteTrans.position.y <= transform.position.y;

            if (!_isOnGround)
            {
                _verticalVelocity += gravity * Time.deltaTime;
                spriteTrans.position += Vector3.up * (_verticalVelocity * Time.deltaTime);
            }
            else
            {
                _verticalVelocity = 0;
            }

            _isStopped = !(Vector3.Distance(transform.position, _targetPos) > 0.1f);
            
            if (!_isStopped)
            {
                transform.position += (Vector3)_direction * (bounceSpeed * _distance * Time.deltaTime);
            }
            
        }
    }
}