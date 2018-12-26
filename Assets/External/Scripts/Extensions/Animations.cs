using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hanafuda;


namespace ExtensionMethods
{
    public static class Animations
    {
        public const float minBlinkAlpha = 0.5f;
        public const float maxBlinkAlpha = .75f;
        public const float OrthoCamSize = 41f;
        public const float PlaneSize = 5f;
        public const float OrthoPlaneSize = (OrthoCamSize * 2f) / PlaneSize;
        public static float CardSize { get { return OrthoPlaneSize * Camera.main.aspect; } }
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
                var alpha = Time.time * 3f % 5f / 10f + minBlinkAlpha;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b,
                    alpha > maxBlinkAlpha ? maxBlinkAlpha - (alpha - maxBlinkAlpha) : alpha);
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
            if (Global.Settings.mobile)
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
            yield return koi.transform.StandardAnimation(koi.transform.position, koi.transform.eulerAngles, new Vector3(9.6f, 2.4f, 2.4f));
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
        public static IEnumerator ResortCards(this List<Card> toSort, int maxSize, bool isMobileHand = false, bool rowWise = true, float delay = 0f)
        {
            Vector3 StartPos = toSort[0].Objekt.transform.parent.position;
            yield return new WaitForSeconds(delay);
            int iterations = 1;
            if (isMobileHand)
                yield return SortHand(toSort);
            else
                yield return SortCollection(toSort, maxSize, rowWise, StartPos, iterations);
        }

        private static IEnumerator SortCollection(List<Card> toSort, int maxSize, bool rowWise, Vector3 StartPos, int iterations)
        {
            iterations = maxSize;
            for (int i = 0; i < toSort.Count; i++)
            {
                float offsetX = toSort[i].Objekt.transform.localScale.x;
                float offsetY = toSort[i].Objekt.transform.localScale.y;
                float cardWidth = CardSize * offsetX;
                float cardHeight = CardSize * offsetY;
                float alignY = (cardHeight + offsetY) * ((maxSize - 1) * 0.5f);
                if (rowWise)
                    Global.global.StartCoroutine(toSort[i].Objekt.transform.StandardAnimation(StartPos +
                        new Vector3((i % iterations) * (cardWidth + offsetX), -alignY + (i / iterations) * (cardHeight + offsetY), 0),
                        toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
                else
                    Global.global.StartCoroutine(toSort[i].Objekt.transform.StandardAnimation(StartPos +
                    new Vector3((i / iterations) * (cardWidth + offsetX), -alignY + (i % iterations) * (cardHeight + offsetY), 0),
                    toSort[i].Objekt.transform.rotation.eulerAngles, toSort[i].Objekt.transform.localScale, 1f / toSort.Count, .5f));
                yield return null;
            }
        }

        private static IEnumerator SortHand(List<Card> toSort)
        {
            for (int card = 0; card < toSort.Count; card++)
            {
                GameObject temp = toSort[card].Objekt;
                bool hand1 = temp.transform.parent.name.Contains("1");
                Global.global.StartCoroutine(temp.transform.StandardAnimation(temp.transform.parent.position, new Vector3(0, temp.transform.rotation.eulerAngles.y, hand1 ? 0 : 180), temp.transform.localScale, 0, .3f, () =>
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
                    Global.global.StartCoroutine(temp.transform.parent.StandardAnimation(temp.transform.parent.position + new Vector3(0, 0, -id), temp.transform.parent.eulerAngles + new Vector3(0, 0, -60f + (120f / max) * (max - id)), temp.transform.parent.localScale, .6f, .3f, () =>
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
    }
}