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
    private enum State { Flocking, Fleeing };

    private State state = State.Flocking;

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

    private GameObject[] spiders;
    private GameObject targetToFlee;

    public float fleeRadius;
    private float fleeRadiusSqr;

    private Obstacle[] trees;
    private TerrainData terrainData;

    //property
    public bool Flocking
    {
        get
        {
            if (state == State.Flocking) return true;
            else return false;
        }
    }

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
        spiders = manager.Spiders;
        fleeRadiusSqr = Mathf.Pow(fleeRadius, 2);
        minY = 1 + minFlightHeight;
        maxY = 2 + maxFlightHeight;
    }


    protected override void CalcSteringForces()
    {
        switch (state)
        {
            //flocks to a goal
            case State.Flocking:
                totalForce += Seek(manager.CurrentGoal) * seekWeight;

                totalForce += Separate(manager.Flockers) * separationWeight;                        //separate
                totalForce += (manager.AverageAlignment * maxSpeed - velocity) * alignmentWeight;   //align
                totalForce += Seek(manager.AveragePosition) * cohesionWeight;                       //cohesion

                CheckSpiders();
                break;
            //flees from spider
            case State.Fleeing:
                foreach (GameObject spider in spiders)
                {
                    if (spider)
                        totalForce += Flee(spider.transform.position) * seekWeight;
                }
                CheckFarEnough();
                break;
        }

        //avoid trees
        foreach(Obstacle obs in trees)
        {
            totalForce += AvoidObstacle(obs) * obstacleWeight;
        }

        //UpdateYBounds();
        
        //steer vertical bounds
        totalForce += SteerVertically(minY, maxY) * boundaryWeight;

        //clamp
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
    }

    /// <summary>
    /// Check if close enoug 
    /// </summary>
    private void CheckSpiders()
    {
        foreach(GameObject spider in spiders)
        {
            if (spider)
            {
                Vector3 dist = spider.transform.position - transform.position - posToCenter;
                dist.y = 0;

                if (dist.sqrMagnitude < fleeRadiusSqr && dist.magnitude < fleeRadius)
                {
                    state = State.Fleeing;
                    return;
                }
            }
        }
    }
    
    //check if far enough from spider
    private void CheckFarEnough()
    {
        foreach (GameObject spider in spiders)
        {
            if (spider)
            {
                Vector3 dist = spider.transform.position - transform.position - posToCenter;
                dist.y = 0;

                if (dist.sqrMagnitude < fleeRadiusSqr && dist.magnitude < fleeRadius)
                    return;
            }
        }
        state = State.Flocking;
    }

    /*
     * For bumpy heightmaps
    private void UpdateYBounds()
    {
        flightHeight = terrainData.GetHeight((int)(position.x * terrainData.heightmapResolution / terrainData.size.x),
            (int)(position.z * terrainData.heightmapResolution / terrainData.size.z));
        minY = 1 + minFlightHeight;
        maxY = 2 + maxFlightHeight;
    }
    */

    /// <summary>
    /// Separate from other flockers
    /// </summary>
    private Vector3 Separate(Flocker[] flockers)
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
