using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Inventory 
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarHotKey : MonoBehaviour
    {
        [SerializeField]private KeyCode key;
        private SlotUI _slotUI;

        private void Awake()
        {
            _slotUI = GetComponent<SlotUI>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                _slotUI.HandleClickEvent();
            }
        }
    }
}
