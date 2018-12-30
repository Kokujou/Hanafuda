using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

/*
 * Todo:
 * - Bei Explosion Überblendung der Sprites einbauen für weichere Übergänge
 */

namespace Hanafuda
{
    [RequireComponent(typeof(Image))]
    public class SpriteAnimations : MonoBehaviour
    {
        /// <summary>
        /// Länge einer Animation
        /// </summary>
        private const float Duration = 1f;
        /// <summary>
        /// Splitter-Blockgröße der Icons, 2^x.
        /// </summary>
        private const int SplitSize = 2;
        /// <summary>
        /// Einheitliche Größe der Icon-Texturen
        /// </summary>
        private const int IconSize = 128;
        private static readonly int blockSize = (int)Mathf.Pow(2, SplitSize);
        private static readonly int length = (IconSize / blockSize);

        public List<Sprite> Items;

        private Image Active;
        private List<Func<Sprite, IEnumerator>> Animations;
        private bool AnimationRunning = false;
        private int SubanimationsRunning = 0;
        private void Awake()
        {
            Animations = new List<Func<Sprite, IEnumerator>>() {
                Scale, FadeIn, Explode, Implode, GrowFromGround, Glow, Waterdrop, Fog, PixelMix };
            Animations = new List<Func<Sprite, IEnumerator>>() { Scale, FadeIn, Explode, Implode, GrowFromGround, PixelMix };
            Active = gameObject.GetComponent<Image>();
            StartCoroutine(Coordinate());
        }

        private IEnumerator Coordinate()
        {
            StartCoroutine(Scale(Active.sprite));
            int lastItem = -1, lastAnimation = -1;
            while (true)
            {
                yield return null;
                if (AnimationRunning) continue;
                yield return new WaitForSeconds(1);
                int nextAnimation, nextItem;
                do { nextItem = Random.Range(0, Items.Count); }
                while (nextItem == lastItem);
                do { nextAnimation = Random.Range(0, Animations.Count); }
                while (nextAnimation == lastAnimation);
                StartCoroutine(Animations[nextAnimation](Items[nextItem]));
                lastAnimation = nextAnimation;
                lastItem = nextItem;
            }
        }

        private List<KeyValuePair<Vector2, Transform>> SplitImage(Texture2D target)
        {
            List<KeyValuePair<Vector2, Transform>> fragments = new List<KeyValuePair<Vector2, Transform>>();
            GameObject parent = new GameObject("Fragments");
            parent.SetActive(false);
            parent.transform.SetParent(Active.transform.parent, false);
            parent.AddComponent<RectTransform>().localPosition = Vector3.zero;
            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    GameObject fragObject = new GameObject("Fragment");
                    Image fragImg = fragObject.AddComponent<Image>();
                    fragObject.transform.SetParent(parent.transform, false);
                    fragImg.rectTransform.sizeDelta = Vector2.one * blockSize;
                    fragObject.transform.localPosition = new Vector3(x * blockSize, y * blockSize, 0) - Vector3.one * IconSize * 0.5f;
                    fragImg.rectTransform.pivot = Vector2.zero;
                    fragImg.sprite = Sprite.Create(target, new Rect(x * blockSize, y * blockSize, blockSize, blockSize), Vector2.one * 0.5f);
                    fragments.Add(new KeyValuePair<Vector2, Transform>(new Vector2(x, y), fragImg.transform));
                }
            }
            parent.SetActive(true);
            return fragments;
        }
        private IEnumerator ShootFragment(Transform fragment)
        {
            SubanimationsRunning++;
            Vector3 startPos = fragment.localPosition;
            Vector3 startRot = fragment.rotation.eulerAngles;
            Vector3 targetPos = (startPos).normalized * Random.Range(0, 200);
            Vector3 targetRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            while (watch.Elapsed.TotalSeconds <= Duration)
            {
                float percFinished = (float)watch.Elapsed.TotalSeconds / Duration;
                fragment.localPosition = Vector3.Lerp(startPos, targetPos, Mathf.Pow(percFinished, 1f / 2f));
                fragment.rotation = Quaternion.Euler(Vector3.Lerp(startRot, targetRot, (float)watch.Elapsed.TotalSeconds / Duration));
                yield return null;
            }
            SubanimationsRunning--;
        }
        private IEnumerator DrawBackFragment(Transform fragment, Vector2 origin, bool randomRotation = false)
        {
            SubanimationsRunning++;
            Vector3 startPos = fragment.localPosition;
            Vector3 startRot = fragment.rotation.eulerAngles;
            Vector3 targetPos = origin * blockSize - Vector2.one * IconSize * 0.5f;
            Vector3 targetRot;
            if (randomRotation)
                targetRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            else
                targetRot = Vector3.zero;
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            while (watch.Elapsed.TotalSeconds <= Duration)
            {
                float percFinished = (float)watch.Elapsed.TotalSeconds / Duration;
                fragment.localPosition = Vector3.Lerp(startPos, targetPos, percFinished);
                fragment.rotation = Quaternion.Euler(Vector3.Lerp(startRot, targetRot, (float)watch.Elapsed.TotalSeconds / Duration));
                yield return null;
            }
            fragment.localPosition = targetPos;
            fragment.rotation = Quaternion.Euler(targetRot);
            SubanimationsRunning--;
        }
        private IEnumerator GrowFromGround(Sprite next)
        {
            StartCoroutine(FragmentAnimation(next, fragment => DrawBackFragment(fragment,
                new Vector2((fragment.localPosition.x + IconSize * 0.5f) / blockSize, 0), true)));
            yield return null;
        }
        private IEnumerator Explode(Sprite next)
        {
            StartCoroutine(FragmentAnimation(next, ShootFragment));
            yield return null;
        }
        private IEnumerator Implode(Sprite next)
        {
            StartCoroutine(FragmentAnimation(next, fragment => DrawBackFragment(fragment, Vector2.one * (length / 2f), true)));
            yield return null;
        }
        private IEnumerator FragmentAnimation(Sprite next, Func<Transform, IEnumerator> DoAnimation)
        {
            AnimationRunning = true;
            Texture2D current = Active.sprite.texture;
            List<KeyValuePair<Vector2, Transform>> fragments = SplitImage(current);
            GameObject parent = fragments[0].Value.parent.gameObject;
            Active.enabled = false;
            for (int frag = 0; frag < fragments.Count; frag++)
            {
                StartCoroutine(DoAnimation(fragments[frag].Value));
            }
            while (SubanimationsRunning > 0) yield return null;
            yield return new WaitForSeconds(.2f);
            for (int frag = 0; frag < fragments.Count; frag++)
            {
                Image fragImg = fragments[frag].Value.GetComponent<Image>();
                fragImg.sprite = Sprite.Create(next.texture, new Rect(fragments[frag].Key.x * blockSize, fragments[frag].Key.y * blockSize, blockSize, blockSize), Vector2.one * blockSize / 2f);
                StartCoroutine(DrawBackFragment(fragments[frag].Value, fragments[frag].Key));
            }
            while (SubanimationsRunning > 0) yield return null;
            Destroy(parent.gameObject);
            Active.sprite = next;
            Active.enabled = true;
            AnimationRunning = false;
        }

        private IEnumerator Glow(Sprite next)
        {
            yield return null;
        }
        private IEnumerator PixelMix(Sprite next)
        {
            AnimationRunning = true;
            Texture2D current = Active.sprite.texture;
            List<KeyValuePair<Vector2, Transform>> oldFragments = SplitImage(current);
            List<KeyValuePair<Vector2, Transform>> newFragments = SplitImage(next.texture);
            GameObject parent1 = oldFragments[0].Value.parent.gameObject;
            GameObject parent2 = newFragments[0].Value.parent.gameObject;
            float waitPerPixel = Duration / oldFragments.Count;
            Active.enabled = false;
            Active.sprite = next;
            while (oldFragments.Count > 0)
            {
                int toReplace = Random.Range(0, oldFragments.Count);
                newFragments[toReplace].Value.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                StartCoroutine(Fade(oldFragments[toReplace].Value.GetComponent<Image>(),
                    newFragments[toReplace].Value.GetComponent<Image>(), Random.Range(0f, Duration * 2), true,
                    (int)oldFragments[toReplace].Key.x, (int)oldFragments[toReplace].Key.y));
                newFragments.RemoveAt(toReplace);
                oldFragments.RemoveAt(toReplace);
            }
            while (SubanimationsRunning > 0) yield return null;
            Destroy(parent1);
            Destroy(parent2);
            Active.enabled = true;
            AnimationRunning = false;
        }

        private IEnumerator Waterdrop(Sprite next)
        {
            yield return null;
        }

        private IEnumerator Fog(Sprite next)
        {
            yield return null;
        }

        private Color GetAverageColor(Color[] list)
        {
            List<Color> List = new List<Color>(list);

            float r = 0;
            float g = 0;
            float b = 0;
            float a = 0;
            for (int i = List.Count - 1; i >= 0; i--)
            {
                if (List[i].a <= 0.1f)
                {
                    r += .25f;
                    b += .25f;
                    g += .25f;
                    continue;
                }
                r += List[i].r * List[i].r;

                g += List[i].g * List[i].g;

                b += List[i].b * List[i].b;

            }

            return new Color(Mathf.Sqrt(r / List.Count), Mathf.Sqrt(g / List.Count), Mathf.Sqrt(b / List.Count), 1);
        }
        private IEnumerator Fade(Image oldImage, Image newImage, float wait = 0f, bool mixColors = false, int x = -1, int y = -1)
        {
            SubanimationsRunning++;
            yield return new WaitForSeconds(wait);
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            Color oldColor = new Color(), newColor = new Color();
            if (mixColors)
            {
                oldColor = GetAverageColor(oldImage.sprite.texture.GetPixels(x * blockSize, y * blockSize, blockSize, blockSize));
                newColor = GetAverageColor(newImage.sprite.texture.GetPixels());
            }
            while (watch.Elapsed.TotalSeconds <= Duration)
            {
                float alpha = (float)watch.Elapsed.TotalSeconds / Duration;
                if (mixColors)
                {
                    Color oldToNew = Color.Lerp(Color.white, newColor, alpha);
                    Color newToOld = Color.Lerp(oldColor, Color.white, alpha);
                    oldImage.color = new Color(oldToNew.r, oldToNew.g, oldToNew.b, 1 - alpha);
                    newImage.color = new Color(newToOld.r, newToOld.g, newToOld.b, alpha);
                }
                else
                {
                    oldImage.color = new Color(1, 1, 1, 1 - alpha);
                    newImage.color = new Color(1, 1, 1, alpha);
                }

                yield return null;
            }
            oldImage.sprite = newImage.sprite;
            oldImage.color = Color.white;
            SubanimationsRunning--;
        }
        private IEnumerator FadeIn(Sprite next)
        {
            AnimationRunning = true;
            GameObject overlay = new GameObject("Overlay");
            Image imgOverlay = overlay.AddComponent<Image>();
            imgOverlay.sprite = next;
            imgOverlay.color = new Color(1, 1, 1, 0);
            overlay.transform.SetParent(Active.transform.parent);
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localScale = Vector3.one;
            imgOverlay.rectTransform.sizeDelta = Vector2.one * 128;
            StartCoroutine(Fade(Active, imgOverlay));
            while (SubanimationsRunning > 0) yield return null;
            Destroy(overlay);
            AnimationRunning = false;
            yield return null;
        }

        private IEnumerator Scale(Sprite next)
        {
            AnimationRunning = true;
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            while (watch.Elapsed.TotalSeconds < Duration && transform.localScale.x != 0)
            {
                transform.localScale = (1f - (float)((watch.Elapsed.TotalSeconds) / (Duration))) * Vector3.one;
                yield return null;
            }
            watch.Restart();
            Active.sprite = next;
            while (watch.Elapsed.TotalSeconds < Duration)
            {
                transform.localScale = ((float)(watch.Elapsed.TotalSeconds / (Duration))) * Vector3.one;
                yield return null;
            }
            transform.localScale = Vector3.one;
            AnimationRunning = false;
        }
    }
}