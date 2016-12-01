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
    private Vector3 direction;

    protected Vector3 totalForce = Vector3.zero;
    protected float mass = 1f;

    protected float maxSpeed = 10f;
    protected float maxForce = 5f;
    protected float radius;

    float seed;
    protected Vector3 minBounds;
    protected Vector3 maxBounds;
    private Vector3 center;

    public Material debugMaterial;

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
    }

    /// <summary>
    /// Set the max bounds, min bounds, and center
    /// </summary>
    protected void SetBounds(Vector3 terrainMin, Vector3 terrainMax)
    {
        minBounds = terrainMin;
        maxBounds = terrainMax;

        center = (maxBounds + minBounds) / 2;
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

    /*
    /// <summary>
    /// Avoids an obstacle
    /// </summary>
    protected Vector3 AvoidObstacle(Obstacle obstacle)
    {
        Vector3 avoid = Vector3.zero;

        //gets the distance
        Vector3 distance = obstacle.Pos - nextPos;
        Vector3 actualDistance = obstacle.Pos - position;

        //checks if obstacle is in front of vehicle
        if (Vector3.Dot(obstacle.Pos - position, transform.forward) < 0)
            return avoid;

        //checks if will intersect obstacle
        if (distance.magnitude > obstacle.Radius + radius)
            return avoid;

        //get the dot product with right
        float dotProd = Vector3.Dot(transform.right, actualDistance);

        if (dotProd < 0)
            avoid += transform.right;
        else avoid += -transform.right;

        //get the dot product with up
        dotProd = Vector3.Dot(transform.up, actualDistance);

        if (dotProd < 0)
            avoid += -transform.up;
        else avoid += transform.up;

        return (avoid).normalized * maxSpeed - velocity;
    }
    */



    /// <summary>
    /// Seeks the opposite y or x position to steer inwards
    /// </summary>
    protected Vector3 SteerInBounds(Vector3 min, Vector3 max)
    {
        if (nextPos.x > max.x || nextPos.x < min.x
            || nextPos.y > max.y || nextPos.y < min.y
            || nextPos.z > max.z || nextPos.z < min.z)
            return Seek(center);

        //return steering force
        return Vector3.zero;
    }

    /// <summary>
    /// Steers to center (useful for when stuck in corner)
    /// </summary>
    protected Vector3 SteerToCenter(Vector3 min, Vector3 max)
    {
        //checks with position instead of next position
        if (position.x > max.x || position.x < min.x || position.z < min.z || position.z > max.z)
        {
            return Seek(center);
        }
        else return Vector3.zero;
    }

    /// <summary>
    /// Applies to acc and resets everything to zero
    /// </summary>
    protected void ApplyToAcceleration()
    {
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

        nextPos = position + transform.forward * maxSpeed * 0.7f;
        //halfNextPos = position + transform.forward * maxSpeed * 0.7f;
    }

    /// <summary>
    /// Apply to transform
    /// </summary>
    void ApplyToTransform()
    {
        GetComponent<CharacterController>().Move(velocity * Time.deltaTime);

        direction = velocity.normalized;
        if (direction != Vector3.zero)
            //transform.right = direction;
            transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 100);
    }


    /// <summary>
    /// Method to draw debug lines
    /// </summary>
    protected virtual void OnRenderObject()
    {
        if (Input.GetKey(KeyCode.D))
        {
            //draw forward line
            debugMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);
            GL.Vertex(gameObject.transform.position);
            GL.Vertex(nextPos);
            GL.End();
        }
    }
}
