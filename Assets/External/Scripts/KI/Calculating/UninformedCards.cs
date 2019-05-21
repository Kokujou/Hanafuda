using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public partial class CalculatingAI
    {
        public class UninformedCards : CardCollection<UninformedBoard>
        {
            Dictionary<Card.Months, uint> PPlayableMonths;
            Dictionary<Card.Months, uint> PCollectableMonths;
            Dictionary<Card.Months, uint> OppPlayableMonths;
            Dictionary<Card.Months, uint> OppCollectableMonths;

            List<Card> activeCollection;
            List<Card> opponentCollection;
            Dictionary<Card, float> activeHand;
            Dictionary<Card, float> opponentHand;
            int activeHandSize;
            int opponentHandSize;

            protected override void Preparations()
            {
                activeCollection = Turn ? State.computer.CollectedCards : State.OpponentCollection;
                opponentCollection = Turn ? State.OpponentCollection : State.computer.CollectedCards;
                activeHand = Turn ? State.computer.Hand.ToDictionary(x => x, x => 1f) : State.UnknownCards;
                opponentHand = Turn ? State.UnknownCards : State.computer.Hand.ToDictionary(x => x, x => 1f);
                activeHandSize = Turn ? State.computer.Hand.Count : State.OpponentHandSize;
                opponentHandSize = Turn ? State.OpponentHandSize : State.computer.Hand.Count;
                CalcMonths(State, Turn);
            }

            public UninformedCards(IEnumerable<CardProperties> list, UninformedBoard State, bool Turn) : base(list, State, Turn) { }

            private void CalcMonths(UninformedBoard State, bool Turn)
            {
                PPlayableMonths = new Dictionary<Card.Months, uint>(
                    Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => (uint)0));

                foreach (var pair in State.UnknownCards)
                {
                    PPlayableMonths[pair.Key.Monat]++;
                }

                OppPlayableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);

                if (Turn)
                {
                    for (int cardID = 0; cardID < State.computer.Hand.Count; cardID++)
                    {
                        Card handCard = State.computer.Hand[cardID];
                        PPlayableMonths[handCard.Monat]++;
                    }
                }

                PCollectableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);
                OppCollectableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);

                foreach (Card card in State.Field)
                    PCollectableMonths[card.Monat]++;

                foreach (Card card in State.Field)
                    OppCollectableMonths[card.Monat]++;

                PCollectableMonths = PCollectableMonths.ToDictionary(x => x.Key, x => (x.Value / 2) * 2);
                OppCollectableMonths = OppCollectableMonths.ToDictionary(x => x.Key, x => (x.Value / 2) * 2);

            }

            protected override void CalcMinTurns(UninformedBoard State, bool Turn)
            {
                /*
                 * Verstärkte Ungenauigkeit durch fehlende Zug-ID der Deck-Karten
                 * Memo: Zug aus Spielersicht, Karten der KI sind dann eigentlich auch unbekannt...
                 */
                foreach (Card card in activeCollection)
                    this[card.ID].MinTurns = 0;
                foreach (Card card in opponentCollection)
                    this[card.ID].MinTurns = -1;

                foreach (var pair in State.UnknownCards)
                {
                    if (pair.Value == 1)
                    {
                        if (OppCollectableMonths[pair.Key.Monat] == 0)
                            this[pair.Key.ID].MinTurns = 1;
                    }
                    else
                        this[pair.Key.ID].MinTurns = 1;
                }

                foreach (Card card in State.computer.Hand)
                {
                    if (Turn || OppCollectableMonths[card.Monat] == 0)
                        this[card.ID].MinTurns = 1;
                }

                foreach (Card card in State.Field)
                {
                    if (PPlayableMonths[card.Monat] > 0)
                        this[card.ID].MinTurns = 1;
                }
            }

            protected override void CalcProbs(UninformedBoard State, bool Turn)
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

                if (!Turn)
                {
                    int OppTotalMatches = opponentHand.Where(x => x.Value > 0).Select(x => x.Key.Monat).Intersect(State.Field.Select(x => x.Monat)).Count();
                    if (OppTotalMatches == 0)
                    {
                        foreach (var pair in opponentHand)
                        {
                            this[pair.Key.ID].Probability = 1f / opponentHandSize;
                            this[pair.Key.ID].Probability *= this[pair.Key.ID].RelevanceForYaku.Sum(x => x.Value) / Global.allYaku.Count;
                            this[pair.Key.ID].Probability *= CalcFieldCardProb(State, pair.Key);
                        }
                    }
                }

                foreach (var pair in State.UnknownCards)
                {
                    if (pair.Value == 1) continue;
                    Card deckCard = pair.Key;

                    List<Card> deckMatches = State.Field.FindAll(x => x.Monat == deckCard.Monat).ToList();
                    float FromFieldProb = CalcFieldCardProb(State, deckCard);

                    float AsOppDeckProb = 1;
                    if (deckMatches.Count == 0)
                        AsOppDeckProb = FromFieldProb;
                    else
                    {
                        foreach (Card card in deckMatches)
                            AsOppDeckProb *= this[card.ID].Probability;
                        AsOppDeckProb *= FromFieldProb;
                    }
                    AsOppDeckProb *= (1 - pair.Value);

                    float AsPDeckProb = 1;
                    if (activeHand.Count(x => x.Key.Monat == deckCard.Monat) > 0)
                        AsPDeckProb = 1;
                    else if (State.Field.FindAll(x => x.Monat == deckCard.Monat).Count == 2)
                        AsPDeckProb = 1;
                    else
                    {
                        AsPDeckProb = this
                            .Where(x => x.card.Monat == deckCard.Monat)
                            .Sum(x => x.Probability);
                        AsPDeckProb.Clamp(0f, 1f);
                    }

                    float AsDeckProb = ((AsOppDeckProb + AsPDeckProb) / 2f) * (1 - pair.Value);

                    if (pair.Value == 0) this[deckCard.ID].Probability = AsDeckProb;
                    else
                    {
                        float AsHandProb = 1;
                        int OppTotalMatches = opponentHand.Where(x => x.Value > 0).Select(x => x.Key.Monat).Intersect(State.Field.Select(x => x.Monat)).Count();
                        if (OppTotalMatches == 0)
                        {
                            AsHandProb = 1f / opponentHandSize;
                            AsHandProb *= this[deckCard.ID].RelevanceForYaku.Sum(x => x.Value) / Global.allYaku.Count;
                            AsHandProb *= CalcFieldCardProb(State, deckCard);
                            AsHandProb *= pair.Value;
                            this[deckCard.ID].Probability = AsHandProb + AsDeckProb;
                        }
                        else this[deckCard.ID].Probability = 0f;
                    }
                }

                if (Turn)
                {
                    foreach (Card card in State.computer.Hand)
                    {
                        float handCardProb = 1f;
                        foreach(var pair in State.UnknownCards.Where(x=>x.Key.Monat == card.Monat))
                        {
                            //Probability for being in AIs Deck Drawn Cards
                            float cardProb = (1 - pair.Value) * 0.5f;
                            handCardProb += cardProb;
                        }
                        handCardProb.Clamp(0f, 1f);

                        if (State.Field.FindAll(x => x.Monat == card.Monat).Count == 2)
                            this[card.ID].Probability = 1;
                        else
                        {
                            this[card.ID].Probability = this
                                .Where(x => x.card.Monat == card.Monat)
                                .Sum(x => x.Probability);
                            if (this[card.ID].Probability > 1)
                                this[card.ID].Probability = 1;
                            else if (handCardProb > this[card.ID].Probability)
                                this[card.ID].Probability = handCardProb;
                        }
                    }
                }
            }

            /// <summary>
            /// Calculates players probability of collecting a card, assuming it's on the field
            /// </summary>
            /// <param name="State">current Board</param>
            /// <param name="card">target card</param>
            /// <param name="source">source of card: 0 = from field, 1 = from player, 2 = from opponent</param>
            /// <returns></returns>
            private float CalcFieldCardProb(UninformedBoard State, Card card, int source = 0)
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

                int PPlayable = activeHandSize * 2 - pCorrect;
                int OppPlayable = opponentHandSize * 2 - oppCorrect;

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