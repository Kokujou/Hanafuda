using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    [RequireComponent(typeof(Spielfeld))]
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
            if (Settings.Mobile)
                UpdateMobile();
        }

        private void OnGUI()
        {
            if (Settings.Mobile)
                OnGUIMobile();
            else
                OnGUIPC();
        }
    }
}
