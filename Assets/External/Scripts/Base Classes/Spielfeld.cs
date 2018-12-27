using ExtensionMethods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* To-Do:
- Anfangsspieler ermitteln
- Rundenplanung
- Sammlungs-GUI (oder Einbindung)
    - GUI Kartendarstellung
- Animationen!
- Koikoi Ansagen synchronisieren
*/

namespace Hanafuda
{
    public partial class Spielfeld : MonoBehaviour
    {
        public List<Card> Deck = new List<Card>();
        public bool isFinal;
        public PlayerAction LastMove;
        public List<Card> Field = new List<Card>();
        public List<Player> players = new List<Player>();
        public float Value;
        public bool _Turn = true;
        public bool Turn
        {
            get { return _Turn; }
            set { _Turn = value; }
        }
        /// <summary>
        /// KI-Konstruktor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="move"></param>
        /// <param name="Turn"></param>
        public Spielfeld Create(Spielfeld parent, PlayerAction move, bool Turn)
        {
            //WICHTIG! Einsammeln bei Kartenzug!
            Deck.AddRange(parent.Deck);
            Field.AddRange(parent.Field);
            Value = 0f;
            players = parent.players;
            isFinal = move.isFinal();
            move.Apply();
            /*var matches = Platz.FindAll(x => move.HandSelection.Monat == x.Monat);
            switch (matches.Count)
            {
                case 0:
                    Platz.Add(move.HandSelection);
                    break;
                case 1:
                case 3:
                    for (var i = 0; i < matches.Count; i++)
                        Platz.Remove(matches[i]);
                    matches.Add(move.HandSelection);
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.AddRange(matches);
                    break;
                case 2:
                    Platz.Remove(move.fCard);
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(move.fCard);
                    ((Player)players[Turn ? 0 : 1]).CollectedCards.Add(move.HandSelection);
                    break;
            }
            ((Player)players[Turn ? 0 : 1]).Hand.Remove(move.HandSelection);
            Platz.Add(Deck[0]);
            Deck.RemoveAt(0);*/
            var Yakus = new List<Yaku>();
            Yakus = Yaku.GetYaku(((Player)players[Turn ? 0 : 1]).CollectedCards.ToList());
            var nPoints = 0;
            for (var i = 0; i < Yakus.Count; i++)
            {
                nPoints += Yakus[i].basePoints;
                if (Yakus[i].addPoints != 0)
                    nPoints += (((Player)players[Turn ? 0 : 1]).CollectedCards.Count(x => x.Typ == Yakus[i].TypPref) -
                                Yakus[i].minSize) * Yakus[i].addPoints;
            }

            if (nPoints > ((Player)players[Turn ? 0 : 1]).tempPoints)
                isFinal = true;
            else isFinal = false;
            return this;
        }

        public void RefillCards()
        {
            for (var i = 0; i < Field.Count; i++)
            {
                foreach (Transform side in Field[i].Object.transform)
                {
                    var mat = side.gameObject.GetComponent<MeshRenderer>().material;
                    mat.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, .5f));
                }
            }
        }
    }
}