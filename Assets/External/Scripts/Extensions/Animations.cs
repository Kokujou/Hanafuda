using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hanafuda;


namespace ExtensionMethods
{
    public static class Animations
    {
        public const float _minBlinkAlpha = 0.5f;
        public const float _maxBlinkAlpha = .75f;
        public const float _OrthoCamSize = 41f;
        public const float _PlaneSize = 5f;
        public const float _OrthoPlaneSize = (_OrthoCamSize * 2f) / _PlaneSize;
        public const float _CardSize = _OrthoPlaneSize / 1.6f;
        public const float _CardAngle = 20f;
        public static readonly Vector3 StandardScale = new Vector3(1, 1.6f, 1);
        /// <summary>
        /// Transparenz-Animation des Hilfs-Pfeils für die mobile Handkarten-Animation
        /// </summary>
        /// <returns></returns>
        public static IEnumerator BlinkSlide(this GameObject obj)
        {
            while (true)
            {
                var renderer = obj.GetComponent<SpriteRenderer>();
                while (renderer.color.a == 0)
                    yield return null;
                var alpha = Time.time * 3f % 5f / 10f + _minBlinkAlpha;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b,
                    alpha > _maxBlinkAlpha ? _maxBlinkAlpha - (alpha - _maxBlinkAlpha) : alpha);
                yield return null;
            }
        }
        /// <summary>
        /// Hervorheben der Karten. Mobil: Verschiebung, PC: Skalierung
        /// </summary>
        /// <param name="unhover">Rückgängigmachen des Hovers</param>
        public static void HoverCard(this BoxCollider col, bool unhover = false)
        {
            if (!col) return;
            int factor = unhover ? -1 : 1;
            if (Settings.Mobile)
            {
                var tempZ = col.gameObject.transform.position.z;
                col.gameObject.transform.Translate(0, factor * 10, 0);
                col.gameObject.transform.position = new Vector3(col.gameObject.transform.position.x,
                    col.gameObject.transform.position.y, tempZ);
            }
            else
            {
                col.gameObject.transform.position -= factor * new Vector3(0, 0, 5);
                col.gameObject.transform.localScale *= Mathf.Pow(2, factor);
                col.size /= Mathf.Pow(2, factor);
            }
            Global.prev = unhover ? null : col;
        }

        /// <summary>
        /// Skalierungs-Animation des Koikoi-Schriftzugs
        /// </summary>
        /// <param name="append">Aktion nach Abschluss der Animation</param>
        /// <returns></returns>
        public static IEnumerator KoikoiAnimation(this GameObject toInstantiate, Action append)
        {
            GameObject koi = GameObject.Instantiate(toInstantiate);
            koi.transform.position = Vector3.zero;
            koi.transform.localScale = Vector3.zero;
            Vector3 destScale;
            if (Settings.Mobile)
                destScale = new Vector3(5f, 2.4f, 2.4f);
            else
                destScale = new Vector3(9.6f, 2.4f, 2.4f);
            yield return koi.transform.StandardAnimation(koi.transform.position, koi.transform.eulerAngles, destScale);
            yield return koi.transform.StandardAnimation(koi.transform.position, koi.transform.eulerAngles, Vector3.zero, 1.5f, AddFunc: () =>
            {
                GameObject.Destroy(koi);
                append();
            });

        }
        /// <summary>
        ///     Standardmäßige Animation von Objekten durch Interpolation
        /// </summary>
        /// <param name="destPos">Zielposition</param>
        /// <param name="destRot">Zielrotation</param>
        /// <param name="destScale">Zielskalierung</param>
        /// <param name="delay">Verzögerung vor Beginn der Aniamtion</param>
        /// <param name="AddFunc">Aktion nach Ende der Animation</param>
        /// <returns></returns>
        public static IEnumerator StandardAnimation(this Transform obj, Vector3 destPos, Vector3 destRot, Vector3 destScale,
            float delay = 0f, float duration = 1f, params Action[] AddFunc)
        {
            //destPos.z = obj.position.z;
            Global.MovingCards++;
            yield return new WaitForSeconds(delay);
            var startTime = Time.time;
            Vector3 startPos = obj.position, startRot = obj.rotation.eulerAngles, startScale = obj.localScale;
            while ((obj.position != destPos || obj.localScale != destScale || obj.rotation.eulerAngles != destRot) &&
                   Time.time - startTime < duration)
            {
                var elapsed = Time.time - startTime;
                if (obj.position != destPos)
                    if (Vector3.Distance(obj.position, destPos) <
                        Vector3.Distance(startPos + (destPos - startPos) / (duration / elapsed), destPos))
                        obj.position = destPos;
                    else
                        obj.position = startPos + (destPos - startPos) / (duration / elapsed);
                obj.rotation = Quaternion.Slerp(Quaternion.Euler(startRot), Quaternion.Euler(destRot),
                    elapsed / duration > 1 ? 1 : elapsed / duration);
                if (obj.localScale != destScale)
                    if (Vector3.Distance(obj.localScale, destScale) <
                        Vector3.Distance(startScale + (destScale - startScale) / (duration / elapsed), destScale))
                        obj.localScale = destScale;
                    else
                        obj.localScale = startScale + (destScale - startScale) / (duration / elapsed);
                yield return new WaitForSeconds(.0f);
            }
            obj.position = destPos;
            obj.rotation = Quaternion.Euler(destRot);
            obj.localScale = destScale;
            for (var i = 0; i < AddFunc.Length; i++)
                AddFunc[i]();
            Global.MovingCards--;
        }
        /// <summary>
        /// Sortieren von Eltern-Objekten der Karten-Sammlungen
        /// </summary>
        /// <param name="toSort">zu sortierende Sammlung</param>
        /// <param name="StartPos">Startposition der Sammlung</param>
        /// <param name="rows">Anzahl der Zeilen, auf die die Karten aufgeteilt werden sollen</param>
        /// <param name="maxCols">Maximale Anzahl von Spalten</param>
        /// <returns></returns>
        /// 
        public static IEnumerator ResortCards(this List<Card> toSort, CardLayout layout)
        {
            Vector3 StartPos = toSort[0].Object.transform.parent.position;
            yield return new WaitForSeconds(layout.Delay);
            if (layout.IsMobileHand)
                yield return SortMobileHand(toSort);
            else
                yield return SortCollection(toSort, layout.MaxSize, layout.RowWise, StartPos);
        }

        private static IEnumerator SortCollection(List<Card> toSort, int maxSize, bool rowWise, Vector3 StartPos)
        {
            int iterations = maxSize;
            for (int i = 0; i < toSort.Count; i++)
            {
                float offsetX = toSort[i].Object.transform.localScale.x;
                float offsetY = toSort[i].Object.transform.localScale.y;
                float cardWidth = _CardSize * offsetX;
                float cardHeight = _CardSize * offsetY;
                float alignY = (cardHeight + offsetY) * ((maxSize - 1) * 0.5f);
                if (rowWise)
                    Global.instance.StartCoroutine(toSort[i].Object.transform.StandardAnimation(StartPos +
                        new Vector3((i % iterations) * (cardWidth + offsetX), -alignY + (i / iterations) * (cardHeight + offsetY), 0),
                        toSort[i].Object.transform.rotation.eulerAngles, toSort[i].Object.transform.localScale, 1f / toSort.Count, .5f));
                else
                    Global.instance.StartCoroutine(toSort[i].Object.transform.StandardAnimation(StartPos +
                    new Vector3((i / iterations) * (cardWidth + offsetX), -alignY + (i % iterations) * (cardHeight + offsetY), 0),
                    toSort[i].Object.transform.rotation.eulerAngles, toSort[i].Object.transform.localScale, 1f / toSort.Count, .5f));
                yield return null;
            }
        }

        private static IEnumerator SortMobileHand(List<Card> toSort)
        {
            for (int card = 0; card < toSort.Count; card++)
            {
                GameObject temp = toSort[card].Object;
                bool hand1 = temp.transform.parent.name.Contains("1");
                Global.instance.StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position, new Vector3(0, temp.transform.rotation.eulerAngles.y, hand1 ? 0 : 180), temp.transform.localScale, 0, .3f, () =>
                {
                    GameObject Card = new GameObject();
                    Card.transform.parent = temp.transform.parent;
                    temp.transform.parent = Card.transform;
                    Card.transform.localPosition = new Vector3(0, hand1 ? -8 : 8);
                    temp.transform.localPosition = new Vector3(0, hand1 ? 8 : -8, 0);
                    List<Transform> hand = new List<Transform>(temp.transform.parent.parent.gameObject.GetComponentsInChildren<Transform>());
                    hand.RemoveAll(x => !x.name.Contains("New"));
                    int id = hand.IndexOf(temp.transform.parent);
                    float max = toSort.Count - 1;
                    if (max == 0) max = 0.5f;
                    Global.instance.StartCoroutine(temp.transform.parent.StandardAnimation(temp.transform.parent.position + new Vector3(0, 0, -id), temp.transform.parent.eulerAngles + new Vector3(0, 0, -(_CardAngle * max * 0.5f) + _CardAngle * (max - id)), temp.transform.parent.localScale, .6f, .3f, () =>
                         {
                             GameObject oldParent = temp.transform.parent.gameObject;
                             temp.transform.parent = temp.transform.parent.parent;
                             GameObject.Destroy(oldParent);
                        //temp.transform.localPosition = new Vector3(temp.transform.localPosition.x, temp.transform.localPosition.y, id/10f);
                    }));
                }));
                yield return null;
            }
        }

        public static IEnumerator AfterAnimation(Action action)
        {
            while (Global.MovingCards > 0)
            {
                yield return null;
            }
            action();
        }

        public static IEnumerator CoordinateQueue(this List<Action> actions)
        {
            bool actionRunning = Global.MovingCards > 0;
            int actionIndex = -1;
            while (true)
            {
                if (!actionRunning)
                {
                    actionIndex++;
                    if (actionIndex >= actions.Count)
                        break;
                    else
                    {
                        actionRunning = true;
                        actions[actionIndex]();
                    }
                }
                else if (Global.MovingCards == 0)
                    actionRunning = false;
                yield return null;
            }
        }
    }
}