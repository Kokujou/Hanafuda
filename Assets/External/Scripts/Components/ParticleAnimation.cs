using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAnimation : MonoBehaviour
{
    public float scale = 2f;
    public const float Duration = 2f;

    public int _rows;
    public int _size;

    bool awakeCalled;

    ParticleSystem system;
    ParticleSystem.Particle[] particles;
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    Dictionary<uint, Vector3> startPos = new Dictionary<uint, Vector3>(),
        randomSpeed = new Dictionary<uint, Vector3>(), targetRot = new Dictionary<uint, Vector3>();

    IEnumerator initParticles()
    {
        yield return null;
    }
    void Start()
    {

        system = GetComponent<ParticleSystem>();
        _rows = system.textureSheetAnimation.numTilesX;
        _size = _rows * _rows;
        particles = new ParticleSystem.Particle[_size];
        gameObject.transform.localScale = (scale / _rows) * Vector3.one;
        StartCoroutine(initParticles());
        for (int i = 0; i < _size; i++)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = new Vector3(0, i / _rows, i % _rows) - new Vector3(0, _rows / 2f, _rows / 2f);
            emitParams.startLifetime = _size * 100;
            emitParams.randomSeed = (uint)i;
            randomSpeed.Add((uint)i, new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
            startPos.Add((uint)i, emitParams.position);
            system.Emit(emitParams, 1);
        }
        watch.Restart();
        awakeCalled = true;
    }

    // Update is called once per frame
    void Update()
    {
        int count = system.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            particles[i].remainingLifetime = particles[i].randomSeed * 100;
            if (watch.Elapsed.TotalSeconds < Duration)
            {
                float percFinished = (float)watch.Elapsed.TotalSeconds / (Duration);
                particles[i].velocity = randomSpeed[(uint)particles[i].randomSeed] * (1 - percFinished) * (.3f / (scale / _rows));
                particles[i].angularVelocity3D = particles[i].velocity;
            }
            else if (Vector3.Distance(startPos[(uint)particles[i].randomSeed], particles[i].position) > .3f / (scale / _rows))
            {
                float percFinished = (float)(watch.Elapsed.TotalSeconds - Duration) / Duration;
                particles[i].angularVelocity3D = Vector3.zero;
                particles[i].velocity = -randomSpeed[(uint)particles[i].randomSeed] * (1 - percFinished) * (.3f / (scale / _rows));
                particles[i].angularVelocity3D = particles[i].velocity;
            }
            else
            {
                particles[i].velocity = Vector3.zero;
                particles[i].position = startPos[(uint)particles[i].randomSeed];
            }
        }
        system.SetParticles(particles, count);
    }
}
