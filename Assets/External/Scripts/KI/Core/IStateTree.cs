using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ExtensionMethods;

namespace Hanafuda
{
    public abstract class IStateTree<T> where T : IBoard<T>
    {
        public int MaxDepth;
        public bool StartTurn;
        public bool SkipOpponent;

        public T Root;
        public int Size => Content.Count;
        public List<T> GetLevel(int id) => Content[id];
        public T GetState(int x, int y) => Content[x][y];

        protected List<List<T>> Content = new List<List<T>>(9);

        protected readonly List<Task<object>> tasks = new List<Task<object>>();

        [Serializable]
        protected class NodeParameters
        {
            public int level;
            public int node;
            public bool turn;
        }

        [Serializable]
        protected class NodeReturn
        {
            public int level;
            public bool turn;
            public List<T> states = new List<T>();
        }

        /// <summary>
        /// Funktion zum Implementieren des Aufbaus einzelner Zustände -> Konstruktion eines Folgezustands
        /// </summary>
        /// <param name="param">Serialisierte Instanz der NodeParameter-Klasse</param>
        /// <returns>Serialisierte Instanz der Note-Return-Klasse</returns>
        protected abstract object BuildChildNodes(object param);
        

        /// <summary>
        /// Prototyp für Koordinierung des Multithreadings
        /// </summary>
        /// <param name="maxDepth">Maximale Tiefe des Suchbaums</param>
        /// <param name="Turn">Gibt an, ob die KI am Zug ist</param>
        /// <param name="SkipOpponent">Gibt an, ob sich der Spieler während der Berechnung ändert</param>
        public virtual void Build(int maxDepth = 16, bool Turn = true, bool skipOpponent = false)
        {
            MaxDepth = maxDepth;
            StartTurn = Turn;
            SkipOpponent = skipOpponent;

            Root.Turn = StartTurn;

            Content = new List<List<T>>(9);
            Content.Add(new List<T> { Root });

            Content.Add(new List<T>());
            List<T> firstResult = (List<T>)BuildChildNodes(Root.Clone());
            Content[1].AddRange(firstResult);

            if (maxDepth <= 1) return;

            for (int taskID = 0; taskID < firstResult.Count; taskID++)
            {
                firstResult[taskID].Root = taskID;
                object input = firstResult[taskID].Clone();
                Task<object> newTask = new Task<object>(DeepConstruction, input, TaskCreationOptions.LongRunning );
                newTask.Start();
                tasks.Add(newTask);
            }
            while (tasks.Count > 0)
            {
                int taskID = Task.WaitAny(tasks.ToArray());
                List<List<T>> results = (List<List<T>>)tasks[taskID].Result;
                for (int level = 0; level < results.Count; level++)
                {
                    if (results[level].Count == 0) break;
                    if (level + 2 >= Content.Count)
                        Content.Add(new List<T>());
                    Content[level + 2].AddRange(results[level]);
                }
                tasks.RemoveAt(taskID);
            }
        }

        public object DeepConstruction(object param)
        {
            List<List<T>> stateTree = new List<List<T>>();
            int level = 0;
            int node = 0;

            stateTree.Add(new List<T>());
            stateTree[level].AddRange((List<T>)BuildChildNodes(param));
            stateTree.Add(new List<T>());

            while (true)
            {
                if (node >= stateTree[level].Count)
                {
                    if (stateTree[level + 1].Count == 0 || level + 1 >= (MaxDepth-2))
                        break;
                    level++;
                    node = 0;
                    stateTree.Add(new List<T>());
                }
                object anonparam = stateTree[level][node];
                object anonResult = BuildChildNodes(anonparam);
                List<T> results = (List<T>)anonResult;
                stateTree[level + 1].AddRange(results);
                node++;
            }

            return stateTree;
        }

        public IStateTree(T root = null, List<List<T>> tree = null)
        {
            if (root != null)
                Root = root;
            if (tree != null)
                Content = tree;
        }
    }
}
