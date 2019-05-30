using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public partial class OmniscientAI
    {
        public class OmniscientCards : CardCollection<OmniscientBoard>
        {
            Dictionary<Card.Months, uint> PPlayableMonths;
            Dictionary<Card.Months, uint> PCollectableMonths;
            Dictionary<Card.Months, uint> OppPlayableMonths;
            Dictionary<Card.Months, uint> OppCollectableMonths;

            Player player;
            Player opponent;

            protected override void Preparations()
            {
                player = Turn ? State.computer : State.player;
                opponent = Turn ? State.player : State.computer;
                CalcMonths(State, Turn);
            }

            public OmniscientCards(IEnumerable<CardProperties> list, OmniscientBoard State, bool Turn) : base(list, State, Turn) { }

            private void CalcMonths(OmniscientBoard State, bool Turn)
            {
                PPlayableMonths = new Dictionary<Card.Months, uint>(
                    Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => (uint)0));
                OppPlayableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);

                for (int cardID = 0; cardID < opponent.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2 + 1];
                    Card handCard = opponent.Hand[cardID];
                    OppPlayableMonths[handCard.Monat]++;
                    OppPlayableMonths[deckCard.Monat]++;
                }

                for (int cardID = 0; cardID < player.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2];
                    Card handCard = player.Hand[cardID];
                    PPlayableMonths[handCard.Monat]++;
                    PPlayableMonths[deckCard.Monat]++;
                }

                PCollectableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);
                OppCollectableMonths = new Dictionary<Card.Months, uint>(OppPlayableMonths);

                foreach (Card card in State.Field)
                    PCollectableMonths[card.Monat]++;

                foreach (Card card in State.Field)
                    OppCollectableMonths[card.Monat]++;

                PCollectableMonths = PCollectableMonths.ToDictionary(x => x.Key, x => (x.Value / 2) * 2);
                OppCollectableMonths = OppCollectableMonths.ToDictionary(x => x.Key, x => (x.Value / 2) * 2);

            }

            protected override void CalcMinTurns(OmniscientBoard State, bool Turn)
            {
                /*
                 * Verbesserung: Bessere Approximation für Einsammelnbare Karten
                 */
                foreach (Card card in player.CollectedCards)
                    this[card.ID].MinTurns = 0;
                foreach (Card card in opponent.CollectedCards)
                    this[card.ID].MinTurns = -1;

                for (int cardID = 0; cardID < player.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2];
                    Card handCard = player.Hand[cardID];
                    this[deckCard.ID].MinTurns = cardID + 1;
                    this[handCard.ID].MinTurns = 1;
                }

                foreach (Card card in State.Field)
                {
                    if (PPlayableMonths[card.Monat] > 0)
                        this[card.ID].MinTurns = 1;
                }

                //Gegnerische Karten werden aufs Feld gelegt, wenn kein Match existiert
                for (int cardID = 0; cardID < opponent.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2 + 1];
                    Card handCard = opponent.Hand[cardID];
                    if (OppCollectableMonths[handCard.Monat] == 0)
                        this[handCard.ID].MinTurns = 1;
                    this[deckCard.ID].MinTurns = cardID + 1;
                }
            }

            protected override void CalcProbs(OmniscientBoard State, bool Turn)
            {
                /*
                 *  Beschreibung:
                 *  - Alle eingesammelten Karten haben Wahrscheinlichkeit 1
                 *  - Die Wahrscheinlichkeit von Feldkarten ergibt sich über virtuellen Wettbewerb mit dem Gegner
                 *      - Wahrscheinlichkeit die Karte einzusammeln durch auszählen: Matches mit Karte / alle Matches
                 *      - Wettbewerb: Gesamtwhkt = (1- Einsammelwhkt. des Gegners) * Einsammelwhkt. des Spielers
                 *  - Whkt. der Karten auf Hand und gezogen vom Deck:
                 *      - Wenn Karte unter eigenen Kontrolle -> Whkt = 1 (Match in Hand oder Deck)
                 *      - Sonst: Addition aller Match-Wahrscheinlichkeiten
                 *      - -> Berechnung nach allen anderen, damit Whkt aufgebaut werden kann
                 *      - Sonderfall des Einsammelns einer Handkarte durch andere Handkarte wird ignoriert
                 *  - Whkt. der Karten kommt nur zu Tragen wenn der Gegner keine anderen Möglichkeiten hat (=Verzweiflungszug)
                 *      - In dem Fall Zufallshochschicken von Karten
                 *      - Wichtung möglich durch Relevanz für Yaku, optional gewichtet mit Punkten der Yaku
                 *      - Wichtung mit relevanten Yaku paradox, da zur Ermittlung selbiger die Kartenwahrscheinlichkeiten nötig sind -> Näherungswert?
                 *  - Gegnerisches Ziehen vom Deck ist unumstößlich -> Sichere Wahrscheinlichkeit
                 *  - Whkt der gegnerischen vom Deck gezogenen Karten: Whkt. des "Wegschnappens" durch Spieler * Whkt. des Einsammelns
                 *  - Memo: fehlende Wichtung durch neue Methode
                 */
                foreach (CardProperties card in this)
                {
                    if (card.MinTurns == 0)
                        card.Probability = 1;
                    else if (card.MinTurns < 0)
                        card.Probability = 0;
                }

                foreach (Card card in State.Field)
                    this[card.ID].Probability = CalcFieldCardProb(State, card);

                int PTotalMatches = opponent.Hand.Select(x => x.Monat).Intersect(State.Field.Select(x => x.Monat)).Count();
                if (PTotalMatches == 0)
                {
                    foreach (Card card in opponent.Hand)
                    {
                        this[card.ID].Probability = 1f / opponent.Hand.Count;
                        this[card.ID].Probability *= this[card.ID].RelevanceForYaku.Sum(x => x.Value) / Global.allYaku.Count;
                        this[card.ID].Probability *= CalcFieldCardProb(State, card);
                    }
                }

                for (int cardID = 1; cardID < opponent.Hand.Count * 2; cardID += 2)
                {
                    Card deckCard = State.Deck[cardID];

                    List<Card> deckMatches = State.Field.FindAll(x => x.Monat == deckCard.Monat).ToList();
                    float FromFieldProb = CalcFieldCardProb(State, deckCard);
                    if (deckMatches.Count == 0)
                        this[deckCard.ID].Probability = FromFieldProb;
                    else
                    {
                        float prob = 1;
                        foreach (Card card in deckMatches)
                            prob *= this[card.ID].Probability;
                        this[deckCard.ID].Probability = prob * FromFieldProb;
                    }
                }

                List<Card> playerDeck = new List<Card>();
                for (int cardID = 2; cardID < player.Hand.Count * 2; cardID += 2)
                    playerDeck.Add(State.Deck[cardID]);
                foreach (Card card in player.Hand)
                {
                    if (playerDeck.Exists(x => x.Monat == card.Monat))
                        this[card.ID].Probability = 1;
                    else if (State.Field.FindAll(x => x.Monat == card.Monat).Count == 2)
                        this[card.ID].Probability = 1;
                    else
                    {
                        float prob = 1f;
                        foreach (CardProperties month in this.Where(x => x.card.Monat == card.Monat))
                            prob *= (1 - month.Probability);
                        this[card.ID].Probability = 1 - prob;
                    }
                }
                foreach (Card card in playerDeck)
                {
                    if (player.Hand.Exists(x => x.Monat == card.Monat))
                        this[card.ID].Probability = 1;
                    else if (State.Field.FindAll(x => x.Monat == card.Monat).Count == 2)
                        this[card.ID].Probability = 1;
                    else
                    {
                        float prob = 1f;
                        foreach (CardProperties month in this.Where(x => x.card.Monat == card.Monat))
                            prob *= (1 - month.Probability);
                        this[card.ID].Probability = 1 - prob;
                    }
                }
            }

            /// <summary>
            /// Calculates players probability of collecting a card, considering it's on the field
            /// </summary>
            /// <param name="State">current Board</param>
            /// <param name="card">target card</param>
            /// <param name="source">source of card: 0 = from field, 1 = from player, 2 = from opponent</param>
            /// <returns></returns>
            private float CalcFieldCardProb(OmniscientBoard State, Card card, int source = 0)
            {
                float result = 0;

                int pCorrect = 0;
                int oppCorrect = 0;
                switch (source)
                {
                    case 1:
                        pCorrect = -1;
                        break;
                    case 2:
                        oppCorrect = -1;
                        break;
                    default: break;
                }

                int PCardMatches = (int)PPlayableMonths
                    .Where(x => x.Key == card.Monat)
                    .Sum(x => x.Value) - pCorrect;
                int OppCardMatches = (int)OppPlayableMonths
                    .Where(x => x.Key == card.Monat)
                    .Sum(x => x.Value) - oppCorrect;

                int PPlayable = player.Hand.Count * 2 - pCorrect;
                int OppPlayable = opponent.Hand.Count * 2 - oppCorrect;

                if (OppPlayable > 0)
                {
                    result = OppCardMatches / (float)OppPlayable;
                    result *= this[card.ID].RelevanceForYaku.Sum(x => x.Value) / Global.allYaku.Count;
                }
                result = 1 - result;

                if (PPlayable > 0)
                {
                    result *= PCardMatches / (float)PPlayable;
                }
                else result = 0;

                return result;
            }
        }
    }
}