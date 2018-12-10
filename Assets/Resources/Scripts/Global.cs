using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
// ReSharper disable All

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        public static int MovingCards;
        public static Sprite[] CardSkins;
        public static int Turn = -1;
        public static Font JFont;
        public static BoxCollider prev;
        public static List<Card> allCards = new List<Card>();
        public static List<Yaku> allYaku = new List<Yaku>();
        public static List<string> Spielverlauf = new List<string>();
        public static List<Player> players = new List<Player>();
        public Font jFont;

        /// <summary>
        ///     Generierung einer 1x1 Textur, die nur aus einer Farbe besteht
        /// </summary>
        /// <param name="color">Farbe der Textur</param>
        /// <returns></returns>
        public static Texture2D ColorTex(Color color)
        {
            var result = new Texture2D(1, 1);
            result.SetPixel(0, 0, color);
            result.Apply();
            return result;
        }

        public static IEnumerator BlinkSlide(GameObject obj)
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
        public static void HoverCard(BoxCollider col)
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

            prev = col;
        }

        /// <summary>
        ///     Rückgängigmachen von Karten-Hervorhebung
        /// </summary>
        /// <param name="col">Kollider der hervorgehobenen Karte</param>
        public static void UnhoverCard(BoxCollider col)
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

            if (prev == col) prev = null;
        }

        public static IEnumerator KoikoiAnimation(Action append, GameObject toInstantiate)
        {
            var koi = Instantiate(toInstantiate);
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
            Destroy(koi);
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
        public static IEnumerator StandardAnimation(Transform obj, Vector3 destPos, Vector3 destRot, Vector3 destScale,
            float delay = 0f, float duration = 1f, params Action[] AddFunc)
        {
            //destPos.z = obj.position.z;
            MovingCards++;
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
            MovingCards--;
        }

        public static void SetCameraRect(Camera cam)
        {
            /*float targetaspect = 16.0f / 9.0f;
            float windowaspect = (float)Screen.width / (float)Screen.height;
            float scaleheight = windowaspect / targetaspect;
            if (scaleheight < 1.0f)
            {
                Rect rect = cam.rect;

                rect.width = 1.0f;
                rect.height = scaleheight;
                rect.x = 0;
                rect.y = (1.0f - scaleheight) / 2.0f;

                cam.rect = rect;
            }
            else // add pillarbox
            {
                float scalewidth = 1.0f / scaleheight;

                Rect rect = cam.rect;

                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;

                cam.rect = rect;
            }*/
            if (Screen.width >= Screen.height)
                cam.aspect = 16f / 9f;
            else
                cam.aspect = .6f;
        }

        /// <summary>
        ///     Harte Wertinitialisierung von Karten und Yaku
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(this);
            JFont = jFont;
            allCards.Add(new Card(Card.Monate.Januar, Card.Typen.Landschaft, "Pinie #1"));
            allCards.Add(new Card(Card.Monate.Januar, Card.Typen.Landschaft, "Pinie #2"));
            allCards.Add(new Card(Card.Monate.Januar, Card.Typen.Bänder, "Pinie mit Spruch-Band"));
            allCards.Add(new Card(Card.Monate.Januar, Card.Typen.Lichter, "Kranich unter der Sonne"));
            allCards.Add(new Card(Card.Monate.Febraur, Card.Typen.Landschaft, "Pflaumenblüten #1"));
            allCards.Add(new Card(Card.Monate.Febraur, Card.Typen.Landschaft, "Pflaumenblüten #2"));
            allCards.Add(new Card(Card.Monate.Febraur, Card.Typen.Bänder, "Pflaumenblüten mit Spruch-Band"));
            allCards.Add(new Card(Card.Monate.Febraur, Card.Typen.Tiere, "Buschträllerer"));
            allCards.Add(new Card(Card.Monate.März, Card.Typen.Landschaft, "Kirschblüten #1"));
            allCards.Add(new Card(Card.Monate.März, Card.Typen.Landschaft, "Kirschblüten #2"));
            allCards.Add(new Card(Card.Monate.März, Card.Typen.Bänder, "Kirschblüten mit Spruch-Band"));
            allCards.Add(new Card(Card.Monate.März, Card.Typen.Lichter, "Kirschblüten mit Decke"));
            allCards.Add(new Card(Card.Monate.April, Card.Typen.Landschaft, "Blauregen #1"));
            allCards.Add(new Card(Card.Monate.April, Card.Typen.Landschaft, "Blauregen #2"));
            allCards.Add(new Card(Card.Monate.April, Card.Typen.Bänder, "Blauregen mit Band"));
            allCards.Add(new Card(Card.Monate.April, Card.Typen.Tiere, "Kuckuck"));
            allCards.Add(new Card(Card.Monate.Mai, Card.Typen.Landschaft, "Schwertlilien #1"));
            allCards.Add(new Card(Card.Monate.Mai, Card.Typen.Landschaft, "Schwertlilien #2"));
            allCards.Add(new Card(Card.Monate.Mai, Card.Typen.Bänder, "Schwertlilien mit Band"));
            allCards.Add(new Card(Card.Monate.Mai, Card.Typen.Tiere, "Schwertlilien mit Brücke"));
            allCards.Add(new Card(Card.Monate.Juni, Card.Typen.Landschaft, "Pfingstrose #1"));
            allCards.Add(new Card(Card.Monate.Juni, Card.Typen.Landschaft, "Pfingstrose #2"));
            allCards.Add(new Card(Card.Monate.Juni, Card.Typen.Bänder, "Pfingstrose mit blauem Band"));
            allCards.Add(new Card(Card.Monate.Juni, Card.Typen.Tiere, "Schmetterlinge"));
            allCards.Add(new Card(Card.Monate.Juli, Card.Typen.Landschaft, "Hagi-Strauch #1"));
            allCards.Add(new Card(Card.Monate.Juli, Card.Typen.Landschaft, "Hagi-Strauch #2"));
            allCards.Add(new Card(Card.Monate.Juli, Card.Typen.Bänder, "Hagi-Strauch mit Band"));
            allCards.Add(new Card(Card.Monate.Juli, Card.Typen.Tiere, "Eber"));
            allCards.Add(new Card(Card.Monate.August, Card.Typen.Landschaft, "China-Schilf #1"));
            allCards.Add(new Card(Card.Monate.August, Card.Typen.Landschaft, "China-Schilf #2"));
            allCards.Add(new Card(Card.Monate.August, Card.Typen.Tiere, "Gänse"));
            allCards.Add(new Card(Card.Monate.August, Card.Typen.Lichter, "Vollmond"));
            allCards.Add(new Card(Card.Monate.September, Card.Typen.Landschaft, "Chrysanthemen #1"));
            allCards.Add(new Card(Card.Monate.September, Card.Typen.Landschaft, "Chrysanthemen #2"));
            allCards.Add(new Card(Card.Monate.September, Card.Typen.Bänder, "Chrysanthemen mit blauem Band"));
            allCards.Add(new Card(Card.Monate.September, Card.Typen.Tiere, "Sake-Schale"));
            allCards.Add(new Card(Card.Monate.Oktober, Card.Typen.Landschaft, "Ahorn #1"));
            allCards.Add(new Card(Card.Monate.Oktober, Card.Typen.Landschaft, "Ahorn #2"));
            allCards.Add(new Card(Card.Monate.Oktober, Card.Typen.Bänder, "Ahorn mit blauem Band"));
            allCards.Add(new Card(Card.Monate.Oktober, Card.Typen.Tiere, "Hirsch"));
            allCards.Add(new Card(Card.Monate.November, Card.Typen.Landschaft, "Gewitter"));
            allCards.Add(new Card(Card.Monate.November, Card.Typen.Bänder, "Weide mit Band"));
            allCards.Add(new Card(Card.Monate.November, Card.Typen.Tiere, "Schwalbe"));
            allCards.Add(new Card(Card.Monate.November, Card.Typen.Lichter, "Poet mit Regenschirm"));
            allCards.Add(new Card(Card.Monate.Dezember, Card.Typen.Landschaft, "Paulownie #1"));
            allCards.Add(new Card(Card.Monate.Dezember, Card.Typen.Landschaft, "Paulownie #2"));
            allCards.Add(new Card(Card.Monate.Dezember, Card.Typen.Landschaft, "Paulownie #3"));
            allCards.Add(new Card(Card.Monate.Dezember, Card.Typen.Lichter, "Chinesischer Phönix"));
            allYaku.Add(new Yaku("Sankou", "三光", new int[2] {1, 0}, 5, 3, Card.Typen.Lichter));
            allYaku.Add(new Yaku("Ameshikou", "雨四光", new int[2] {1, 1}, 7, 4, new List<string> {"Poet mit Regenschirm"},
                Card.Typen.Lichter));
            allYaku.Add(new Yaku("Shikou", "四光", new int[2] {1, -1}, 8, 4, new List<string> {"Poet mit Regenschirm"},
                Card.Typen.Lichter));
            allYaku.Add(new Yaku("Gokou", "五光", new int[2] {1, 0}, 10, 5, Card.Typen.Lichter));
            allYaku.Add(new Yaku("Ino Shika Chou", "猪鹿蝶", new int[2] {0, 1}, 5, 3,
                new List<string> {"Eber", "Hirsch", "Schmetterlinge"}));
            allYaku.Add(new Yaku("Tane", "タネ", new int[2] {1, 0}, 1, 5, 1, Card.Typen.Tiere));
            allYaku.Add(new Yaku("Akatan", "赤短", new int[2] {0, 1}, 5, 3,
                new List<string>
                    {"Pinie mit Spruch-Band", "Kirschblüten mit Spruch-Band", "Pflaumenblüten mit Spruch-Band"}));
            allYaku.Add(new Yaku("Aotan", "青短", new int[2] {0, 1}, 5, 3,
                new List<string>
                    {"Chrysanthemen mit blauem Band", "Ahorn mit blauem Band", "Pfingstrose mit blauem Band"}));
            allYaku.Add(new Yaku("Aka Ao Kasane", "赤青重", new int[2] {0, 1}, 10, 6,
                new List<string>
                {
                    "Pinie mit Spruch-Band", "Pflaumenblüten mit Spruch-Band", "Kirschblüten mit Spruch-Band",
                    "Chrysanthemen mit blauem Band", "Ahorn mit blauem Band", "Pfingstrose mit blauem Band"
                }));
            allYaku.Add(new Yaku("Tanzaku", "短冊", new int[2] {1, 0}, 1, 5, 1, Card.Typen.Bänder));
            allYaku.Add(new Yaku("Tsukimizake", "月見酒", new int[2] {0, 1}, 5, 2,
                new List<string> {"Sake-Schale", "Vollmond"}));
            allYaku.Add(new Yaku("Hanamizake", "花見酒", new int[2] {0, 1}, 5, 2,
                new List<string> {"Sake-Schale", "Kirschblüten mit Decke"}));
            allYaku.Add(new Yaku("Kasu", "カス", new int[2] {1, 0}, 1, 10, 1, Card.Typen.Landschaft));
            Settings.mobile = Camera.main.aspect < 1;
            var skins = Resources.LoadAll<Texture2D>("Images/").Where(x => x.name.StartsWith("Back")).ToArray();
            CardSkins = new Sprite[skins.Length];
            for (var i = 0; i < skins.Length; i++)
                CardSkins[i] = Sprite.Create(skins[0], new Rect(0, 0, skins[i].width, skins[i].height),
                    new Vector2(.5f, .5f));
            prefabCollection = singleton;
        }

        public class Message : MessageBase
        {
            public string message;
        }
    }
}