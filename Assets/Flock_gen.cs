using System.Collections.Generic;
using UnityEngine;
public struct Fish
{
    public Vector2 position;
    public Vector2 velocity;
    public float wanderAngle;
    public int type;
}
public class Flock_gen : MonoBehaviour
{
    public List<Fish> fishes { get; private set; }
    public int init_num = 200;
    public int shark_num = 2;
    [Space]

    [Header("Bounds")]
    public float boundsScale = 50.0f;
    public float boundingForce = 10.0f;
    public Vector2 bounds = Vector2.one;

    [Header("Flock")]
    public float cohesionRadius = 2.0f;
    public float separationRadius = 1.5f;
    public float alignmentRadius = 1.0f;
    [Space]
    public float cohesionForce = 1.0f;
    public float separationForce = 10.0f;
    public float alignmentForce = 5.0f;
    [Space]
    public float seeSharkRadius = 5.0f;
    public float sharkRatio = 2.0f;
    public float sharkCohRadius = 8.0f;

    [Header("Speed")]
    public float velocityScale = 5.0f;
    public float minSpeed = 5.0f;
    public float maxSpeed = 10.0f;
    public float drag = 0.1f;

    [Header("Wander")]
    public float wanderRadius = 1.0f;
    public float wanderDistance = 2.0f;
    [Range(0.0f, 360.0f)]
    public float wanderAngleJitter = 45.0f;
    public float wanderForce = 1.0f;

    [Header("Debug Visuals")]
    public bool debugBounds = false;
    public bool debugRay = false;

    private int maxCount = 500;
    /*-------------------------------鱼群生成---------------------------*/
    void summonFlock()
    {
        fishes = new();
    }
    public void bornFish(int type)
    {
        //位置在(-b,-b)到(b,b)的区间中
        Vector2 randomPosition = new Vector2(Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y)) * (boundsScale / 2.0f);

        //随机数生成[0,1]区间，修改为[-1,1]区间，并进行缩放
        Vector2 randomVelocity = new(Random.value, Random.value);
        randomVelocity = (randomVelocity * 2.0f) - Vector2.one;
        randomVelocity *= velocityScale;

        Fish fish = new()
        {
            position = randomPosition,
            velocity = randomVelocity,
            type = type
         };
         fishes.Add(fish);
    }

    /*-------------------------------边界控制---------------------------*/
    Vector2 bounding(Fish me)
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 lowerBound = origin - bounds * (boundsScale / 2.0f);
        Vector2 upperBound = origin + bounds * (boundsScale / 2.0f);

        Vector2 force = Vector2.zero;

        for (int i = 0; i < 2; ++i)
            if (me.position[i] < lowerBound[i])
                force[i] = boundingForce;
            else if (me.position[i] > upperBound[i])
                force[i] = -boundingForce;

        return force;
    }
    /*-------------------------------Boids模型模拟---------------------------*/
    Vector2 flocking(Fish me,int id) {

        Vector2 force = Vector2.zero;

        Vector2 separation = Vector2.zero;
        Vector2 cohesion = Vector2.zero; 
        Vector2 alignment = Vector2.zero;

        Vector2 per_pos = Vector2.zero;
        Vector2 per_vel = Vector2.zero;
        uint num_pos = 0;
        uint num_vel = 0;

        for (int j = 0; j < fishes.Count; j++)
        {
            if (id == j)
                continue;

            Fish other = fishes[j];

            // Separation.
            float distance = (other.position - me.position).magnitude;
            Vector2 direction = (other.position - me.position) / distance;
            if (me.type > 0 && other.type == 0 && distance < seeSharkRadius)
            {
                separation -= direction * sharkRatio;
            }
            else if (distance < separationRadius && !(me.type == 0 && other.type > 0))
            {
                float ratio = 1.0f - (distance / separationRadius);
                separation -= direction * ratio;
            }

            // Cohesion.

            if (distance < ((me.type > 0)?cohesionRadius:sharkCohRadius)&&other.type > 0 &&(me.type == other.type||me.type==0))
            {
                num_pos++;
                per_pos += other.position;
            }

            // Alignment.

            if (other.type > 0&&me.type>0&& distance < alignmentRadius&&me.type==other.type)
            {
                num_vel++;
                per_vel += other.velocity;
            }
        }    

        if (num_pos > 0)
        {
            per_pos /= num_pos;
            cohesion = (((per_pos - me.position))/(per_pos - me.position).magnitude);
        }
        if (num_vel > 0)
        {
            per_vel /= num_vel;
            alignment = (per_vel - me.velocity);
        }

        force += separation*separationForce + 
                 cohesion*cohesionForce +
                 alignment*alignmentForce;

        return force;
    }
    /*-------------------------------鸟类随机运动---------------------------*/
    Vector2 wander(Fish me)
    {
 
        me.wanderAngle += Random.Range(-Mathf.Deg2Rad * wanderAngleJitter, Mathf.Deg2Rad * wanderAngleJitter);

        Vector2 newXAxis = new Vector2(me.velocity.normalized.y, -me.velocity.normalized.x);

        Vector2 circleOffset = (me.velocity.normalized * Mathf.Cos(me.wanderAngle) + newXAxis * Mathf.Sin(me.wanderAngle) )*wanderRadius;

        Vector2 wanderTarget = me.position + (me.velocity.normalized * wanderDistance);

        wanderTarget += circleOffset;

        Vector2 offsetToTarget = wanderTarget - me.position;

        return offsetToTarget * wanderForce;

    }
    private void Start()
    {
        summonFlock();
        for (int i = 2; i <= init_num / 2 - 1; i++)
            bornFish(2);
        for (int i = 2; i <= init_num / 2 - 1; i++)
            bornFish(1);
        for (int i = 0; i <= shark_num - 1; i++)
            bornFish(0);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&fishes.Count<maxCount)
            bornFish(Random.Range(1, 3));
    }
    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < fishes.Count; i++)
        {
            Vector2 force = Vector2.zero;
            Fish me = fishes[i];
            force += bounding(me) + wander(me) + flocking(me,i);

            me.velocity *= 1.0f - drag * deltaTime;
            me.velocity += force * deltaTime;

            if (me.velocity.magnitude < minSpeed)
            {
                me.velocity = me.velocity.normalized * minSpeed;
            }
            else if (me.velocity.magnitude > maxSpeed)
            {
                me.velocity = me.velocity.normalized * maxSpeed;
            }
            me.position += me.velocity * deltaTime;

            fishes[i] = me;
        }
    }
    void OnDrawGizmos()
    {
        if (debugBounds)
            Gizmos.DrawWireCube(transform.position, bounds * boundsScale);

        if (!Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.gray;

        if(debugRay)
        foreach (Fish f in fishes)
        {

            if (f.velocity != Vector2.zero)
            {
                    Vector2 boidForward = f.velocity.normalized;
                    Gizmos.DrawRay(f.position, boidForward);
            }
        }
    }
}
