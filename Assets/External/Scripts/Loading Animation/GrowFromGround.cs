using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public partial class ParticleAnimation
    {
        public static partial class LoadingAnimations
        {
            public class GrowFromGround : AnimationBase
            {
                private const float _SprayOnGroundX = 1f / 4f;
                private const float _SprayOnGroundY = 1f / 10f;

                public GrowFromGround() : base()
                {
                }

                public override bool _HasUndoAnimation => true;

                public override int _GridSize => 32;

                public override void Animate()
                {
                    if (watch.Elapsed.TotalSeconds > _Duration && watch.Elapsed.TotalSeconds <= _Duration + _ToNextAnimation)
                    {
                        float alpha = (((float)watch.Elapsed.TotalSeconds - _Duration) / _ToNextAnimation);
                        var CBS = OldSystem.colorBySpeed;
                        CBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 1f - alpha));
                        var sCBS = NextSystem.colorBySpeed;
                        sCBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha));
                        return;
                    }
                    if (watch.Elapsed.TotalSeconds > _Duration + _ToNextAnimation)
                    {
                        watch.Stop();
                        return;
                    }
                    for (int particle = 0; particle < _Size; particle++)
                    {
                        Vector3 start = StartPos[particles[particle].randomSeed];
                        Vector3 target = TargetPos[particles[particle].randomSeed];
                        float finished = (float)watch.Elapsed.TotalSeconds / _Duration;
                        particles[particle].remainingLifetime = particles[particle].randomSeed * 1000;
                        particles[particle].position = Vector3.Lerp(start, target, finished);
                        particles[particle].rotation3D = Vector3.Lerp(Vector3.zero, TargetRot[particles[particle].randomSeed], finished);
                    }
                    OldSystem.SetParticles(particles, _Size);
                    NextSystem.SetParticles(particles, _Size);
                }

                public override Vector3 GetPosition(int index)
                {
                    return new Vector3(0, -_GridSize + Random.Range(0, _GridSize * _SprayOnGroundY),
                        particles[index].position.z + Random.Range(-_GridSize * _SprayOnGroundX, _GridSize * _SprayOnGroundX));
                }
            }
        }
    }
}