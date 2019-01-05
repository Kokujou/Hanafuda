using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public partial class ParticleAnimation
    {
        public static partial class LoadingAnimations
        {
            public class Explode : AnimationBase
            {
                public Explode() : base()
                {
                }

                public override int _GridSize { get { return 32; } }
                public override bool _HasUndoAnimation { get { return true; } }

                public override void Animate()
                {
                    for (int particle = 0; particle < _Size; particle++)
                    {
                        Vector3 start = StartPos[particles[particle].randomSeed];
                        Vector3 target = TargetPos[particles[particle].randomSeed];
                        float finished = (float)watch.Elapsed.TotalSeconds / _Duration;
                        particles[particle].remainingLifetime = particles[particle].randomSeed * 1000;
                        particles[particle].position = Vector3.Lerp(start, target, Mathf.Pow(finished, 1f / _Exponent));
                        particles[particle].rotation3D = Vector3.Lerp(Vector3.zero, TargetRot[particles[particle].randomSeed], finished);
                    }
                    OldSystem.SetParticles(particles, _Size);
                    NextSystem.SetParticles(particles, _Size);
                    float alpha = ((float)watch.Elapsed.TotalSeconds / _Duration) / 2f;
                    var CBS = OldSystem.colorBySpeed;
                    CBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 1f - alpha));
                    var sCBS = NextSystem.colorBySpeed;
                    sCBS.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha));
                    if (watch.Elapsed.TotalSeconds > _Duration) watch.Stop();
                }

                public override Vector3 GetPosition(int index)
                {
                    float angle = Random.Range(0, _SprayAngle);
                    float r;
                    Vector3 dir = particles[index].position.normalized;
                    Vector3 direction;
                    if (dir == Vector3.zero)
                        direction = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
                    else
                    {
                        float h = Random.Range(1f, 2.000f);
                        r = Mathf.Tan(angle * Mathf.Deg2Rad) * h;

                        //rotate the point randomly on th cirlce with same radius -> same angle
                        float i = Random.Range(0.0f, 360f);
                        Vector3 newDir = new Vector3(
                            r * Mathf.Cos(Mathf.Deg2Rad * i),
                            h,
                            r * Mathf.Sin(Mathf.Deg2Rad * i));

                        Quaternion rot = Quaternion.LookRotation(dir);
                        //fix rotation cause of start is right instead of top 
                        rot.eulerAngles -= new Vector3(-90, 0, 0);

                        direction = rot * newDir;
                    }
                    return particles[index].position + direction.normalized * Random.Range(_GridSize / 10f, _GridSize);
                }

            }
        }
    }
}