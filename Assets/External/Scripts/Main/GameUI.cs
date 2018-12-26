using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class GameUI : MonoBehaviour
    {
        private List<Player> Players;
        private void Awake()
        {
            _mobileContainerX = -maxMobileContainerX;
            Players = gameObject.GetComponent<PlayerComponent>().Players;
        }

        private void Update()
        {
            if (Global.Settings.mobile)
                UpdateMobile();
        }

        private void OnGUI()
        {
            if (Global.Settings.mobile)
                OnGUIMobile();
            else
                OnGUIPC();
        }
    }
}
