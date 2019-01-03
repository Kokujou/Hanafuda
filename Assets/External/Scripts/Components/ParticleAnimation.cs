using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Stopwatch = System.Diagnostics.Stopwatch;
using Particle = UnityEngine.ParticleSystem.Particle;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAnimation : MonoBehaviour
{
    float scale = 2f;
    const float Duration = 2f;
    const float _ToNextAnimation = .5f;
    const float exponent = 3f;

    int _rows;
    int _size;

    bool awakeCalled;
    bool PixelRestored;

    ParticleSystem system;
    Particle[] particles;
    Stopwatch watch = new Stopwatch();

    Dictionary<uint, Vector3> startPos = new Dictionary<uint, Vector3>(),
        targetPos = new Dictionary<uint, Vector3>(), targetRot = new Dictionary<uint, Vector3>();

    List<Func<Dictionary<uint, Vector3>>> LoadingAnimations;

    Action LoadingAnimation;

    enum AnimationMode
    {
        Animate,
        GetPositions,
        GetRotations
    }

    private void Start()
    {
        LoadingAnimations = new List<Func<Dictionary<uint, Vector3>>>() { () => ShootFragments(), };
        system = GetComponent<ParticleSystem>();
        _rows = system.textureSheetAnimation.numTilesX;
        _size = _rows * _rows;
        particles = new Particle[_size];
        gameObject.transform.localScale = (scale / _rows) * Vector3.one;
        PixelRestored = true;
        for (int i = 0; i < _size; i++)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = new Vector3(0, i / _rows, i % _rows) - new Vector3(0, _rows / 2f, _rows / 2f);
            emitParams.startLifetime = _size * 100;
            emitParams.randomSeed = (uint)i;
            startPos.Add((uint)i, emitParams.position);
            targetPos.Add((uint)i, emitParams.position + emitParams.position.normalized * Random.Range(_rows / 10f, _rows));
            targetRot.Add((uint)i, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            system.Emit(emitParams, 1);
        }
        int count = system.GetParticles(particles);
        ShootFragments();
        awakeCalled = true;
        StartCoroutine(Coordinate());
    }

    private void InitAnimation(Dictionary<uint, Vector3> positions, Dictionary<uint, Vector3> rotations)
    {
        targetPos = positions;
        targetRot = rotations;
    }

    private IEnumerator Coordinate()
    {
        while (true)
        {
            if (!watch.IsRunning)
            {
                if (PixelRestored)
                {
                    InitAnimation(ShootFragments(AnimationMode.GetPositions), ShootFragments(AnimationMode.GetRotations));
                    yield return new WaitForSeconds(_ToNextAnimation);
                    LoadingAnimation = () => ShootFragments();
                }
                else
                    LoadingAnimation = DrawbackFragments;
                watch.Restart();
                PixelRestored = !PixelRestored;
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (!awakeCalled || (int)(watch.ElapsedMilliseconds / 50) % 2 != 0 || !watch.IsRunning) return;
        LoadingAnimation();
    }

    private Dictionary<uint, Vector3> ShootFragments(AnimationMode mode = AnimationMode.Animate)
    {
        Dictionary<uint, Vector3> values = new Dictionary<uint, Vector3>();
        for (int particle = 0; particle < _size; particle++)
        {
            if (mode == AnimationMode.GetPositions)
            {
                float angle = 90f;
                float r;
                Vector3 dir = particles[particle].position.normalized;

                float endAngle;
                float h = Random.Range(1f, 2.000f);
                r = Mathf.Tan(angle * Mathf.Deg2Rad) * h;

                //rotate the point randomly on th cirlce with same radius -> same angle
                float i = Random.Range(0.0f, 360f);
                Vector3 newDir = new Vector3(
                    r * Mathf.Cos(Mathf.Deg2Rad * i),
                    h,
                    r * Mathf.Sin(Mathf.Deg2Rad * i));

                Quaternion rot = Quaternion.LookRotation(dir.normalized);
                //fix rotation cause of start is right instead of top 
                rot.eulerAngles -= new Vector3(-90, 0, 0);

                Vector3 direction = rot * newDir;

                values.Add((uint)particle, particles[particle].position + direction.normalized * Random.Range(_rows / 10f, _rows));
            }
            else if (mode == AnimationMode.GetRotations)
                values.Add((uint)particle, new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            else
            {
                Vector3 start = startPos[particles[particle].randomSeed];
                Vector3 target = targetPos[particles[particle].randomSeed];
                float finished = (float)watch.Elapsed.TotalSeconds / Duration;
                particles[particle].remainingLifetime = particles[particle].randomSeed * 100;
                particles[particle].position = Vector3.Lerp(start, target, Mathf.Pow(finished, 1f / exponent));
                particles[particle].rotation3D = Vector3.Lerp(Vector3.zero, targetRot[particles[particle].randomSeed], finished);
            }
        }
        system.SetParticles(particles, _size);
        if (watch.Elapsed.TotalSeconds > Duration) watch.Stop();
        return values;
    }

    private void DrawbackFragments()
    {
        for (int i = 0; i < _size; i++)
        {
            float finished = (float)watch.Elapsed.TotalSeconds / Duration;
            particles[i].remainingLifetime = particles[i].randomSeed * 100;
            particles[i].position = Vector3.Lerp(targetPos[particles[i].randomSeed], startPos[particles[i].randomSeed],
                Mathf.Pow(finished, exponent));
            particles[i].rotation3D = Vector3.Lerp(targetRot[particles[i].randomSeed], Vector3.zero, finished);
        }
        system.SetParticles(particles, _size);
        if (watch.Elapsed.TotalSeconds > Duration) watch.Stop();
    }
}
