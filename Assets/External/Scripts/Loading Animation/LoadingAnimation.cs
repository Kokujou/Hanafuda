using UnityEngine;
using Particle = UnityEngine.ParticleSystem.Particle;

namespace Hanafuda
{
    public partial class ParticleAnimation
    {
        public static partial class LoadingAnimations
        {
            public static ParticleSystem OldSystem, NextSystem;
            public static void SetPixels(Particle[] particles)
            {
                for (int i = 0; i < particles.Length; i++)
                    particles[i].remainingLifetime = particles[i].randomSeed * 1000;
                OldSystem.SetParticles(particles, particles.Length);
                NextSystem.SetParticles(particles, particles.Length);
            }
        }
    }
}