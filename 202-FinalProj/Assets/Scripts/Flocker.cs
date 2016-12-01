using UnityEngine;
using System.Collections;

/// <summary>
/// A simple flocker that follows the flock 
/// </summary>
public class Flocker : VehicleMovement
{
    //variables
    FlockerManager manager;

    bool flocking;

    //weights
    public float seekWeight = 2f;
    public float obstacleWeight = 10f;
    public float boundaryWeight = 10;

    public float separationWeight = 2;
    public float alignmentWeight = 0.4f;
    public float cohesionWeight = 1;

    public float personalSpace = 3f;
    private float personalSpaceSqr;

    /// <summary>
    /// Used like a constructor
    /// </summary>
    public void Initialize(FlockerManager manager, Vector3 min, Vector3 max)
    {
        maxForce = 4f;
        maxSpeed = 3f;
        radius = 1;
        this.manager = manager;
        SetBounds(min, max);
        flocking = true;
        personalSpaceSqr = Mathf.Pow(personalSpace, 2);


        Animation anim = GetComponent<Animation>();
        foreach (AnimationState state in anim)
        {
            state.speed = Random.Range(0.8f, 1.3f);
        }
    }

    protected override void CalcSteringForces()
    {
        totalForce += Seek(manager.CurrentGoal) * seekWeight;

        //calculate flocking forces
        totalForce += Separate(manager.Flockers) * separationWeight;                        //separate
        totalForce += (manager.AverageAlignment * maxSpeed - velocity) * alignmentWeight;   //align
        totalForce += Seek(manager.AveragePosition) * cohesionWeight;                       //cohesion

        //clamps total force
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
    }

    /*
    /// <summary>
    /// Calculates where to steer towards
    /// </summary>
    protected override void CalcSteringForces()
    {
        //seeks the goal and steer in bounds
        totalForce += SteerInBounds(minBounds, maxBounds) * boundaryWeight;

        //search for an obstacle to avoid
        Obstacle obstacleToAvoid = null;
        float highest = float.MaxValue;
        foreach(Obstacle ob in manager.Obstacles)
        {
            //compares with current highest
            if ((position - ob.Pos).sqrMagnitude < highest)
            {
                obstacleToAvoid = ob;
                highest = (position - ob.Pos).sqrMagnitude;
            }
        }

        //avoids the highest 
        if (obstacleToAvoid)
            totalForce += AvoidObstacle(obstacleToAvoid) * obstacleWeight;

        //separates, aligns, and coheres
        totalForce += Separate(manager.Flockers) * separationWeight;
        totalForce += (manager.AverageAlignment * maxSpeed - velocity) * alignmentWeight;
        totalForce += Seek(manager.AveragePosition) * cohesionWeight;

        //clamps
        totalForce = Vector3.ClampMagnitude(totalForce, maxForce);
    }
    */

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
            if (dist.sqrMagnitude > 0 && dist.sqrMagnitude < personalSpaceSqr)
            {
                //if close enough add the opposite
                separation += -dist;
            }
        }

        return separation;
        /*
        //returns normalize or zero vector
        if (separation == Vector3.zero)
            return Vector3.zero;
        else return separation.normalized;
        */
    }
}
