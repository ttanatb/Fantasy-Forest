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

    public float personalSpace = 1.8f;
    private float personalSpaceSqr;

    private Rigidbody rigidBody;
    private Animator animator;

    protected override void Start()
    {
        base.Start();

        maxForce = 3f;
        maxSpeed = 2f;
        radius = 1f;
        personalSpaceSqr = Mathf.Pow(personalSpace, 2);

        rigidBody = GetComponent<Rigidbody>();
        freezeY = true;
        animator = GetComponent<Animator>();
    }

    protected override void CalcSteringForces()
    {
        position.y = 0;

        if ((position - leader.FollowingPos).sqrMagnitude < 0.9f)
            maxSpeed = Mathf.Lerp(maxSpeed, 0, Time.deltaTime);
        else
        {
            totalForce += Arrive(leader.FollowingPos) * seekingWeight;
            maxSpeed = Mathf.Lerp(maxSpeed, 2, Time.deltaTime);
        }


        if ((leader.Position - position).sqrMagnitude < 3)
            totalForce += Flee(leader.Position) * obstacleWeight;

        totalForce += Separate(leader.Followers) * separationWeight;




        if (totalForce.magnitude < 0.01f)
        {
            totalForce = Vector3.zero;
            animator.SetBool("Walk", false);
        }
        else
        {
            animator.SetBool("Walk", true);
        }
    }

    protected Vector3 Separate(Follower[] followers)
    {
        Vector3 separation = Vector3.zero;

        foreach (Follower follower in followers)
        {
            //checks if this flocker
            if (follower == this) continue;


            //gets the distance
            Vector3 dist = position - follower.transform.position;

            if (Vector3.Dot(dist, transform.forward) > 0)
                continue;

            float distSqrMag = dist.sqrMagnitude;
            if (distSqrMag > 0 && distSqrMag < personalSpaceSqr)
            {
                //if close enough add the opposite
                separation += (dist).normalized * personalSpace;
            }
        }
        return separation.normalized * personalSpace;
    }
}
