using System;
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
            private static void AddDeckActions(ref List<Move> baseMoves, OmniscientBoard parent, Card.Months handMonth)
            {
                var deckMatches = parent.Field.FindAll(x => x.Monat == parent.Deck[0].Monat);
                var newMoves = new List<Move>();

                if (parent.Deck[0].Monat == handMonth)
                    return;

                if (deckMatches.Count == 2)
                {
                    foreach (Move move in baseMoves)
                    {
                        newMoves.Add(new Move(move) { DeckFieldSelection = deckMatches[0].Title });
                        move.DeckFieldSelection = deckMatches[1].Title;
                    }
                }
                baseMoves.AddRange(newMoves);
            }

            protected override object BuildChildNodes(object param)
            {
                OmniscientBoard parent = (OmniscientBoard)param;
                List<OmniscientBoard> result = new List<OmniscientBoard>();

                if (parent.isFinal)
                    return result;

                // Memo: matches = 0
                // Memo: Koikoi sagen!
                List<Card> aHand = parent.Turn ? parent.computer.Hand : parent.player.Hand;
                for (var i = 0; i < aHand.Count; i++)
                {
                    List<Move> toBuild = new List<Move>();
                    Move move = new Move();
                    move.HandSelection = aHand[i].Title;
                    move.DeckSelection = parent.Deck[0].Title;

                    List<Card> handMatches = parent.Field.FindAll(x => x.Monat == aHand[i].Monat);
                    if (handMatches.Count == 2)
                    {
                        toBuild.AddRange(new[] {
                            new Move(move) { HandFieldSelection = handMatches[0].Title },
                            new Move(move) { HandFieldSelection = handMatches[1].Title },
                        });
                    }
                    else
                        toBuild.Add(move);

                    AddDeckActions(ref toBuild, parent, aHand[i].Monat);

                    for (int build = 0; build < toBuild.Count; build++)
                    {
                        OmniscientBoard child = parent.ApplyMove(parent, toBuild[build], parent.Turn);
                        if (child.HasNewYaku)
                        {
                            child.SayKoikoi(true);
                            OmniscientBoard finalChild = parent.ApplyMove(parent, toBuild[build], parent.Turn);
                            finalChild.SayKoikoi(false);
                            result.Add(finalChild);
                        }
                        result.Add(child);
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