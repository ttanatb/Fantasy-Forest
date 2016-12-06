using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The abstract class that stimulate the movement of a car
/// </summary>
public abstract class VehicleMovement : MonoBehaviour
{
    //variables
    protected Vector3 acceleration = Vector3.zero;
    protected Vector3 position = Vector3.zero;
    protected Vector3 velocity = Vector3.one;
    protected Vector3 nextPos;
    protected Vector3 direction;

    protected Vector3 totalForce = Vector3.zero;
    protected float mass = 1f;

    protected float maxSpeed = 10f;
    private float savedMaxSpeed;
    protected float maxForce = 5f;
    protected float radius;
    protected Vector3 posToCenter;

    float seed;
    protected Vector3 minBounds;
    protected Vector3 maxBounds;
    private Vector3 terrainCenter;

    public Material debugMaterial;
    protected Vector3 farNextPos;

    //properties
    public float Radius
    {
        get { return radius; }
    }

    public Vector3 NextPos
    {
        get { return nextPos; }
    }

    // Use this for initialization
    protected virtual void Start()
    {
        //initializes variables
        position = gameObject.transform.position;
        velocity = Vector3.zero;
        seed = Random.Range(0, 1000f); //random seed for perlin noise's wandering
        direction.Set(Random.value, Random.value, Random.value);
        radius = 0.8f;

        if (GetComponent<SphereCollider>())
            posToCenter = position - transform.TransformPoint(GetComponent<SphereCollider>().center);
        else if (GetComponent<CapsuleCollider>())
            posToCenter = position - transform.TransformPoint(GetComponent<CapsuleCollider>().center);
        else if (GetComponent<BoxCollider>())
            posToCenter = position - transform.TransformPoint(GetComponent<BoxCollider>().center);
        else if (GetComponent<CharacterController>())
            posToCenter = position - transform.TransformPoint(GetComponent<CharacterController>().center);

    }

    /// <summary>
    /// Set the max bounds, min bounds, and center
    /// </summary>
    protected void SetBounds(Vector3 terrainMin, Vector3 terrainMax)
    {
        minBounds = terrainMin;
        maxBounds = terrainMax;
        maxBounds.y = minBounds.y;

        terrainCenter = (maxBounds + minBounds) / 2;
        //print("Min Bounds:" + minBounds + "Max Bounds:" + maxBounds + "Center:" + terrainCenter);
    }

    //method to override
    protected abstract void CalcSteringForces();

    // Update is called once per frame
    protected virtual void Update()
    {
        CalcSteringForces();
        ApplyToAcceleration();
        UpdatePosition();
        ApplyToTransform();
    }

    /// <summary>
    /// Applies to acc and resets everything to zero
    /// </summary>
    protected void ApplyToAcceleration()
    {
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);

        acceleration *= 0;
        acceleration = totalForce / mass;

        totalForce *= 0;
    }

    /// <summary>
    /// Applies to the kinematics
    /// </summary>
    void UpdatePosition()
    {
        position = gameObject.transform.position;
        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        position += velocity * Time.deltaTime;
        nextPos = position + transform.forward * 4 * radius / maxSpeed - posToCenter;
        farNextPos = position + transform.forward * maxSpeed - posToCenter;
    }

    /// <summary>
    /// Apply to transform
    /// </summary>
    void ApplyToTransform()
    {
        if (GetComponent<CharacterController>())
            GetComponent<CharacterController>().Move(velocity * Time.deltaTime);
        else if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().MovePosition(position);
        else transform.position = position;

        direction = velocity.normalized;
        if (direction != Vector3.zero)
            //transform.right = direction;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 100);
    }

    /// <summary>
    /// Seeks the target's position
    /// </summary>
    protected Vector3 Seek(Vector3 targetPos)
    {
        return ((targetPos - position).normalized * maxSpeed - velocity);
    }

    /// <summary>
    /// Seeks the target's next position
    /// </summary>
    protected Vector3 Persue(VehicleMovement target)
    {
        return ((target.NextPos - position).normalized * maxSpeed - velocity);
    }

    /// <summary>
    /// Flees from the target's position
    /// </summary>
    protected Vector3 Flee(Vector3 targetPos)
    {
        return ((position - targetPos).normalized * maxSpeed - velocity);
    }

    /// <summary>
    /// Evades from the target's next position
    /// </summary>
    protected Vector3 Evade(VehicleMovement target)
    {
        return ((position - target.NextPos).normalized * maxSpeed - velocity);
    }

    protected Vector3 Arrive(Vector3 targetPos)
    {
        Vector3 dist = targetPos - position;
        if (dist.sqrMagnitude < 2)
        {
            return dist - velocity;
        }
        else return ((targetPos - position).normalized * maxSpeed - velocity);
    }

    /// <summary>
    /// Avoids an obstacle
    /// </summary>
    protected Vector3 AvoidObstacle(Obstacle obstacle)
    {
        //gets the distance
        Vector3 distance = obstacle.Pos - nextPos;
        distance.y = 0;

        //checks if obstacle is in front of vehicle
        if (Vector3.Dot(distance, transform.forward) < 0)
            return Vector3.zero;

        //checks if will intersect obstacle
        if (distance.sqrMagnitude > Mathf.Pow(obstacle.Radius + radius, 2))
            return Vector3.zero;

        //checks if too close to obstacle
        if (distance.sqrMagnitude < Mathf.Pow(radius, 2))
            return Flee(obstacle.Pos);

        //get the dot product with right
        float dotProd = Vector3.Dot(transform.right, distance);

        if (dotProd < 0)
            return transform.right * maxSpeed - velocity;
        else return -transform.right * maxSpeed - velocity;
    }

    /// <summary>
    /// Avoids an obstacle
    /// </summary>
    protected Vector3 AvoidObstacle(Leader leader)
    {
        //gets the distance
        Vector3 distance = leader.position- nextPos;
        distance.y = 0;

        //checks if obstacle is in front of vehicle
        if (Vector3.Dot(distance, transform.forward) < 0)
            return Vector3.zero;

        //checks if will intersect obstacle
        if (distance.sqrMagnitude > Mathf.Pow(leader.Radius + radius, 2))
            return Vector3.zero;

        //checks if too close to obstacle
        if (distance.sqrMagnitude < Mathf.Pow(radius, 2))
            return Flee(leader.position);

        //get the dot product with right
        float dotProd = Vector3.Dot(transform.right, distance);

        if (dotProd < 0)
            return transform.right * maxSpeed - velocity;
        else return -transform.right * maxSpeed - velocity;
    }

    protected Vector3 Wander()
    {
        //calculates wandering variables
        Vector3 wanderCenter = position + direction * 4f;
        float wanderRadius = 3f;
        float angle = Mathf.PerlinNoise(seed + Time.fixedTime, seed) * Mathf.PI * 3f;

        //seeks position to wander to
        return Seek(new Vector3(wanderCenter.x + wanderRadius * Mathf.Cos(angle), position.y, wanderCenter.z + wanderRadius * Mathf.Sin(angle)));
    }


    /// <summary>
    /// Seeks the opposite y or x position to steer inwards
    /// </summary>
    protected Vector3 SteerInBounds(Vector3 min, Vector3 max)
    {
        if (farNextPos.x > max.x || farNextPos.x < min.x
            || farNextPos.z > max.z || farNextPos.z < min.z)
            return Seek(terrainCenter);

        //return steering force
        return Vector3.zero;
    }

    /// <summary>
    /// Steers to center (useful for when stuck in corner)
    /// </summary>
    protected Vector3 SteerVertically(float min, float max)
    {
        //checks with position instead of next position
        if (nextPos.y > max || nextPos.y < min)
        {
            Vector3 seekingPos = position;
            seekingPos.y = (min + max) / 2;
            return Seek(seekingPos);
        }
        else return Vector3.zero;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Bush")
        {
            savedMaxSpeed = maxSpeed;
            maxSpeed *= 0.5f;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Bush")
        {
            maxSpeed = savedMaxSpeed;
        }

    }

    /// <summary>
    /// Method to draw debug lines
    /// </summary>
    protected virtual void OnRenderObject()
    {
        if (debugMaterial)
        {
            //draw forward line
            debugMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            GL.Vertex(position - posToCenter);
            GL.Vertex(nextPos);
            GL.End();
        }


        //debugMaterial.SetPass(0);
        //GL.Begin(GL.LINES);
        //for (int i = 0; i < 100; i++)
        //{
        //    float a = i / (float)100;
        //    float angle = a * Mathf.PI * 2;
        //    GL.Vertex(position - posToCenter);
        //    GL.Vertex(position - posToCenter + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        //}
        //GL.End();
    }
}
