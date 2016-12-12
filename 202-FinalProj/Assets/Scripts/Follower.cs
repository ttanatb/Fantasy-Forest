using UnityEngine;

/// <summary>
/// Leader followers
/// </summary>
public class Follower : VehicleMovement
{
    //variables
    private enum State { Follow, Flee, Wander };

    public Leader leader;

    public float seekingWeight = 1;
    public float separationWeight = 1;
    public float obstacleWeight = 1;
    public float boundaryWeight = 1;

    public float personalSpace = 1.8f;
    private float personalSpaceSqr;

    private Rigidbody rigidBody;
    private Animator animator;
    private float savedAnimSpeed;

    private State state = State.Follow;

    protected override void Start()
    {
        maxForce = 8f;
        maxSpeed = 2.75f;

        radius = 1f;
        personalSpaceSqr = Mathf.Pow(personalSpace, 2);

        rigidBody = GetComponent<Rigidbody>();
        freezeY = true;
        animator = GetComponent<Animator>();
        savedAnimSpeed = 1.8f;
        animator.speed = savedAnimSpeed;

        base.Start();
    }

    //steering forces
    protected override void CalcSteringForces()
    {
        //force position
        position.y = 0;

        //changes state based on leader
        if (leader)
        {
            if (leader.Fleeing) state = State.Flee;
            else state = State.Follow;
        }
        else state = State.Wander;

        //states
        switch (state)
        {
            case State.Follow:
                //slows down if too close
                if ((position - leader.FollowingPos).sqrMagnitude < 0.9f)
                    maxSpeed = Mathf.Lerp(maxSpeed, 0, Time.deltaTime);
                else
                {
                    //arrives at leader's behind
                    totalForce += Arrive(leader.FollowingPos) * seekingWeight;
                    maxSpeed = Mathf.Lerp(maxSpeed, savedMaxSpeed, Time.deltaTime);
                }

                //flees leader if too close
                if ((leader.Position - position).sqrMagnitude < 3)
                    totalForce += Flee(leader.Position) * obstacleWeight;

                //stops animation if still
                if (totalForce.magnitude < 0.01f)
                {
                    totalForce = Vector3.zero;
                    animator.SetBool("Walk", false);
                }
                else
                {
                    animator.SetBool("Walk", true);
                }
                break;

            //if leader is dead
            case State.Wander:
                totalForce += Wander();
                break;

            //speeds up if folowed
            case State.Flee:
                maxSpeed = savedMaxSpeed * 1.5f;
                animator.speed = savedAnimSpeed * 1.5f;
                goto case State.Follow;
        }

        //separate from other spiders
        totalForce += Separate(leader.Followers) * separationWeight;
    }

    /// <summary>
    /// Code to seperate from other followers
    /// </summary>
    protected Vector3 Separate(Follower[] followers)
    {
        //zero
        Vector3 separation = Vector3.zero;

        foreach (Follower follower in followers)
        {
            //checks if this flocker
            if (!follower && follower == this) continue;

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
