using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public partial class Global
    {
        public class GridLayout
        {
            public static SortedList<int, List<bool>> Toggles = new SortedList<int, List<bool>>();
            public List<List<GUIElement>> Grid = new List<List<GUIElement>>();
            public float Width, Height, Left, Top, xOffset, yOffset;

            public GridLayout(float width, float height, float left, float top, float xoffset, float yoffset)
            {
                Width = width;
                Height = height;
                xOffset = xoffset;
                yOffset = yoffset;
                Left = left - Width / 2;
                Top = top - Height / 2;
            }

            public GridLayout()
            {
            }

            public void AddRange(List<List<GUIElement>> range, int label)
            {
                for (var i = 0; i < range.Count; i++)
                for (var j = 0; j < range[i].Count; j++)
                    range[i][j].Label = label;
                Grid.AddRange(range);
            }

            public void addToLine(GUIElement element)
            {
                element.Columns = Grid.Last().Last().Columns;
                Grid.Last().Add(element);
            }

            public void addLine(GUIElement element, int columns)
            {
                element.Columns = columns;
                Grid.Add(new List<GUIElement> {element});
            }

            public void DrawLayout(bool drawBox = false, string caption = "")
            {
                if (drawBox)
                    GUI.Box(new Rect(Left, Top, Width, Height), caption);
                GUI.BeginGroup(new Rect(Left, Top, Width, Height));
                var cMax = Grid.Sum(x => x[0].Columns);
                for (var i = 0; i < Grid.Count; i++)
                {
                    var rMax = Grid[i].Sum(x => x.Rows);
                    for (var j = 0; j < Grid[i].Count; j++)
                    {
                        Grid[i][j].Width = (Width - xOffset * (Grid[i].Count + 1)) / rMax * Grid[i][j].Rows;
                        Grid[i][j].Height = (Height - 60 - yOffset * (Grid.Count + 1)) / cMax * Grid[i][j].Columns;
                        Grid[i][j].Left = xOffset + (j > 0 ? Grid[i][j - 1].Width + Grid[i][j - 1].Left : 0);
                        Grid[i][j].Top = yOffset + (i > 0 ? Grid[i - 1][0].Height + Grid[i - 1][0].Top : 60);
                        Grid[i][j].Draw();
                    }
                }

                GUI.EndGroup();
            }

            public class GUIElement
            {
                public Action Draw;
                public int Label;
                public float Left, Top, Width, Height;
                public int Rows, Columns;
                public string Text = "";

                public GUIElement(int rows, string text = "")
                {
                    Rows = rows;
                    Text = text;
                }
            }

            public class Button : GUIElement
            {
                public Action OnClick;

                public Button(int rows, Action onClick, string text = "") : base(rows, text)
                {
                    OnClick = onClick;
                    Draw = delegate
                    {
                        GUI.skin.GetStyle("Button").fontSize = (int) (Height / 1.5f);
                        if (GUI.Button(new Rect(Left, Top, Width, Height), Text)) OnClick();
                        ;
                    };
                }
            }

            public class Toggle : GUIElement
            {
                public Toggle(int rows, int layer, int id, string text = "") : base(rows, text)
                {
                    if (!Toggles.Keys.Contains(layer)) Toggles.Add(layer, new List<bool> {true});
                    else Toggles[layer].Add(false);
                    Draw = delegate
                    {
                        var mark = "";
                        if (Toggles[layer][id]) mark = "\u25CF";
                        if (GUI.Button(new Rect(Left, Top + Height / 4, Height / 2, Height / 2), mark,
                            new GUIStyle(GUI.skin.GetStyle("Button"))
                            {
                                padding = new RectOffset(0, 0, (int) (-Height / 7f), 0),
                                fontSize = (int) (Height / 1.25f)
                            }))
                            Toggles[layer][id] = true;
                        //Toggles[layer][id] = GUI.Toggle(new Rect(Left, Top, Width, Height), Toggles[layer][id], "");
                        GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleLeft;
                        GUI.Label(new Rect(Height / 2 + Left + 20, Top, Width, Height), Text);
                        if (Toggles[layer][id])
                        {
                            var count = Toggles[layer].Count;
                            Toggles[layer].Clear();
                            var set = new bool[count];
                            set[id] = true;
                            Toggles[layer].AddRange(set);
                        }
                        else
                        {
                            Toggles[layer][id] = true;
                        }
                    };
                }
            }

            public class Empty : GUIElement
            {
                public Empty(int rows, string text = "") : base(rows, text)
                {
                    Draw = delegate { };
                }
            }

            public class Label : GUIElement
            {
                private readonly bool Center;

                public Label(int rows, string text = "", bool center = false) : base(rows, text)
                {
                    Center = center;
                    Draw = delegate
                    {
                        GUI.skin.GetStyle("Label").fontSize = (int) Height / 2;
                        if (Center)
                            GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleCenter;
                        else
                            GUI.skin.GetStyle("Label").alignment = TextAnchor.MiddleLeft;
                        GUI.Label(new Rect(Left, Top, Width, Height), Text);
                    };
                }
            }

            public class TextField : GUIElement
            {
                public int maxLength = int.MaxValue;

                public TextField(int rows, int maxlength, string text = "") : base(rows, text)
                {
                    maxLength = maxlength;
                    Draw = delegate
                    {
                        GUI.skin.GetStyle("TextField").fontSize = (int) Height / 2;
                        Text = GUI.TextField(new Rect(Left, Top + Height / 4.5f, Width, Height / 1.5f), Text);
                    };
                }
            }

            public class SelectionGrid : GUIElement
            {
                private int _Selected = -1;
                public Action<int> SelectionChanged;
                public string[] Values;
                public int xCount;

                public SelectionGrid(int rows, int xcount, string[] values, string text = "",
                    Action<int> selectionChanged = null) : base(rows, text)
                {
                    SelectionChanged = selectionChanged;
                    xCount = xcount;
                    Values = values;
                    Draw = delegate
                    {
                        Selected = GUI.SelectionGrid(new Rect(Left, Top, Width, Height), Selected, Values, xCount);
                    };
                }

                public int Selected
                {
                    get { return _Selected; }
                    set
                    {
                        if (_Selected != value)
                        {
                            _Selected = value;
                            SelectionChanged?.Invoke(value);
                        }
                    }
                }
            }
        }
    }
}