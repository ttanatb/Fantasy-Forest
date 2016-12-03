using UnityEngine;
using System.Collections;

/// <summary>
/// A simple flocker that follows the flock 
/// </summary>
public class Flocker : VehicleMovement
{
    //variables
    FlockerManager manager;

    //change to enum
    bool flocking = true;

    public float minFlightHeight = 0.3f;
    public float maxFlightHeight = 3f;
    private float flightHeight;

    private float minY;
    private float maxY;

    //weights
    public float seekWeight = 2f;
    public float obstacleWeight = 10f;
    public float boundaryWeight = 10;

    public float separationWeight = 2;
    public float alignmentWeight = 0.4f;
    public float cohesionWeight = 1;

    public float personalSpace = 1f;
    private float personalSpaceSqr;

    private Obstacle[] trees;
    private TerrainData terrainData;


    /// <summary>
    /// Used like a constructor
    /// </summary>
    public void Initialize(FlockerManager manager, Vector3 min, Vector3 max, TerrainData terrainData)
    {
        this.manager = manager;
        SetBounds(min, max);
        this.terrainData = terrainData;
    }

    protected override void Start()
    {
        base.Start();
        maxForce = 4f;
        maxSpeed = 3f;
        radius = 0.24f;

        personalSpaceSqr = Mathf.Pow(personalSpace, 2);

        Animation anim = GetComponent<Animation>();
        foreach (AnimationState state in anim)
        {
            state.speed = Random.Range(1.3f, 1.8f);
        }

        trees = manager.Obstacles;
    }


    protected override void CalcSteringForces()
    {

        totalForce += Seek(manager.CurrentGoal) * seekWeight;

        foreach(Obstacle obs in trees)
        {
            totalForce += AvoidObstacle(obs) * obstacleWeight;
        }

        UpdateYBounds();
        totalForce += SteerVertically(minY, maxY) * boundaryWeight;

        //calculate flocking forces
        totalForce += Separate(manager.Flockers) * separationWeight;                        //separate
        totalForce += (manager.AverageAlignment * maxSpeed - velocity) * alignmentWeight;   //align
        totalForce += Seek(manager.AveragePosition) * cohesionWeight;                       //cohesion

        //clamps total force
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
    }

    void UpdateYBounds()
    {
        flightHeight = terrainData.GetHeight((int)(position.x * terrainData.heightmapResolution / terrainData.size.x),
            (int)(position.z * terrainData.heightmapResolution / terrainData.size.z));
        minY = flightHeight + minFlightHeight;
        maxY = flightHeight + maxFlightHeight;
    }

    /// <summary>
    /// Separate from other flockers
    /// </summary>
    protected Vector3 Separate(Flocker[] flockers)
    {
        Vector3 separation = Vector3.zero;

        foreach(Flocker flocker in flockers)
        {
            //checks if this flocker
            if (flocker == this) continue;
            
            //gets the distance
            Vector3 dist = flocker.transform.position - position;
            float distSqrMag = dist.sqrMagnitude;
            if (distSqrMag > 0 && distSqrMag < personalSpaceSqr)
            {
                //if close enough add the opposite
                separation += -(dist * (personalSpaceSqr / (personalSpaceSqr - distSqrMag)));
            }
        }
        return separation;
    }
}
