using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;

namespace Hanafuda
{
    public class NumericUpDown : MonoBehaviour
    {
        public uint Maximum, Minimum;
        public Button Increment, Decrement;
        public Text Content;

        private uint _Value;
        public uint Value
        {
            get => _Value;
            private set
            {
                _Value = value.Clamp(Minimum, Maximum);
                UpdateText(_Value);
            }
        }

        public void UpdateText(uint value)
        {
            Content.text = $"{value}x";
        }

        void Start()
        {
            Increment.onClick.AddListener(() => Value++);
            Decrement.onClick.AddListener(() => Value--);
            Value = Minimum;
        }
    }
}