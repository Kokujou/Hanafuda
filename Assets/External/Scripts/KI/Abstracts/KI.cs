using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class KI<T> : Player, IArtificialIntelligence where T : IBoard<T>
    {
        /*
         * Berechnung der Mindestzüge über minTurn + >minturn der Karte
         * Idee für unwissende KI: Gegner legt Karte aufs Feld -> 
         *      - keine Monat der Feldkarten ist nicht in den Handkarten enthalten, und/oder
         *      - Plan ist gespielte Karte einzusammeln
         * 
         */
        public abstract Dictionary<string, float> GetWeights();
        public abstract void SetWeight(string name, float value);
        protected abstract void BuildStateTree(Spielfeld cRoot);
        public virtual Move MakeTurn(Spielfeld board)
        {
            Debug.Log("KI Turn Decision started");
            BuildStateTree(board);
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            foreach (T state in Tree.GetLevel(1))
                state.Value = RateState(state);

            //Parallel.ForEach(Tree.GetLevel(1), state => state.Value = RateState(state));
            for (var i = 0; i < Tree.GetLevel(1).Count; i++)
            {
                if (Tree.GetState(1, i).Value > maxValue)
                {
                    maxValue = Tree.GetState(1, i).Value;
                    selectedMove = Tree.GetState(1, i).LastMove;
                }
            }
            Global.Log($"Time for Enemy Turn Decision: {watch.ElapsedMilliseconds}");
            return selectedMove;
        }
        public abstract float RateState(T state);

        public IStateTree<T> Tree;

        public KI(string name) : base(name) { }
    }
    public class KI
    {
        public static Player Init(Settings.AIMode mode, string name)
        {
            switch (mode)
            {
                case Settings.AIMode.Omniscient:
                    return new OmniscientAI(name);
                case Settings.AIMode.Searching:
                    //return new SearchingAI(name);
                case Settings.AIMode.Statistic:
                    return new CalculatingAI(name);
                default:
                    return null;
            }
        }
    }
}