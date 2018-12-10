using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hanafuda;


namespace ExtensionMethods
{
    public static partial class Animation
    {
        /// <summary>
        /// Transparenz-Animation des Hilfs-Pfeils für die mobile Handkarten-Animation
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerator BlinkSlide(this GameObject obj)
        {
            while (true)
            {
                var renderer = obj.GetComponent<SpriteRenderer>();
                while (renderer.color.a == 0)
                    yield return null;
                var alpha = Time.time * 3f % 5f / 10f + 0.5f;
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b,
                    alpha > .75f ? .75f - (alpha - .75f) : alpha);
                yield return null;
            }
        }
        /// <summary>
        ///     Hervorheben (Vergrößern) einer Karte
        /// </summary>
        /// <param name="col">Kollider der zu vergrößernden Karte</param>
        public static void HoverCard(this BoxCollider col)
        {
            var mobile = Camera.main.aspect < 1;
            if (mobile)
            {
                var tempZ = col.gameObject.transform.position.z;
                col.gameObject.transform.Translate(0, 10, 0);
                col.gameObject.transform.position = new Vector3(col.gameObject.transform.position.x,
                    col.gameObject.transform.position.y, tempZ);
            }
            else
            {
                col.gameObject.transform.position -= new Vector3(0, 0, 5);
                col.gameObject.transform.localScale *= 2;
                col.size /= 2;
            }
            Global.prev = col;
        }

        /// <summary>
        ///     Rückgängigmachen von Karten-Hervorhebung
        /// </summary>
        /// <param name="col">Kollider der hervorgehobenen Karte</param>
        public static void UnhoverCard(this BoxCollider col)
        {
            var mobile = Camera.main.aspect < 1;
            if (mobile)
            {
                var tempZ = col.gameObject.transform.position.z;
                col.gameObject.transform.Translate(0, -10, 0);
                col.gameObject.transform.position = new Vector3(col.gameObject.transform.position.x,
                    col.gameObject.transform.position.y, tempZ);
            }
            else
            {
                col.gameObject.transform.position += new Vector3(0, 0, 5);
                col.gameObject.transform.localScale /= 2;
                col.size *= 2;
            }

            if (Global.prev == col) Global.prev = null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="append"></param>
        /// <param name="toInstantiate"></param>
        /// <returns></returns>
        public static IEnumerator KoikoiAnimation(this GameObject toInstantiate, Action append)
        {
            GameObject koi = GameObject.Instantiate(toInstantiate);
            koi.transform.position = new Vector3(0, 0, 0);
            koi.transform.localScale = new Vector3(0, 0, 0);
            while (koi.transform.localScale.x <= 9.6f)
            {
                koi.transform.localScale += new Vector3(9.6f / 50f, 2.4f / 50f, 2.4f / 50f);
                yield return null;
            }

            koi.transform.localScale = new Vector3(9.6f, 2.4f, 2.4f);
            yield return new WaitForSeconds(.5f);
            while (koi.transform.localScale.x >= 0)
            {
                koi.transform.localScale -= new Vector3(9.6f / 50f, 2.4f / 50f, 2.4f / 50f);
                yield return null;
            }

            koi.transform.localScale = new Vector3(0, 0, 0);
            GameObject.Destroy(koi);
            append();
        }
        /// <summary>
        ///     Standardmäßige Animation von Objekten durch Interpolation
        /// </summary>
        /// <param name="obj">Zielobjekt</param>
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
    }
}