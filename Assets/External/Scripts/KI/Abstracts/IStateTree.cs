using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public abstract class IStateTree<T> where T : IBoard<T>
    {
        public T Root;
        public int Size => Content.Count;
        public List<T> GetLevel(int id) => Content[id];
        public T GetState(int x, int y) => Content[x][y];

        protected List<List<T>> Content = new List<List<T>>();

        public static object thisLock;
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
        public virtual void Build(int maxDepth = 16, bool Turn = true, bool SkipOpponent = false)
        {
            Content.Clear();
            Content.Add(new List<T> { Root });
            for (var i = 0; i < maxDepth; i++)
                Content.Add(new List<T>());
            Task<object> firstTask = new Task<object>(x => BuildChildNodes(x), (new NodeParameters() { level = 0, node = 0, turn = Turn }));
            firstTask.Start();
            tasks.Add(firstTask);
            while (tasks.Count > 0 && Content.Last().Count == 0)
            {
                Task.WaitAny(tasks.ToArray());
                for (int task = tasks.Count - 1; task >= 0; task--)
                {
                    if (tasks[task].IsCompleted)
                    {
                        NodeReturn result = (NodeReturn)tasks[task].Result;
                        tasks.RemoveAt(task);
                        Content[result.level + 1].AddRange(result.states);
                        if (result.level + 1 >= maxDepth) continue;
                        for (int i = 0; i < result.states.Count; i++)
                        {
                            Task<object> newTask = new Task<object>(x => BuildChildNodes(x), (object)new NodeParameters() { level = result.level + 1, node = Content[result.level + 1].Count - (i + 1), turn = SkipOpponent ? Turn : !result.turn });
                            newTask.Start();
                        }
                    }
                }
            }
        }

        public IStateTree(T root = null, List<List<T>> tree = null)
        {
            thisLock = new object();
            if (root != null)
                Root = root;
            if (tree != null)
                Content = tree;
        }
    }
}
