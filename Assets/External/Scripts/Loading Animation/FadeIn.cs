using UnityEngine;

namespace Hanafuda
{
    public partial class ParticleAnimation
    {
        public static partial class LoadingAnimations
        {
            public class FadeIn : AnimationBase
            {
                public FadeIn() : base()
                {
                }

                public override bool _HasUndoAnimation => false;

                public override int _GridSize => 8;

                public override void Animate()
                {
                    for (int particle = 0; particle < _Size; particle++)
                    {
                        particles[particle].remainingLifetime = particles[particle].randomSeed * 1000;
                    }
                    OldSystem.SetParticles(particles, _Size);
                    NextSystem.SetParticles(particles, _Size);
                    float alpha = ((float)watch.Elapsed.TotalSeconds / _Duration);
                    var CBS = OldSystem.colorBySpeed;
                    CBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 1f - alpha));
                    var sCBS = NextSystem.colorBySpeed;
                    sCBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha));
                    if (watch.Elapsed.TotalSeconds > _Duration) watch.Stop();
                }

                public override Vector3 GetPosition(int index)
                {
                    return particles[index].position;
                }
            }
        }
    }
}