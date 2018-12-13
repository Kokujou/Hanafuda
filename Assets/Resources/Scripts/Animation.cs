using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hanafuda;


namespace ExtensionMethods
{
    public static partial class Animations
    {
        private const float minBlinkAlpha = 0.5f;
        private const float maxBlinkAlpha = .75f;
        /// <summary>
        /// Korrigiert den Kamera-Aspekt zu Portrait(mobil) oder Landscape (PC)
        /// </summary>
        public static void SetCameraRect(this Camera cam)
        {
            if (Screen.width >= Screen.height)
                cam.aspect = 16f / 9f;
            else
                cam.aspect = .6f;
        }
        /// <summary>
        /// Generiert eine 1x1 Textur einer Farbe
        /// </summary>
        /// <returns></returns>
        public static Texture2D CreateTexture(this Color color)
        {
            var result = new Texture2D(1, 1);
            result.SetPixel(0, 0, color);
            result.Apply();
            return result;
        }
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
                col.size /= 2;
            }
            if (Global.prev == col) return;
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

    }
}