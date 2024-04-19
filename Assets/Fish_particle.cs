using UnityEngine;

public class Fish_particle : MonoBehaviour
{
    Flock_gen flock;
    public ParticleSystem particleSys;
    ParticleSystem.Particle[] particles;

    public float positionLerpSpeed = 2.0f;
    public float velocityLerpSpeed = 10.0f;
    private Color[] fishColorList = { Color.red, Color.white, Color.yellow };
    private float[] fishSizeList = { 2f, 1f,0.5f };

    void Start()
    {
        flock = GetComponent<Flock_gen>();
    }
    void Update()
    {
        int fishes_Count = flock.fishes.Count;

        ParticleSystem.MainModule particleSystem_mainModule = particleSys.main;
        float particleSystemStartSize = particleSystem_mainModule.startSize.constant;

        if (particleSystem_mainModule.maxParticles < fishes_Count)
            particleSystem_mainModule.maxParticles = fishes_Count;

        int maxParticles = particleSystem_mainModule.maxParticles;

        if (particles == null || particles.Length != maxParticles)
        {
            particles = new ParticleSystem.Particle[maxParticles];
        }

        float deltaTime = Time.deltaTime;

        for (int i = 0; i < fishes_Count; i++)
        {
            Fish fish = flock.fishes[i];


            ParticleSystem.Particle particle = particles[i];


            if (particle.position == Vector3.zero)
            {
                particle.position = fish.position;
            }


            particle.velocity = Vector2.Lerp(particle.velocity, fish.velocity, deltaTime * velocityLerpSpeed);


            particle.position += particle.velocity * deltaTime;
            particle.position = Vector2.Lerp(particle.position, fish.position, deltaTime * positionLerpSpeed);


            particle.startSize = particleSystemStartSize*fishSizeList[fish.type];

            particle.startLifetime = 1.0f;

            particle.startColor = fishColorList[fish.type]; 

            particle.remainingLifetime = 1.0f;

            particles[i] = particle;
        }

        particleSys.SetParticles(particles, fishes_Count);
    }
}
