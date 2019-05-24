﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/*
 * Theoretischer Zustandsbaum:
 *  - Mögliche Kombination mit unwissender mathematischer KI im ersten Zug!
 *  - Basis: Zusammenstellung aller Yaku bzgl. bekannter und unbekannter Karten
 *  - In jedem Zug: 
 *      - Berechnung aller erreichbaren Karten inklusive deren Wahrscheinlichkeit
 *          - Über Berechnung von Sammelwahrscheinlichkeit von Tupeln <Monat; 0,2,4 eingesammelt> 
 *              - Wahrscheinlichkeit der gespielten Karte eingesammelt = 1, wenn > 0
 *              - Erhöhte Wahrscheinlichkeit für alle ursprünglich auf dem feld liegenden Karten
 *                  -> Sinkende Wahrscheinlichkeit für große Baumlänge
 *          - Kombination meherer Monats-Einträge über Produktregel
 *          - Beachtung von Zügen vom Deck!
 *      - Anschließend: Berechnung der daraus resultierenden Yaku inklusive Wahrscheinlichkeit
 *      - Entgültiger Wert: Wahrscheinlichkeit eines Yaku in diesem Zug
 *          - Beanchte: Addpoints, Whkt einen Yaku mehrfach zu erzielen!
 *  - Weiterverfolgung eines Zuges, wenn:
 *      - Die neu erreichten Karten zu Yaku mit hoher Gesamtwahrscheinlichkeit gehören
 *          - Bsp: Kasu ausschließen
 *  - Qualität eines direkten Folgezuges: Gesamtwahrscheinlichkeit einen Yaku zu erzielen vor dem Gegner
 *  - Für Gegner:
 *      - Berechne Alle erreichbaren Yaku aus erreichbaren Karten
 *      - Dann berechne Monate, die mindestens in einem Yaku enthalten sind -> Mindestdauer
 *  - Zusatz: In jedem Zug: abziehen von sehr wahrscheinlichen Karten vom Gegnerpool
 *      -> Neuberechnung der Mindesdauer
 *  - In jedem Zug: Mindestdauer bis Yaku für Gegner
 *      - Gewicht verringern, wenn Eigene Yakudauer länger (?)
 *  - Nullzüge werden ignoriert, da unsinnig und unwahrscheinlich
 */

/*
 * Vollständiger Zustandsbaum:
 *  - Nutzung eines echten Spielfeldes
 *  - Vermeidung des vollständigen Aufbaus, sondern lediglich bis Tiefe 1 oder 2 (mit Gegner)
 *  
 */

namespace Hanafuda
{
    public partial class OmniscientAI
    {
        public class OmniscientStateTree : IStateTree<OmniscientBoard>
        {
            private static List<Move> AddDeckActions(List<Card> deckMatches, Move root)
            {
                List<Move> ToBuild = new List<Move>();
                if (deckMatches.Count == 2)
                {
                    for (var deckChoice = 0; deckChoice < 2; deckChoice++)
                    {
                        Move deckMove = new Move(root);
                        deckMove.DeckFieldSelection = deckMatches[deckChoice].Title;
                        ToBuild.Add(deckMove);
                    }
                }
                else
                    ToBuild.Add(root);
                return ToBuild;
            }

            protected override object BuildChildNodes(object param)
            {
                OmniscientBoard parent = (OmniscientBoard)param;
                List<OmniscientBoard> result = new List<OmniscientBoard>();
                // Memo: matches = 0
                // Memo: Koikoi sagen!
                if (!parent.isFinal)
                {
                    List<Card> aHand = parent.Turn ? parent.computer.Hand : parent.player.Hand;
                    for (var i = 0; i < aHand.Count; i++)
                    {
                        List<Move> ToBuild = new List<Move>();
                        Move move = new Move();
                        move.HandSelection = aHand[i].Title;
                        move.DeckSelection = parent.Deck[0].Title;
                        List<Card> handMatches = new List<Card>();
                        List<Card> deckMatches = new List<Card>();
                        for (int field = 0; field < parent.Field.Count; field++)
                        {
                            if (parent.Field[field].Monat == aHand[i].Monat)
                                handMatches.Add(parent.Field[field]);
                            if (parent.Field[field].Monat == parent.Deck[0].Monat)
                                deckMatches.Add(parent.Field[field]);
                        }
                        if (handMatches.Count == 2)
                        {
                            for (var handChoice = 0; handChoice < 2; handChoice++)
                            {
                                Move handMove = new Move(move);
                                handMove.HandFieldSelection = handMatches[handChoice].Title;
                                ToBuild.AddRange(AddDeckActions(deckMatches, handMove));
                            }
                        }
                        else ToBuild.AddRange(AddDeckActions(deckMatches, move));
                        for (int build = 0; build < ToBuild.Count; build++)
                        {
                            OmniscientBoard child = parent.ApplyMove(parent, ToBuild[build], parent.Turn);
                            if (child.HasNewYaku)
                            {
                                child.SayKoikoi(true);
                                OmniscientBoard finalChild = parent.ApplyMove(parent, ToBuild[build], parent.Turn);
                                finalChild.SayKoikoi(false);
                                result.Add(finalChild);
                            }
                            result.Add(child);
                        }
                    }
                }
                return result;
            }

            // Memo: Konstruktion nur für einen Spieler einbauen: Jede 2. Karte ziehen.
            public override void Build(int maxDepth = 16, bool Turn = true, bool SkipOpponent = false) => base.Build(maxDepth, Turn, SkipOpponent);

            public OmniscientStateTree(OmniscientBoard root = null, List<List<OmniscientBoard>> tree = null) : base(root, tree) { }
        }
    }
}