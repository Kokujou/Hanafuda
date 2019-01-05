using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Stopwatch = System.Diagnostics.Stopwatch;
using Particle = UnityEngine.ParticleSystem.Particle;
using AnimationBase = Hanafuda.ParticleAnimation.LoadingAnimations.AnimationBase;

namespace Hanafuda
{
    public partial class ParticleAnimation : MonoBehaviour
    {
        public List<Material> Images;

        const float _Scale = 2f;
        const float _Duration = 1f;
        const float _ToNextAnimation = 1f;
        const float _Exponent = 3f;
        const float _SprayAngle = 45f;

        bool AwakeCalled;
        bool PixelRestored;
        bool SkipNext;

        public ParticleSystem OldSystem, NewSystem;

        List<AnimationBase> Animations;
         AnimationBase ActiveAnimation;

        Action LoadingAnimation;
        Material oldMat, newMat;

        private void Start()
        {
            //LoadingAnimations = new List<Func<Dictionary<uint, Vector3>>>() { () => ShootFragments(), };
            LoadingAnimations.NextSystem = NewSystem;
            LoadingAnimations.OldSystem = OldSystem;
            Animations = new List<AnimationBase>()
            {
                new LoadingAnimations.FadeIn(),
                new LoadingAnimations.GrowFromGround(),
                new LoadingAnimations.Explode(),
                new LoadingAnimations.Implode()
                //*/
            };
            PixelRestored = true;
            AwakeCalled = true;
            oldMat = Images.GetRandom();
            ActiveAnimation = Animations.GetRandom();
            ActiveAnimation.ResetParticleSystem(oldMat, Images.GetRandom());
            ActiveAnimation.Animate();
            StartCoroutine(Coordinate());
        }

        private IEnumerator Coordinate()
        {
            while (true)
            {
                if (!ActiveAnimation.watch.IsRunning)
                {
                    if (PixelRestored || !ActiveAnimation._HasUndoAnimation)
                    {
                        PixelRestored = true;
                        float start = Time.timeSinceLevelLoad;
                        ActiveAnimation = Animations.GetRandom(x => x == ActiveAnimation);
                        if (newMat)
                            oldMat = newMat;
                        newMat = Images.GetRandom(x => x == oldMat);
                        ActiveAnimation.ResetParticleSystem(oldMat, newMat);
                        LoadingAnimation = () => LoadingAnimation = ActiveAnimation.Animate;
                        yield return new WaitForSeconds(_ToNextAnimation - (Time.timeSinceLevelLoad - start));
                        
                    }
                    else
                    {
                        if (ActiveAnimation._HasUndoAnimation)
                            LoadingAnimation = ActiveAnimation.UndoAnimation;
                    }
                    PixelRestored = !PixelRestored;
                    ActiveAnimation.watch.Restart();
                }
                yield return null;
            }
        }

        private void Update()
        {
            if (!AwakeCalled ) return;
            LoadingAnimation();
        }

    }
}