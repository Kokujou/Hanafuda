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
        protected List<CardProperties> CardProps = new List<CardProperties>();
        public abstract Dictionary<string, float> GetWeights();
        public abstract void SetWeight(string name, float value);
        protected abstract void BuildStateTree(IHanafudaBoard cRoot, int playerID);
        public virtual Move MakeTurn(IHanafudaBoard board, int playerID)
        {
            Debug.Log("KI Turn Decision started");
            BuildStateTree(board, playerID);
            //Bewertung möglicherweise in Threads?
            float maxValue = float.NegativeInfinity;
            Move selectedMove = null;
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            List<T> FirstLevel = Tree.GetLevel(1);
            foreach (T state in FirstLevel)
                state.Value = RateState(state);

            //Parallel.ForEach(Tree.GetLevel(1), state => state.Value = RateState(state));

            foreach (T state in FirstLevel)
            {
                if (state.Value > maxValue)
                {
                    maxValue = state.Value;
                    selectedMove = state.LastMove;
                }
            }
            Global.Log($"Time for Enemy Turn Decision: {watch.ElapsedMilliseconds}");
            return selectedMove;
        }
        public abstract float RateState(T state);

        public abstract Move RequestDeckSelection(Spielfeld board, Move baseMove, int playerID);

        public IStateTree<T> Tree;

        public KI(string name) : base(name)
        {
            for (int i = 0; i < Global.allCards.Count; i++)
            {
                CardProps.Add(new CardProperties(i));
            }
        }
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
                    return new SearchingAI(name);
                case Settings.AIMode.Statistic:
                    return new CalculatingAI(name);
                case Settings.AIMode.Random:
                    return new RandomAI(name);
                default:
                    return null;
            }
        }
    }
}