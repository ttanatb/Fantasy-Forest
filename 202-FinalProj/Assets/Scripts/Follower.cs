using UnityEngine;
using System.Collections;
using System;

public class Follower : VehicleMovement
{

    public Leader leader;

    public float seekingWeight = 1;
    public float separationWeight = 1;
    public float obstacleWeight = 1;
    public float boundaryWeight = 1;

    public float personalSpace = 1;
    private float personalSpaceSqr;

    protected override void Start()
    {
        base.Start();

        maxForce = 4f;
        maxSpeed = 2f;
        radius = 1f;
        personalSpaceSqr = Mathf.Pow(personalSpace, 2);
    }

    protected override void CalcSteringForces()
    {
        totalForce += Arrive(leader.FollowingPos) * seekingWeight;

        if ((leader.Position - position).sqrMagnitude < 1)
            totalForce += AvoidObstacle(leader) * obstacleWeight;

        totalForce += Separate(leader.Followers) * separationWeight;
    }

    protected Vector3 Separate(Follower[] followers)
    {
        Vector3 separation = Vector3.zero;

        foreach (Follower follower in followers)
        {
            //checks if this flocker
            if (follower == this) continue;

            //gets the distance
            Vector3 dist = follower.transform.position - position;
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
