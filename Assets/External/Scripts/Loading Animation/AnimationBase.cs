
using System.Collections.Generic;
using UnityEngine;
using Particle = UnityEngine.ParticleSystem.Particle;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Hanafuda
{
    public partial class ParticleAnimation
    {
        public static partial class LoadingAnimations
        {
            public abstract class AnimationBase
            {
                public abstract bool _HasUndoAnimation { get; }
                public abstract int _GridSize { get; }

                public int _Size { get { return _GridSize * _GridSize; } }

                public Particle[] particles;
                public ParticleSystem OldSystem;
                public ParticleSystem NextSystem;
                public Dictionary<uint, Vector3> StartPos, TargetPos, TargetRot;
                public Stopwatch watch;

                public AnimationBase()
                {
                    OldSystem = LoadingAnimations.OldSystem;
                    NextSystem = LoadingAnimations.NextSystem;
                    StartPos = new Dictionary<uint, Vector3>();
                    TargetPos = new Dictionary<uint, Vector3>();
                    TargetRot = new Dictionary<uint, Vector3>();
                    watch = new Stopwatch();
                    particles = new Particle[_Size];
                }

                public virtual void ResetParticleSystem(Material oldMat, Material newMat)
                {
                    StartPos.Clear();
                    TargetPos.Clear();
                    TargetRot.Clear();
                    watch.Reset();
                    OldSystem.GetComponent<Renderer>().material = oldMat;
                    NextSystem.GetComponent<Renderer>().material = newMat;
                    OldSystem.transform.localScale = (_Scale / _GridSize) * Vector3.one;
                    NextSystem.transform.localScale = (_Scale / _GridSize) * Vector3.one;
                    var oldTSAM = OldSystem.textureSheetAnimation;
                    oldTSAM.numTilesX = _GridSize;
                    oldTSAM.numTilesY = _GridSize;
                    var newTSAM = NextSystem.textureSheetAnimation;
                    newTSAM.numTilesX = _GridSize;
                    newTSAM.numTilesY = _GridSize;
                    var CBS = OldSystem.colorBySpeed;
                    CBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 1));
                    var sCBS = NextSystem.colorBySpeed;
                    sCBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 0));
                    for (int part = 0; part < _Size; part++)
                    {
                        Particle particle = new Particle();
                        particle.startColor = new Color32(255, 255, 255, 255);
                        particle.startSize = 1;
                        particle.position = new Vector3(0, part / _GridSize, part % _GridSize) - new Vector3(0, _GridSize / 2f, _GridSize / 2f);
                        particle.startLifetime = _Size * 1000;
                        particle.randomSeed = (uint)part;
                        StartPos.Add((uint)part, particle.position);
                        particles[part] = particle;
                    }
                    OldSystem.SetParticles(particles, _Size);
                    NextSystem.SetParticles(particles, _Size);
                    TargetPos = GetTargetPositions();
                    TargetRot = GetTargetRotations();
                }

                public virtual void UndoAnimation()
                {
                    for (int i = 0; i < _Size; i++)
                    {
                        float finished = (float)watch.Elapsed.TotalSeconds / _Duration;
                        particles[i].remainingLifetime = particles[i].randomSeed * 1000;
                        particles[i].position = Vector3.Lerp(TargetPos[particles[i].randomSeed], StartPos[particles[i].randomSeed],
                            Mathf.Pow(finished, _Exponent));
                        particles[i].rotation3D = Vector3.Lerp(TargetRot[particles[i].randomSeed], Vector3.zero, finished);
                    }
                    OldSystem.SetParticles(particles, _Size);
                    NextSystem.SetParticles(particles, _Size);
                    float alpha = ((float)watch.Elapsed.TotalSeconds / _Duration) / 2f + .5f;
                    var CBS = OldSystem.colorBySpeed;
                    CBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 1f - alpha));
                    var sCBS = NextSystem.colorBySpeed;
                    sCBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha));
                    if (watch.Elapsed.TotalSeconds > _Duration) watch.Stop();
                }

                public virtual Dictionary<uint, Vector3> GetTargetRotations()
                {
                    Dictionary<uint, Vector3> values = new Dictionary<uint, Vector3>();
                    for (int particle = 0; particle < _Size; particle++)
                        values.Add((uint)particle, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
                    return values;
                }
                public Dictionary<uint, Vector3> GetTargetPositions()
                {
                    Dictionary<uint, Vector3> values = new Dictionary<uint, Vector3>();
                    for (int particle = 0; particle < _Size; particle++)
                    {
                        values.Add((uint)particle, GetPosition(particle));
                    }
                    return values;
                }

                public abstract Vector3 GetPosition(int index);
                public abstract void Animate();
            }
        }
    }
}