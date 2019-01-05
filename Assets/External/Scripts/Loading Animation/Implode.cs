using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public partial class ParticleAnimation
    {
        public static partial class LoadingAnimations
        {
            public class Implode : AnimationBase
            {
                public Implode() : base()
                {
                }

                public override bool _HasUndoAnimation => true;

                public override int _GridSize => 16;

                public override void Animate()
                {
                    for (int particle = 0; particle < _Size; particle++)
                    {
                        Vector3 start = StartPos[particles[particle].randomSeed];
                        Vector3 target = TargetPos[particles[particle].randomSeed];
                        float finished = (float)watch.Elapsed.TotalSeconds / _Duration;
                        particles[particle].remainingLifetime = particles[particle].randomSeed * 1000;
                        particles[particle].position = Vector3.Lerp(start, target, Mathf.Pow(finished, _Exponent));
                        particles[particle].rotation3D = Vector3.Lerp(Vector3.zero, TargetRot[particles[particle].randomSeed], finished);
                    }
                    OldSystem.SetParticles(particles, _Size);
                    NextSystem.SetParticles(particles, _Size);
                    if (watch.Elapsed.TotalSeconds > _Duration) watch.Stop();
                }

                public override Vector3 GetPosition(int index)
                {
                    return Vector3.zero;
                }
            }
        }
    }
}